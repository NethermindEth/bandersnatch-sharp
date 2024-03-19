using System.Data;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Nethermind.Int256;
using FE = Nethermind.Verkle.Fields.FpEElement.FpE;

namespace Nethermind.Verkle.Fields.FpEElement;

public static class LookUpTable{
    private const ulong RU0 = 17662759726869145328;
    private const ulong RU1 = 5064093798700299342;
    private const ulong RU2 = 10063401387408601010;
    private const ulong RU3 = 3873438938108290097;

    // g is 2^32 th primitive root. it is denoted by dyadicRootOfUnity.
    public static readonly FE dyadicRootofUnity = new(RU0, RU1, RU2, RU3);

    private const ulong g24_0 = 3426179166498458371;
    private const ulong g24_1 = 17774891265778865780;
    private const ulong g24_2 = 289456567831301218;
    private const ulong g24_3 = 1946194709224543838;

    //g^(2^24)
    public static readonly FE g24 = new(g24_0, g24_1, g24_2, g24_3);

    private const ulong g16_0 = 9256342164997675388;
    private const ulong g16_1 = 9412477108735699039;
    private const ulong g16_2 = 18002130140546731823;
    private const ulong g16_3 = 7306109011983698537;

    //g^(2^16)
    public static readonly FE g16 = new(g16_0, g16_1, g16_2, g16_3);

    private const ulong g8_0 = 17088207400005424367;
    private const ulong g8_1 = 2154423591275905172;
    private const ulong g8_2 = 16679520095305972919;
    private const ulong g8_3 = 1422677820602253280;

    //g^(2^8)
    public static readonly FE g8 = new(g8_0, g8_1, g8_2, g8_3);

    private const ulong gInv_0 = 16202019587029671427;
    private const ulong gInv_1 = 12017219221170101976;
    private const ulong gInv_2 = 7842944655549890123;
    private const ulong gInv_3 = 1422657761923336254;

    //inverse of dyadicRootOfUnity, that is inverse of 2^32 th primitive root.
    public static readonly FE gInv = new(gInv_0, gInv_1, gInv_2, gInv_3);

    public static FE[,] GenerateLookUpTable(){
        FE[,] table = new FE[4, 256];
        FE[] gValues = {dyadicRootofUnity, g8, g16, g24};

        for(int i=0;i<4;i++){
            table[i,0] = FE.One;
            for(int j=1;j<256;j++){
                FE.MultiplyMod(gValues[i], table[i,j-1], out table[i,j]);
            }
        }

        return table;
    }

    public static Dictionary<FE, ulong> lookUpTableG2_24(FE[,] arr){
        Dictionary<FE, ulong> map = new Dictionary<FE, ulong>();
        for(ulong i=0;i<256;i++){
            map[arr[3,i]] = i;
        }
        return map;
    }

