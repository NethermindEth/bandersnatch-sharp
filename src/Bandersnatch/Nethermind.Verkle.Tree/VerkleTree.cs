using System.Runtime.CompilerServices;
using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Db;
using Nethermind.Verkle.Tree.VerkleNodes;
using Nethermind.Verkle.Utils;

namespace Nethermind.Verkle.Tree
{
    public class VerkleTree
    {
        private readonly IVerkleStore _stateDb;

        public VerkleTree(DbMode dbMode, string? dbPath)
        {
            _stateDb = new VerkleStateStore(dbMode, dbPath);
            _stateDb.InitRootHash();
        }
        public byte[] RootHash => _stateDb.GetBranch(Array.Empty<byte>())?._internalCommitment.PointAsField.ToBytes().ToArray() ?? throw new InvalidOperationException();

        private static Banderwagon GetLeafDelta(byte[]? oldValue, byte[] newValue, byte index)
        {
            (FrE newValLow, FrE newValHigh) = VerkleUtils.BreakValueInLowHigh(newValue);
            (FrE oldValLow, FrE oldValHigh) = VerkleUtils.BreakValueInLowHigh(oldValue);

            int posMod128 = index % 128;
            int lowIndex = 2 * posMod128;
            int highIndex = lowIndex + 1;

            Banderwagon deltaLow = Committer.ScalarMul(newValLow - oldValLow, lowIndex);
            Banderwagon deltaHigh = Committer.ScalarMul(newValHigh - oldValHigh, highIndex);
            return deltaLow + deltaHigh;
        }

        public void Insert(Span<byte> key, byte[] value)
        {
            LeafUpdateDelta leafDelta = UpdateLeaf(key, value);
            UpdateTreeCommitments(key[..31], leafDelta);
        }

        public IEnumerable<byte>? Get(byte[] key)
        {
            return _stateDb.GetLeaf(key);
        }

        public void InsertStemBatch(Span<byte> stem, Dictionary<byte, byte[]> leafIndexValueMap)
        {
            LeafUpdateDelta leafDelta = UpdateLeaf(stem, leafIndexValueMap);
            UpdateTreeCommitments(stem, leafDelta);
        }

        private void UpdateTreeCommitments(Span<byte> stem, LeafUpdateDelta leafUpdateDelta)
        {
            // calculate this by update the leafs and calculating the delta - simple enough
            TraverseContext context = new TraverseContext(stem, leafUpdateDelta);
            Banderwagon rootDelta = TraverseBranch(context);
            UpdateRootNode(rootDelta);
        }

        private void UpdateRootNode(Banderwagon rootDelta)
        {
            InternalNode root = _stateDb.GetBranch(Array.Empty<byte>()) ?? throw new ArgumentException();
            root._internalCommitment.AddPoint(rootDelta);
        }

