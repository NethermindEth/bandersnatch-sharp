using Nethermind.Int256;
using FE=Nethermind.Field.Montgomery.FrEElement.FrE;

namespace Nethermind.Field.Montgomery.FrEElement
{
    public readonly partial struct FrE
    {
        internal const int Limbs = 4;
        internal const int Bits = 253;
        internal const int Bytes = Limbs * 8;
        internal const ulong SqrtR = 5;
        internal const ulong QInvNeg = 17410672245482742751;

        public static readonly FE Zero = new FE(0);

        internal const ulong One0 = 6347764673676886264;
        internal const ulong One1 = 253265890806062196;
        internal const ulong One2 = 11064306276430008312;
        internal const ulong One3 = 1739710354780652911;
        public static readonly FE One = new FE(One0, One1, One2, One3);

        internal const ulong Q0 = 8429901452645165025;
        internal const ulong Q1 = 18415085837358793841;
        internal const ulong Q2 = 922804724659942912;
        internal const ulong Q3 = 2088379214866112338;
        public static readonly FE qElement = new FE(Q0, Q1, Q2, Q3);

        internal const ulong R0 = 15831548891076708299;
        internal const ulong R1 = 4682191799977818424;
        internal const ulong R2 = 12294384630081346794;
        internal const ulong R3 = 785759240370973821;
        internal static readonly FE rSquare = new FE(R0, R1, R2, R3);

        internal const ulong G0 = 5415081136944170355;
        internal const ulong G1 = 16923187137941795325;
        internal const ulong G2 = 11911047149493888393;
        internal const ulong G3 = 436996551065533341;
        internal static readonly FE gResidue = new FE(G0, G1, G2, G3);

        internal const ulong QM0 = 5415081136944170355;
        internal const ulong QM1 = 16923187137941795325;
        internal const ulong QM2 = 11911047149493888393;
        internal const ulong QM3 = 436996551065533341;
        internal static readonly FE qMinOne = new FE(QM0, QM1, QM2, QM3);

        public static Lazy<UInt256> _modulus = new Lazy<UInt256>(() =>
        {
            UInt256.TryParse("13108968793781547619861935127046491459309155893440570251786403306729687672801", out UInt256 output);
            return output;
        });
        public static Lazy<UInt256> _bLegendreExponentElement = new Lazy<UInt256>(() =>
        {
            UInt256 output = new UInt256(Convert.FromHexString("0e7db4ea6533afa906673b0101343b007fc7c3803a0c8238ba7e835a943b73f0"), true);
            return output;
        });
        public static Lazy<UInt256> _bSqrtExponentElement = new Lazy<UInt256>(() =>
        {
            UInt256 output = new UInt256(Convert.FromHexString("073eda753299d7d483339d80809a1d803fe3e1c01d06411c5d3f41ad4a1db9f"), true);
            return output;
        });
    }
}