    public static readonly FE[,] table;
    public static readonly Dictionary<FE, ulong> mapG2_24;
    static LookUpTable(){
        table = GenerateLookUpTable();
        mapG2_24 = lookUpTableG2_24(table);
    }

}
public readonly partial struct FpE{
// , out FE squareRootCandidate, out FE rootOfUnity
    public static FE computeRelevantPowers(in FE z, out FE squareRootCandidate, out FE rootOfUnity){
        void SquareEqNTimes(FE x, out FE y, int n){
            for (int i = 0; i < n; i++){
                MultiplyMod(in x, in x, out x);
            }
            y = x;
        }

        FE z2 = new();
        FE z3 = new();
        FE z7 = new();
        FE z6 = new();
        FE z9 = new();
        FE z11 = new();
        FE z13 = new();
        FE z19 = new();
        FE z21 = new();
        FE z25 = new();
        FE z27 = new();
        FE z29 = new();
        FE z31 = new();
        FE z255 = new();
        FE acc = new();

        MultiplyMod(in z, in z, out z2); // 0b10
        MultiplyMod(in z, in z2, out z3); // 0b11
        MultiplyMod(in z3, in z3, out z6); // 0b110
        MultiplyMod(in z, in z6, out z7);
        MultiplyMod(in z7, in z2, out z9);
        MultiplyMod(in z9, in z2, out z11);
        MultiplyMod(in z11, in z2, out z13);
        MultiplyMod(in z13, in z6, out z19);
        MultiplyMod(in z2, in z19, out z21);
        MultiplyMod(in z19, in z6, out z25);
        MultiplyMod(in z25, in z2, out z27);
        MultiplyMod(in z27, in z2, out z29);
        MultiplyMod(in z29, in z2, out z31);
        MultiplyMod(in z27, in z29, out acc); //56
        MultiplyMod(in acc, in acc, out acc); //112
        MultiplyMod(in acc, in acc, out acc); //224
        MultiplyMod(in acc, in z31, out z255);//255
        MultiplyMod(in acc, in acc, out acc);//448
        MultiplyMod(in acc, in acc, out acc);//896
        MultiplyMod(in acc, in z31, out acc);//927
        SquareEqNTimes(acc, out acc, 6);//59328
        MultiplyMod(in acc, in z27, out acc);//59355
        SquareEqNTimes(acc, out acc, 6);//3798720
        MultiplyMod(in acc, in z19, out acc);//3798739
        SquareEqNTimes(acc, out acc, 5);//121559648
        MultiplyMod(in acc, in z21, out acc);//121559669
        SquareEqNTimes(acc, out acc, 7);
        MultiplyMod(in acc, in z25, out acc);
        SquareEqNTimes(acc, out acc, 6);
        MultiplyMod(in acc, in z19, out acc);
        SquareEqNTimes(acc, out acc, 5);
        MultiplyMod(in acc, in z7, out acc);
        SquareEqNTimes(acc, out acc, 5);
        MultiplyMod(in acc, in z11, out acc);
        SquareEqNTimes(acc, out acc, 5);
        MultiplyMod(in acc, in z29, out acc);
        SquareEqNTimes(acc, out acc, 5);
        MultiplyMod(in acc, in z9, out acc);
        SquareEqNTimes(acc, out acc, 7);
        MultiplyMod(in acc, in z3, out acc);
        SquareEqNTimes(acc, out acc, 7);
        MultiplyMod(in acc, in z25, out acc);
        SquareEqNTimes(acc, out acc, 5);
        MultiplyMod(in acc, in z25, out acc);
        SquareEqNTimes(acc, out acc, 5);
        MultiplyMod(in acc, in z27, out acc);
        SquareEqNTimes(acc, out acc, 8);
        MultiplyMod(in acc, in z, out acc);
        SquareEqNTimes(acc, out acc, 8);
        MultiplyMod(in acc, in z, out acc);
        SquareEqNTimes(acc, out acc, 6);
        MultiplyMod(in acc, in z13, out acc);
        SquareEqNTimes(acc, out acc, 7);
        MultiplyMod(in acc, in z7, out acc);
        SquareEqNTimes(acc, out acc, 3);
        MultiplyMod(in acc, in z3, out acc);
        SquareEqNTimes(acc, out acc, 13);
        MultiplyMod(in acc, in z21, out acc);
        SquareEqNTimes(acc, out acc, 5);
        MultiplyMod(in acc, in z9, out acc);
        SquareEqNTimes(acc, out acc, 5);
        MultiplyMod(in acc, in z27, out acc);
        SquareEqNTimes(acc, out acc, 5);
        MultiplyMod(in acc, in z27, out acc);
        SquareEqNTimes(acc, out acc, 5);
        MultiplyMod(in acc, in z9, out acc);
        SquareEqNTimes(acc, out acc, 10);
        MultiplyMod(in acc, in z, out acc);
        SquareEqNTimes(acc, out acc, 7);
        MultiplyMod(in acc, in z255, out acc);
        SquareEqNTimes(acc, out acc, 8);
        MultiplyMod(in acc, in z255, out acc);
        SquareEqNTimes(acc, out acc, 6);
        MultiplyMod(in acc, in z11, out acc);
        SquareEqNTimes(acc, out acc, 9);
        MultiplyMod(in acc, in z255, out acc);
        SquareEqNTimes(acc, out acc, 2);
        MultiplyMod(in acc, in z, out acc);
        SquareEqNTimes(acc, out acc, 7);
        MultiplyMod(in acc, in z255, out acc);
        SquareEqNTimes(acc, out acc, 8);
        MultiplyMod(in acc, in z255, out acc);
        SquareEqNTimes(acc, out acc, 8);
        MultiplyMod(in acc, in z255, out acc);
        SquareEqNTimes(acc, out acc, 8);
        MultiplyMod(in acc, in z255, out acc);
//acc = n^(Q-1)/2
        MultiplyMod(in acc, in acc, out rootOfUnity);
        MultiplyMod(in rootOfUnity, in z, out rootOfUnity);//n^Q
        MultiplyMod(in acc, in z, out squareRootCandidate);//n^(Q+1)/2

        return acc;
    }

//Given n, whose square root needs to be found, this method returns n^(Q*2^24), n^(Q*2^16), n^(Q*2^8), n^(Q) where p-1 = Q*2^s where s is 32 for this curve. This method takes n^Q as argument.
    public static FE[] powersOfNq(in FE n){
        FE[] arr = new FE[4];
        FE res = n;
        arr[0] = res;

        for(int i=0;i<24;i++){
            FE.MultiplyMod(res, res, out res);
            if(i==7){
                arr[1] = res;
            }else if(i==15){
                arr[2] = res;
            }
        }
        arr[3] = res;
        return arr;
    }

    public static ulong[] decomposeNumber(ulong x){
        ulong y0 = (x >> 24) & 0xFF; // Shift right by 24 bits and mask out the last 8 bits
        ulong y1 = (x >> 16) & 0xFF; // Shift right by 16 bits and mask out the last 8 bits
        ulong y2 = (x >> 8) & 0xFF;  // Shift right by 8 bits and mask out the last 8 bits
        ulong y3 = x & 0xFF;         // Mask out the last 8 bits
        ulong[] arr = {y0, y1, y2, y3}; 
        return arr;
    }

    public static void computePower(ulong x, FE[,] table, out FE res){
        ulong[] arr = decomposeNumber(x);
        // FE.MultiplyMod(lookUpTableG2_24[arr[0]], lookUpTableG2_16[arr[1]], out res);
        MultiplyMod(table[3,arr[0]], table[2,arr[1]], out res);
        // FE.MultiplyMod(res, lookUpTableG2_8[arr[2]], out res);
        MultiplyMod(res, table[1,arr[2]], out res);
        // FE.MultiplyMod(res, lookUpTableG2_0[arr[3]], out res);
        MultiplyMod(res, table[0,arr[3]], out res);
    }

    public static void SqrtNew(in FE n, out FE final){
        computeRelevantPowers(n, out FE squareRootCandidate, out FE rootOfUnity);
        FE[] arrOfN = powersOfNq(rootOfUnity);

        ulong x0, x1, x2, x3;

        FE[,] table = LookUpTable.table;
        Dictionary<FE, ulong> mapG2_24 = LookUpTable.mapG2_24;

        // x3 = dLog(arrOfN[3], lookUpTableG2_24);
        x3 = mapG2_24[arrOfN[3]];

        computePower((((ulong)1<<32)-((ulong)1<<16)*x3), table, out FE secEq);
        MultiplyMod(secEq, arrOfN[2], out secEq);

        // x2 = dLog(secEq, lookUpTableG2_24);
        x2 = mapG2_24[secEq];

        computePower((((ulong)1<<32)-((ulong)1<<16)*x2), table, out FE thirdEq1);
        computePower((((ulong)1<<32)-((ulong)1<<8)*x3), table, out FE thirdEq2);
        MultiplyMod(thirdEq1, thirdEq2, out FE thirdEq);
        MultiplyMod(arrOfN[1], thirdEq, out thirdEq);

        // x1 = dLog(thirdEq, lookUpTableG2_24);
        x1 = mapG2_24[thirdEq];

        computePower((((ulong)1<<32)-((ulong)1<<16)*x1), table, out FE fourthEq1);
        computePower((((ulong)1<<32)-((ulong)1<<8)*x2), table, out FE fourthEq2);
        computePower((((ulong)1<<32)-x3), table, out FE fourthEq3);
        MultiplyMod(fourthEq1, fourthEq2, out FE fourthEq);
        MultiplyMod(fourthEq, fourthEq3, out fourthEq);
        MultiplyMod(arrOfN[0], fourthEq, out fourthEq);

        // x0 = dLog(fourthEq, lookUpTableG2_24);
        x0 = mapG2_24[fourthEq];

        ulong xBy2 = ((ulong)1<<23)*x0 + ((ulong)1<<15)*x1 + ((ulong)1<<7)*x2 + x3/2;
        xBy2 = ((ulong)1<<32) - xBy2;
        computePower(xBy2, table, out final);
        MultiplyMod(final, squareRootCandidate, out final);
    }

    public static void TestSqrt(){
        using IEnumerator<FE> set = GetRandom().GetEnumerator();
        Stopwatch stopwatch = new Stopwatch();
        for (int i = 0; i < 1000; i++){
            FE x = set.Current;
            if (Legendre(x) != 1){
                set.MoveNext();
                continue;
            }

            stopwatch.Restart();
            Sqrt(x, out FE sqrtElem);
            stopwatch.Stop();
            long timeOldAlgoNs = (long)((double)stopwatch.ElapsedTicks / Stopwatch.Frequency * 1_000_000_000);

            Exp(sqrtElem, 2, out FE res);
            if(x.Equals(res)){
                Console.WriteLine($"Time taken by OLD ALGO: {timeOldAlgoNs} ms");
            }else{
                Console.WriteLine("OLD ALGO FAILED");
            }

            stopwatch.Restart();
            SqrtNew(x, out FE sqrtElemImp);
            stopwatch.Stop();
            long timeNewAlgoNs = (long)((double)stopwatch.ElapsedTicks / Stopwatch.Frequency * 1_000_000_000);

            Exp(sqrtElemImp, 2, out FE resImp);
            if(x.Equals(resImp)){
                Console.WriteLine($"Time taken by NEW ALGO: {timeNewAlgoNs} ms");
            }else{
                Console.WriteLine("NEW ALGORITHM FAILED");
            }

            set.MoveNext();
        }
    }

}