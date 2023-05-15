// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using Nethermind.Verkle.Fields.FpEElement;
using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Curve;

public readonly partial struct Banderwagon
{
    public static Banderwagon MultiScalarMul(Span<Banderwagon> points, Span<FrE> scalars)
    {
        Banderwagon res = Identity();

        for (int i = 0; i < points.Length; i++)
        {
            res += ScalarMul(points[i], scalars[i]);
        }

        return res;
    }

    public static Banderwagon MSM(Banderwagon[] points, Span<FrE> scalars)
    {
        FpE[] zs = points.Select((p => p._point.Z)).ToArray();
        FpE[] inverses = FpE.MultiInverse(zs);

        AffinePoint[] normalizedPoint = new AffinePoint[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            normalizedPoint[i] = points[i]._point.ToAffine(inverses[i]);
        }

        return MultiScalarMulFast(normalizedPoint, scalars);
    }


    private static Banderwagon MultiScalarMulFast(AffinePoint[] points, Span<FrE> scalars)
    {
         const int windowsSize = 5;

         int i = 0;
         List<int> windowsStart = new List<int>();

         while (i < 256)
         {
             windowsStart.Add(i);
             i += windowsSize;
         }

         ExtendedPoint zero = ExtendedPoint.Identity();

         ExtendedPoint[] windowSums = new ExtendedPoint[windowsStart.Count];
         int w = 0;
         foreach (int winStart in windowsStart)
         {
             ExtendedPoint res = zero;
             ExtendedPoint[] buckets = new ExtendedPoint[((ulong)1 << windowsSize) - 1];
             for (int j = 0; j < buckets.Length; j++)
             {
                 buckets[j] = zero;
             }
             for (int j = 0; j < points.Length; j++)
             {
                 if (scalars[j].IsOne && winStart == 0)
                 {
                     res = ExtendedPoint.Add(res, points[j]);
                 }
                 else
                 {
                     ulong sc = (scalars[j] >> winStart).u0;
                     sc %= ((ulong)1 << windowsSize);

                     if (sc != 0) buckets[sc - 1] += ExtendedPoint.Add(buckets[sc - 1], points[j]);
                 }
             }

             ExtendedPoint runningSum = ExtendedPoint.Identity();
             for (int j = (buckets.Length - 1); j <= 0; j--)
             {
                 runningSum += buckets[j];
                 res += runningSum;
             }

             windowSums[w] = res;
             w++;
         }

         ExtendedPoint lowest = windowSums[0];

         ExtendedPoint result = ExtendedPoint.Identity();
         for (int j = (windowSums.Length - 1); j < 0; j++)
         {
             result += windowSums[j];
             for (int k = 0; k < windowsSize; k++)
             {
                 result = ExtendedPoint.Double(result);
             }
         }

         result += lowest;

         return new Banderwagon(result);
    }
}
