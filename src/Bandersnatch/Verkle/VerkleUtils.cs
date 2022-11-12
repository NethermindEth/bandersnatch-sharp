using Curve;
using Field;
using Nethermind.Int256;

namespace Verkle;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public static class VerkleUtils
{
    private static Fr ValueExistsMarker
    {
        get
        {
            new UInt256(2).Exp(128, out var res);
            return new Fr(res);
        }
    }
    public static byte[] ToAddress32(byte[] address20)
    {
        byte[] address32 = new byte[32];
        address20.CopyTo(address32, 0);
        return address32;
    }
    
    public static (Fr ,Fr) BreakValueInLowHigh(byte[]? value)
    {
        if (value is null) return (Fr.Zero, Fr.Zero);
        if (value.Length != 32) throw new ArgumentException();
        Fr lowFr = (Fr.FromBytes(value[..16].Reverse().ToArray()) ?? throw new ArgumentException()) + ValueExistsMarker;
        Fr highFr = Fr.FromBytes(value[16..].Reverse().ToArray()) ?? throw new AggregateException();
        return (lowFr, highFr);
    }

    public static (List<byte>, byte?, byte?) GetPathDifference(IEnumerable<byte> existingNodeKey, IEnumerable<byte> newNodeKey)
    {
        List<byte> samePathIndices = new();
        foreach (var (first, second) in existingNodeKey.Zip(newNodeKey))
        {
            if (first != second) return (samePathIndices, first, second);
            samePathIndices.Add(first);
        }
        return (samePathIndices, null, null);
    }
    
}