// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using Nethermind.Verkle.Fields.FpEElement;
using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Curve;

public readonly partial struct Banderwagon
{
    public static Banderwagon MultiScalarMulOld(Span<Banderwagon> points, Span<FrE> scalars)
    {
        Banderwagon res = Identity;

        for (int i = 0; i < points.Length; i++)
        {
            res += ScalarMul(points[i], scalars[i]);
        }

        return res;
    }

    public static Banderwagon MultiScalarMul(Span<Banderwagon> points, Span<FrE> scalars)
    {
        int numOfPoints = points.Length;
        FpE[] zs = new FpE[numOfPoints];
        for (int i = 0; i < numOfPoints; i++)
        {
            zs[i] = points[i].Z;
        }

        FpE[] inverses = FpE.MultiInverse(zs);

        AffinePoint[] normalizedPoint = new AffinePoint[numOfPoints];
        for (int i = 0; i < numOfPoints; i++)
        {
            normalizedPoint[i] = points[i].ToAffine(inverses[i]);
        }

        return MultiScalarMulFast(normalizedPoint, scalars.ToArray());
    }


    private static Banderwagon MultiScalarMulFast(AffinePoint[] points, FrE[] scalars)
    {
        int numOfPoints = points.Length;
        int windowsSize = numOfPoints < 32 ? 3 : (int)((Math.Log2(numOfPoints) * 69) / 100) + 2;
        // const int windowsSize = 3;

        int i = 0;
        List<int> windowsStart = new();

        while (i < 253)
        {
            windowsStart.Add(i);
            i += windowsSize;
        }

        Banderwagon zero = Banderwagon.Identity;

        Banderwagon[] windowSums = new Banderwagon[windowsStart.Count];
        Parallel.For(0, windowsStart.Count, w =>
        {
            int winStart = windowsStart[w];

            Banderwagon res = zero;
            Banderwagon[] buckets = new Banderwagon[((ulong)1 << windowsSize) - 1];

            for (int j = 0; j < buckets.Length; j++)
            {
                buckets[j] = zero;
            }

            for (int j = 0; j < points.Length; j++)
            {
                if (scalars[j].IsOne)
                {
                    if (winStart == 0)
                    {
                        res = Banderwagon.Add(res, points[j]);
                    }
                }
                else
                {
                    FrE scalar = scalars[j];
                    FrE.FromMontgomery(in scalar, out scalar);
                    scalar >>= winStart;

                    ulong sc = scalar.u0;
                    sc %= ((ulong)1 << windowsSize);

                    if (sc != 0)
                    {
                        buckets[sc - 1] = Banderwagon.Add(buckets[sc - 1], points[j]);
                    }
                }
            }

            Banderwagon runningSum = Banderwagon.Identity;
            for (int j = (buckets.Length - 1); j >= 0; j--)
            {
                runningSum += buckets[j];
                res += runningSum;
            }

            windowSums[w] = res;
        });

        Banderwagon lowest = windowSums[0];

        Banderwagon result = Banderwagon.Identity;
        for (int j = (windowSums.Length - 1); j > 0; j--)
        {
            result += windowSums[j];
            for (int k = 0; k < windowsSize; k++)
            {
                result = Banderwagon.Double(result);
            }
        }

        result += lowest;

        return result;
    }
}
