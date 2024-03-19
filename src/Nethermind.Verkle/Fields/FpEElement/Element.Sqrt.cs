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

//Given n, whose square root needs to be found, this method returns n^(Q*2^24), n^(Q*2^16), n^(Q*2^8), n^(Q) where p-1 = Q*2^s where s is 32 for this curve.
    public static FE[] powersOfNq(in FE n){
        FE[] arr = new FE[4];

        // w = n^CONST, where CONST=((q-1)/2))
        Exp(in n, _bSqrtExponentElement.Value, out FE w);

        // y = n^((q+1)/2)) = w * n
        MultiplyMod(n, w, out FE y);

        // b = n^q = w * w * n = y * n
        MultiplyMod(w, y, out FE res);

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
        FE[] arrOfN = powersOfNq(n);

        // w = n^CONST, where CONST=((q-1)/2))
        Exp(in n, _bSqrtExponentElement.Value, out FE w);

        // y = n^((q+1)/2)) = w * n
        MultiplyMod(n, w, out FE y);

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
        MultiplyMod(final, y, out final);
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