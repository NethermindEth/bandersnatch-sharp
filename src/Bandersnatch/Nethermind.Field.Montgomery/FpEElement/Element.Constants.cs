using Nethermind.Int256;
using FE = Nethermind.Field.Montgomery.FpEElement.FpE;

namespace Nethermind.Field.Montgomery.FpEElement
{
    public readonly partial struct FpE
    {
        internal const int Limbs = 4;
        internal const int Bits = 255;
        internal const int Bytes = Limbs * 8;
        internal const ulong SqrtR = 32;
        internal const ulong QInvNeg = 18446744069414584319;

        public static readonly FE Zero = new FE(0);

        internal const ulong One0 = 8589934590;
        internal const ulong One1 = 6378425256633387010;
        internal const ulong One2 = 11064306276430008309;
        internal const ulong One3 = 1739710354780652911;
        public static readonly FE One = new FE(One0, One1, One2, One3);

        internal const ulong Q0 = 18446744069414584321;
        internal const ulong Q1 = 6034159408538082302;
        internal const ulong Q2 = 3691218898639771653;
        internal const ulong Q3 = 8353516859464449352;
        public static readonly FE qElement = new FE(Q0, Q1, Q2, Q3);

        internal const ulong R0 = 14526898881837571181;
        internal const ulong R1 = 3129137299524312099;
        internal const ulong R2 = 419701826671360399;
        internal const ulong R3 = 524908885293268753;
        internal static readonly FE rSquare = new FE(R0, R1, R2, R3);

        internal const ulong G0 = 11289237133041595516;
        internal const ulong G1 = 2081200955273736677;
        internal const ulong G2 = 967625415375836421;
        internal const ulong G3 = 4543825880697944938;
        internal static readonly FE gResidue = new FE(G0, G1, G2, G3);

        internal const ulong QM0 = 9223372034707292161;
        internal const ulong QM1 = 12240451741123816959;
        internal const ulong QM2 = 1845609449319885826;
        internal const ulong QM3 = 4176758429732224676;
        internal static readonly FE qMinOne = new FE(QM0, QM1, QM2, QM3);

        public static Lazy<UInt256> _modulus = new Lazy<UInt256>(() =>
        {
            UInt256.TryParse("52435875175126190479447740508185965837690552500527637822603658699938581184513", out UInt256 output);
            return output;
        });
        public static Lazy<UInt256> _bLegendreExponentElement = new Lazy<UInt256>(() =>
        {
            UInt256 output = new UInt256(Convert.FromHexString("39f6d3a994cebea4199cec0404d0ec02a9ded2017fff2dff7fffffff80000000"), true);
            return output;
        });
        public static Lazy<UInt256> _bSqrtExponentElement = new Lazy<UInt256>(() =>
        {
            UInt256 output = new UInt256(Convert.FromHexString("39f6d3a994cebea4199cec0404d0ec02a9ded2017fff2dff7fffffff"), true);
            return output;
        });
    }
}
