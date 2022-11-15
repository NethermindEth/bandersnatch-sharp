using Nethermind.Field;
using Nethermind.Int256;
using Nethermind.Verkle.Curve;

namespace Nethermind.Verkle.Utils;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public struct LeafUpdateDelta
{
    public Banderwagon? DeltaC1 { get; private set; }
    public Banderwagon? DeltaC2 { get; private set; }

    public LeafUpdateDelta()
    {
        DeltaC1 = null;
        DeltaC2 = null;
    }

    public void UpdateDelta(Banderwagon deltaLeafCommitment, byte index)
    {
        if (index < 128)
        {
            if (DeltaC1 is null) DeltaC1 = deltaLeafCommitment;
            else DeltaC1 += deltaLeafCommitment;
        }
        else
        {
            if (DeltaC2 is null) DeltaC2 = deltaLeafCommitment;
            else DeltaC2 += deltaLeafCommitment;
        }
    }
}

public static class VerkleUtils
{
    private static Fr ValueExistsMarker
    {
        get
        {
            new UInt256(2).Exp(128, out UInt256 res);
            return new Fr(res);
        }
    }
    public static byte[] ToAddress32(byte[] address20)
    {
        byte[] address32 = new byte[32];
        address20.CopyTo(address32, 0);
        return address32;
    }

    public static (Fr, Fr) BreakValueInLowHigh(byte[]? value)
    {
        if (value is null) return (Fr.Zero, Fr.Zero);
        if (value.Length != 32) throw new ArgumentException();
        Fr lowFr = (Fr.FromBytes(value[..16].Reverse().ToArray()) ?? throw new ArgumentException()) + ValueExistsMarker;
        Fr highFr = Fr.FromBytes(value[16..].Reverse().ToArray()) ?? throw new AggregateException();
        return (lowFr, highFr);
    }

    public static (List<byte>, byte?, byte?) GetPathDifference(IEnumerable<byte> existingNodeKey, IEnumerable<byte> newNodeKey)
    {
        List<byte> samePathIndices = new List<byte>();
        foreach ((byte first, byte second) in existingNodeKey.Zip(newNodeKey))
        {
            if (first != second) return (samePathIndices, first, second);
            samePathIndices.Add(first);
        }
        return (samePathIndices, null, null);
    }

}
