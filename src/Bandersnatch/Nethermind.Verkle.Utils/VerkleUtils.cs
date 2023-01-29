using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Int256;
using Nethermind.Verkle.Curve;

namespace Nethermind.Verkle.Utils
{
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
        private static FrE ValueExistsMarker
        {
            get
            {
                new UInt256(2).Exp(128, out UInt256 res);
                return FrE.SetElement(res.u0, res.u1, res.u2, res.u3);
            }
        }
        public static Span<byte> ToAddress32(ReadOnlySpan<byte> address20)
        {
            Span<byte> address32 = new byte[32];
            Span<byte> x = address32[12..];
            address20.CopyTo(x);
            return address32;
        }

        public static (FrE, FrE) BreakValueInLowHigh(byte[]? value)
        {
            if (value is null) return (FrE.Zero, FrE.Zero);
            if (value.Length != 32) throw new ArgumentException();
            FrE lowFr = FrE.FromBytes(value[..16].Reverse().ToArray()) + ValueExistsMarker;
            FrE highFr = FrE.FromBytes(value[16..].Reverse().ToArray());
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
}
