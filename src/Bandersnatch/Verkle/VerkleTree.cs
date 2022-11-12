using Curve;
using Field;

namespace Verkle;

using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class VerkleTree
{
    private readonly MemoryDb _db;
    public Commitment RootNode { get; private set; }

    public VerkleTree()
    {
        _db = new MemoryDb();
        RootNode = new Commitment();
    }
    
    public Banderwagon GetLeafDelta(byte[]? oldValue, byte[] newValue, byte index)
    {
        (Fr newValLow, Fr newValHigh) = VerkleUtils.BreakValueInLowHigh(newValue);
        (Fr oldValLow, Fr oldValHigh) = VerkleUtils.BreakValueInLowHigh(oldValue);

        var posMod128 = index % 128;

        var lowIndex = 2 * posMod128;
        var highIndex = lowIndex + 1;
        
        var deltaLow = Committer.ScalarMul(newValLow - oldValLow, lowIndex);
        var deltaHigh = Committer.ScalarMul(newValHigh - oldValHigh, highIndex);
        return deltaLow + deltaHigh;
    }

    public void Insert(byte[] key, byte[] value)
    {
        _db.LeafTable.TryGetValue(key, out var oldValue);
        Banderwagon leafDeltaCommitment = GetLeafDelta(oldValue, value, key[31]);
        _db.LeafTable[key] = value;
        LeafUpdateDelta leafDelta = new();
        leafDelta.UpdateDelta(leafDeltaCommitment, key[31]);
        UpdateTreeCommitments(key, leafDelta);
    }

    public void InsertStemBatch(byte[] stem, Dictionary<byte, byte[]> leafIndexValueMap)
    {
        LeafUpdateDelta leafDelta = new();
        byte[] key = new byte[32];
        stem.CopyTo(key,0);
        foreach (var indexVal in leafIndexValueMap)
        {
            key[31] = indexVal.Key;
            _db.LeafTable.TryGetValue(key, out var oldValue);
            Banderwagon leafDeltaCommitment = GetLeafDelta(oldValue, indexVal.Value, indexVal.Key);
            leafDelta.UpdateDelta(leafDeltaCommitment, indexVal.Key);
            _db.LeafTable[key] = indexVal.Value;
        }
        UpdateTreeCommitments(key, leafDelta);
    }
    
    private void UpdateTreeCommitments(byte[] key, LeafUpdateDelta leafUpdateDelta)
    {
        // calculate this by update the leafs and calculating the delta - simple enough
        TraverseContext context = new TraverseContext(key, leafUpdateDelta);
        Banderwagon rootDelta = TraverseBranch(context);
        RootNode.AddPoint(rootDelta);
    }
    
    private Banderwagon TraverseBranch(TraverseContext traverseContext)
    {
        var pathIndex = traverseContext.Key[traverseContext.CurrentIndex];
        var absolutePath = traverseContext.Key[..(traverseContext.CurrentIndex + 1)].ToArray();
        InternalNode? child = GetBranchChild(absolutePath);
        
        if (child is null)
        {
            // 1. create new suffix node 
            // 2. update the C1 or C2 - we already know the leafDelta - traverseContext.LeafUpdateDelta
            // 3. update ExtensionCommitment
            // 4. get the delta for commitment - ExtensionCommitment - 0;
            (Fr deltaHash, Commitment? suffixCommitment) = UpdateSuffixNode(traverseContext.Stem.ToArray(), traverseContext.LeafUpdateDelta,
                traverseContext.Key[31], true);
            
            // 1. Add internal.stem node
            // 2. return delta from ExtensionCommitment
            Banderwagon point = Committer.ScalarMul(deltaHash, pathIndex);
            _db.BranchTable[absolutePath] = new InternalNode(traverseContext.Stem.ToArray(), suffixCommitment);
            return point;
        }

        Banderwagon parentDeltaHash;
        Banderwagon deltaPoint;
        if (child.Value.IsBranchNode)
        {
            traverseContext.CurrentIndex += 1;
            parentDeltaHash = TraverseBranch(traverseContext);
            traverseContext.CurrentIndex -= 1;
            Fr deltaHash = child.Value.UpdateCommitment(parentDeltaHash);
            _db.BranchTable[absolutePath] = child.Value;
            deltaPoint = Committer.ScalarMul(deltaHash, pathIndex);
        }
        else
        {
            traverseContext.CurrentIndex += 1;
            (parentDeltaHash, bool changeStemToBranch) = TraverseStem(child, traverseContext);
            traverseContext.CurrentIndex -= 1;
            if (changeStemToBranch)
            {
                var newChild = new InternalNode(true);
                // since this is a new child, this would be just the parentDeltaHash.PointToField
                // now since there was a node before and that value is deleted - we need to subtract
                // that from the delta as well
                Fr deltaHash = newChild.UpdateCommitment(parentDeltaHash);
                deltaHash = deltaHash - child.Value.InternalCommitment.PointAsField;
                _db.BranchTable[absolutePath] = newChild;
                deltaPoint = Committer.ScalarMul(deltaHash, pathIndex);
            }
            else
            {
                Fr deltaHash = child.Value.UpdateCommitment(parentDeltaHash);
                _db.BranchTable[absolutePath] = child.Value;
                deltaPoint = Committer.ScalarMul(deltaHash, pathIndex);
            }
        }
        return deltaPoint;
    }
    
     private (Banderwagon, bool) TraverseStem(IVerkleNode node, TraverseContext traverseContext)
    {
        // replace this node.key by node.Stem
        (List<byte> sharedPath, byte? pathDiffIndexOld, byte? pathDiffIndexNew) =
            VerkleUtils.GetPathDifference(node.Key, traverseContext.Key[..31].ToArray());

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
            // 3. set the existing suffix as the child - get the commitement point
            // 4. update the internal node with the two commitment points
            (Fr deltaHash, Commitment? suffixCommitment) = UpdateSuffixNode(traverseContext.Stem.ToArray(), traverseContext.LeafUpdateDelta,
                traverseContext.Key[31], true);
            
            // creating the stem node for the new suffix node 
            _db.BranchTable[sharedPath.ToArray().Concat(new[] {newLeafIndex}).ToArray()] =
                new InternalNode(traverseContext.Stem.ToArray(), suffixCommitment);
            Banderwagon newSuffixCommitmentDelta = Committer.ScalarMul(deltaHash, newLeafIndex);
            
            
            // instead on declaring new node here - use the node that is input in the function
            
            _db.BranchTable[sharedPath.ToArray().Concat(new[] {oldLeafIndex}).ToArray()] =
                new InternalNode(traverseContext.Stem.ToArray(), new Commitment());

            _db.StemTable.TryGetValue(node.Key, out var oldSuffixNode);
            Banderwagon oldSuffixCommitmentDelta =
                Committer.ScalarMul(oldSuffixNode.ExtensionCommitment.PointAsField, oldLeafIndex);

            Banderwagon deltaCommitment = oldSuffixCommitmentDelta + newSuffixCommitmentDelta;

            var resultDelta = FillSpaceWithInternalBranchNodes(sharedPath.ToArray(), relativePathLength, deltaCommitment);
            
            return (resultDelta, true);
        }

        _db.StemTable.TryGetValue(traverseContext.Key[..31].ToArray(), out var oldValue);
        var deltaFr = oldValue.UpdateCommitment(traverseContext.LeafUpdateDelta, traverseContext.Key[31]);
        _db.StemTable[traverseContext.Key[..31].ToArray()] = oldValue;
        
        return (Committer.ScalarMul(deltaFr, traverseContext.Key[traverseContext.CurrentIndex]), false);
    }
     
     public Banderwagon FillSpaceWithInternalBranchNodes(byte[] path, int length, Banderwagon deltaPoint)
     {
         for (int i = 0; i < length; i++)
         {
             var newInternalNode = new InternalNode(true);
             var upwardsDelta = newInternalNode.UpdateCommitment(deltaPoint);
             _db.BranchTable[path[..(path.Length - i)]] = newInternalNode;
             deltaPoint = Committer.ScalarMul(upwardsDelta, path[path.Length - i - 1]);
         }

         return deltaPoint;
     }

     public InternalNode? GetBranchChild(byte[] pathWithIndex)
    {
        return _db.BranchTable.TryGetValue(pathWithIndex, out var child) ? child : null;
    }
    
    public (Fr, Commitment?) UpdateSuffixNode(byte[] stemKey, LeafUpdateDelta leafUpdateDelta, byte suffixLeafIndex, bool insertNew = false)
    {
        Suffix oldNode;
        if (insertNew) oldNode = new Suffix(stemKey);
        else _db.StemTable.TryGetValue(stemKey, out oldNode);
        
        Fr deltaFr = oldNode.UpdateCommitment(leafUpdateDelta, suffixLeafIndex);
        _db.StemTable[stemKey] = oldNode;
        // add the init commitment, because while calculating diff, we subtract the initCommitment in new nodes.
        return insertNew ? (deltaFr + oldNode.InitCommitmentHash, oldNode.ExtensionCommitment.Dup()) : (deltaFr, null);
    }
    
    private ref struct TraverseContext
    {
        public readonly LeafUpdateDelta LeafUpdateDelta;
        public Span<byte> Key { get; }
        public Span<byte> Stem => Key[..31];
        public int CurrentIndex { get; set; }
        
        public TraverseContext(
            Span<byte> key,
            LeafUpdateDelta delta)
        {
            Key = key;
            CurrentIndex = 0;
            LeafUpdateDelta = delta;
        }
    }
}