using Curve;
using Field;
using Nethermind.Int256;

namespace Verkle;

using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;


public class PedersenHash
{

    public static byte[] Hash(UInt256[] inputElements)
    {
        var inputLength = inputElements.Length;
        var pedersenVec = new Fr[1 + 2 * inputLength];
        pedersenVec[0] = new Fr(new UInt256((ulong)(2 + 256 * inputLength * 32)));

        for (int i = 0; i < inputElements.Length; i++)
        {
            pedersenVec[2*i + 1] = new Fr(new UInt256(inputElements[i].u0, inputElements[i].u1));
            pedersenVec[2*i + 2] = new Fr(new UInt256(inputElements[i].u2, inputElements[i].u3));
        }
        CRS crs = CRS.Default();

        Banderwagon res = Banderwagon.Identity();
        for (int i = 0; i < pedersenVec.Length; i++)
        {
            res += crs.BasisG[i] * pedersenVec[i]; 
        }
        
        return res.ToBytes();
    }
    public static byte[] Hash(byte[] address32, UInt256 treeIndex)
    {
        UInt256 addressUInt256 = new UInt256(address32);

        var pedersenVec = new Fr[5];
        pedersenVec[0] = new Fr(new UInt256((2 + 256 * 64)));
        pedersenVec[1] = new Fr(new UInt256(addressUInt256.u0, addressUInt256.u1));
        pedersenVec[2] = new Fr(new UInt256(addressUInt256.u2, addressUInt256.u3));
        pedersenVec[3] = new Fr(new UInt256(treeIndex.u0, treeIndex.u1));
        pedersenVec[4] = new Fr(new UInt256(treeIndex.u2, treeIndex.u3));

        CRS crs = CRS.Default();

        Banderwagon res = crs.BasisG[0] * pedersenVec[0];
        res += crs.BasisG[1] * pedersenVec[1];
        res += crs.BasisG[2] * pedersenVec[2];
        res += crs.BasisG[3] * pedersenVec[3];
        res += crs.BasisG[4] * pedersenVec[4];
        return res.ToBytes();
    }
}