        private Banderwagon TraverseBranch(TraverseContext traverseContext)
        {
            byte pathIndex = traverseContext.Stem[traverseContext.CurrentIndex];
            byte[] absolutePath = traverseContext.Stem[..(traverseContext.CurrentIndex + 1)].ToArray();

            InternalNode? child = GetBranchChild(absolutePath);
            if (child is null)
            {
                // 1. create new suffix node
                // 2. update the C1 or C2 - we already know the leafDelta - traverseContext.LeafUpdateDelta
                // 3. update ExtensionCommitment
                // 4. get the delta for commitment - ExtensionCommitment - 0;
                (FrE deltaHash, Commitment? suffixCommitment) = UpdateSuffixNode(traverseContext.Stem.ToArray(), traverseContext.LeafUpdateDelta, true);

                // 1. Add internal.stem node
                // 2. return delta from ExtensionCommitment
                Banderwagon point = Committer.ScalarMul(deltaHash, pathIndex);
                _stateDb.SetBranch(absolutePath, new StemNode(traverseContext.Stem.ToArray(), suffixCommitment!));
                return point;
            }

            Banderwagon parentDeltaHash;
            Banderwagon deltaPoint;
            if (child.IsBranchNode)
            {
                traverseContext.CurrentIndex += 1;
                parentDeltaHash = TraverseBranch(traverseContext);
                traverseContext.CurrentIndex -= 1;
                FrE deltaHash = child.UpdateCommitment(parentDeltaHash);
                _stateDb.SetBranch(absolutePath, child);
                deltaPoint = Committer.ScalarMul(deltaHash, pathIndex);
            }
            else
            {
                traverseContext.CurrentIndex += 1;
                (parentDeltaHash, bool changeStemToBranch) = TraverseStem((StemNode)child, traverseContext);
                traverseContext.CurrentIndex -= 1;
                if (changeStemToBranch)
                {
                    BranchNode newChild = new BranchNode();
                    newChild._internalCommitment.AddPoint(child._internalCommitment.Point);
                    // since this is a new child, this would be just the parentDeltaHash.PointToField
                    // now since there was a node before and that value is deleted - we need to subtract
                    // that from the delta as well
                    FrE deltaHash = newChild.UpdateCommitment(parentDeltaHash);
                    _stateDb.SetBranch(absolutePath, newChild);
                    deltaPoint = Committer.ScalarMul(deltaHash, pathIndex);
                }
                else
                {
                    // in case of stem, no need to update the child commitment - because this commitment is the suffix commitment
                    // pass on the update to upper level
                    _stateDb.SetBranch(absolutePath, child);
                    deltaPoint = parentDeltaHash;
                }
            }
            return deltaPoint;
        }

        private (Banderwagon, bool) TraverseStem(StemNode node, TraverseContext traverseContext)
        {
            (List<byte> sharedPath, byte? pathDiffIndexOld, byte? pathDiffIndexNew) =
                VerkleUtils.GetPathDifference(node.Stem, traverseContext.Stem.ToArray());

            if (sharedPath.Count != 31)
            {
                int relativePathLength = sharedPath.Count - traverseContext.CurrentIndex;
                // byte[] relativeSharedPath = sharedPath.ToArray()[traverseContext.CurrentIndex..].ToArray();
                byte oldLeafIndex = pathDiffIndexOld ?? throw new ArgumentException();
                byte newLeafIndex = pathDiffIndexNew ?? throw new ArgumentException();
                // node share a path but not the complete stem.

                // the internal node will be denoted by their sharedPath
                // 1. create SuffixNode for the traverseContext.Key - get the delta of the commitment
                // 2. set this suffix as child node of the BranchNode - get the commitment point
                // 3. set the existing suffix as the child - get the commitment point
                // 4. update the internal node with the two commitment points
                (FrE deltaHash, Commitment? suffixCommitment) = UpdateSuffixNode(traverseContext.Stem.ToArray(), traverseContext.LeafUpdateDelta, true);

                // creating the stem node for the new suffix node
                _stateDb.SetBranch(sharedPath.ToArray().Concat(new[]
                {
                    newLeafIndex
                }).ToArray(), new StemNode(traverseContext.Stem.ToArray(), suffixCommitment!));
                Banderwagon newSuffixCommitmentDelta = Committer.ScalarMul(deltaHash, newLeafIndex);


                // instead on declaring new node here - use the node that is input in the function
                SuffixTree oldSuffixNode = _stateDb.GetStem(node.Stem) ?? throw new ArgumentException();
                _stateDb.SetBranch(sharedPath.ToArray().Concat(new[]
                    {
                        oldLeafIndex
                    }).ToArray(),
                    new StemNode(node.Stem, oldSuffixNode.ExtensionCommitment));

                Banderwagon oldSuffixCommitmentDelta =
                    Committer.ScalarMul(oldSuffixNode.ExtensionCommitment.PointAsField, oldLeafIndex);

                Banderwagon deltaCommitment = oldSuffixCommitmentDelta + newSuffixCommitmentDelta;

                Banderwagon internalCommitment = FillSpaceWithInternalBranchNodes(sharedPath.ToArray(), relativePathLength, deltaCommitment);

                return (internalCommitment - oldSuffixNode.ExtensionCommitment.Point, true);
            }

            SuffixTree oldValue = _stateDb.GetStem(traverseContext.Stem.ToArray()) ?? throw new ArgumentException();
            FrE deltaFr = oldValue.UpdateCommitment(traverseContext.LeafUpdateDelta);
            _stateDb.SetStem(traverseContext.Stem.ToArray(), oldValue);

            return (Committer.ScalarMul(deltaFr, traverseContext.Stem[traverseContext.CurrentIndex - 1]), false);
        }

