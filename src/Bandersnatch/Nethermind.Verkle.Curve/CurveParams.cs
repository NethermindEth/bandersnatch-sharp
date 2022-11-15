using Nethermind.Field;
using Nethermind.Int256;

namespace Nethermind.Verkle.Curve;
using Fp = FixedFiniteField<BandersnatchBaseFieldStruct>;

public readonly struct CurveParams
{
    private static readonly byte[] NumY =
    {
        102, 65, 151, 204, 182, 103, 49, 94, 96, 100, 228, 238, 129, 173, 140, 53, 134, 213, 220, 186, 80, 139, 125,
        21, 15, 62, 18, 218, 158, 102, 108, 42
    };
    private static readonly byte[] NumX =
    {
        24, 174, 82, 162, 102, 24, 231, 225, 101, 132, 153, 173, 34, 192, 121, 43, 243, 66, 190, 123, 119, 17, 55,
        116, 197, 52, 11, 44, 204, 50, 193, 41
    };
    private static readonly byte[] Den =
    {
        161, 74, 179, 97, 103, 214, 177, 153, 239, 175, 214, 83, 241, 2, 252, 128, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    };
    private static readonly byte[] Num =
    {
        187, 153, 128, 178, 147, 191, 59, 160, 253, 39, 168, 37, 209, 37, 113, 104, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0
    };

    public static readonly Fp A = new(-5);

    public static readonly Fp YTe = new(new UInt256(NumY));
    public static readonly Fp XTe = new(new UInt256(NumX));

    public static readonly Fp DNum = new(new UInt256(Num));

    public static readonly Fp DDen = Fp.Inverse(new Fp(new UInt256(Den))) ??
                                     throw new Exception("This should not be null");
    public static readonly Fp D = DNum * DDen;
}
