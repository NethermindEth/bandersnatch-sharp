using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Verkle.Proofs;

namespace Nethermind.Verkle.Utils
{
    using Fr = FrE;

    public interface IVerkleTree
    {
        bool Insert(byte[] key, byte[] value);
        byte[] Get(byte[] key);
        Fr RootHash();
        VerkleProof CreateVerkleProof(byte[][] keys);
    }

    public struct VerkleProof
    {
        public VerificationHint VerifyHint;
        public Fr[] CommsSorted;
        public VerkleProofStruct Proof;
    }

    public struct VerificationHint
    {
        public byte[] Depths;
        public ExtPresent[] ExtensionPresent;
        public byte[] DifferentStemNoProof;
    }

    public enum ExtPresent
    {
        None,
        DifferentStem,
        Present
    }
}