        private Banderwagon FillSpaceWithInternalBranchNodes(byte[] path, int length, Banderwagon deltaPoint)
        {
            for (int i = 0; i < length; i++)
            {
                BranchNode newInternalNode = new BranchNode();
                FrE upwardsDelta = newInternalNode.UpdateCommitment(deltaPoint);
                _stateDb.SetBranch(path[..^i], newInternalNode);
                deltaPoint = Committer.ScalarMul(upwardsDelta, path[path.Length - i - 1]);
            }

            return deltaPoint;
        }

        private InternalNode? GetBranchChild(byte[] pathWithIndex)
        {
            return _stateDb.GetBranch(pathWithIndex);
        }

        private (FrE, Commitment?) UpdateSuffixNode(byte[] stemKey, LeafUpdateDelta leafUpdateDelta, bool insertNew = false)
        {
            SuffixTree oldNode = insertNew ? new SuffixTree(stemKey) : _stateDb.GetStem(stemKey) ?? throw new ArgumentException();

            FrE deltaFr = oldNode.UpdateCommitment(leafUpdateDelta);
            _stateDb.SetStem(stemKey, oldNode);
            // add the init commitment, because while calculating diff, we subtract the initCommitment in new nodes.
            return insertNew ? (deltaFr + oldNode.InitCommitmentHash, oldNode.ExtensionCommitment.Dup()) : (deltaFr, null);
        }

        private LeafUpdateDelta UpdateLeaf(Span<byte> key, byte[] value)
        {
            LeafUpdateDelta leafDelta = new LeafUpdateDelta();
            leafDelta.UpdateDelta(_updateLeaf(key.ToArray(), value), key[31]);
            return leafDelta;
        }
        private LeafUpdateDelta UpdateLeaf(Span<byte> stem, Dictionary<byte, byte[]> indexValuePairs)
        {
            byte[] key = new byte[32];
            stem.CopyTo(key);
            LeafUpdateDelta leafDelta = new LeafUpdateDelta();
            foreach ((byte index, byte[] value) in indexValuePairs)
            {
                key[31] = index;
                leafDelta.UpdateDelta(_updateLeaf(key, value), key[31]);
            }
            return leafDelta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Banderwagon _updateLeaf(byte[] key, byte[] value)
        {
            byte[]? oldValue = _stateDb.GetLeaf(key);
            Banderwagon leafDeltaCommitment = GetLeafDelta(oldValue, value, key[31]);
            _stateDb.SetLeaf(key, value);
            return leafDeltaCommitment;
        }

        public void Flush(long blockNumber)
        {
            _stateDb.Flush(blockNumber);
        }
        public void ReverseState()
        {
            _stateDb.ReverseState();
        }

        private ref struct TraverseContext
        {
            public LeafUpdateDelta LeafUpdateDelta { get; }
            public Span<byte> Stem { get; }
            public int CurrentIndex { get; set; }

            public TraverseContext(Span<byte> stem, LeafUpdateDelta delta)
            {
                Stem = stem;
                CurrentIndex = 0;
                LeafUpdateDelta = delta;
            }
        }
    }
}
