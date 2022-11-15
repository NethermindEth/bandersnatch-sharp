using Nethermind.Field;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Proofs;

namespace Nethermind.Verkle.Utils;

using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

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
    public MultiProofStruct Proof;
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
