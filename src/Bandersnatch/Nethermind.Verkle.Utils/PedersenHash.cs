using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Int256;
using Nethermind.Verkle.Curve;

namespace Nethermind.Verkle.Utils
{
    public static class PedersenHash
    {

        public static byte[] Hash(UInt256[] inputElements)
        {
            int inputLength = inputElements.Length;
            FrE[] pedersenVec = new FrE[1 + 2 * inputLength];
            pedersenVec[0] = FrE.SetElement((ulong)(2 + 256 * inputLength * 32));

            for (int i = 0; i < inputElements.Length; i++)
            {
                pedersenVec[2 * i + 1] = FrE.SetElement(inputElements[i].u0, inputElements[i].u1);
                pedersenVec[2 * i + 2] = FrE.SetElement(inputElements[i].u2, inputElements[i].u3);
            }
            CRS crs = CRS.Instance;

            Banderwagon res = Banderwagon.Identity();
            for (int i = 0; i < pedersenVec.Length; i++)
            {
                res += crs.BasisG[i] * pedersenVec[i];
            }

            return res.ToBytesLittleEndian();
        }
        public static byte[] Hash(ReadOnlySpan<byte> address32, UInt256 treeIndex)
        {
            UInt256 addressUInt256 = new UInt256(address32);

            CRS crs = CRS.Instance;

            Banderwagon res = crs.BasisG[0] * FrE.SetElement(2 + 256 * 64)
             + crs.BasisG[1] * FrE.SetElement(addressUInt256.u0, addressUInt256.u1)
             + crs.BasisG[2] * FrE.SetElement(addressUInt256.u2, addressUInt256.u3)
             + crs.BasisG[3] * FrE.SetElement(treeIndex.u0, treeIndex.u1)
             + crs.BasisG[4] * FrE.SetElement(treeIndex.u2, treeIndex.u3);

            return res.ToBytesLittleEndian();
        }
    }
}
