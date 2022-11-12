using Curve;
using Field;
using Proofs;

namespace Verkle;

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