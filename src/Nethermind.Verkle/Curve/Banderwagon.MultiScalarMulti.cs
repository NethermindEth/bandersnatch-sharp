// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using Nethermind.Verkle.Fields.FpEElement;
using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Curve;

public readonly partial struct Banderwagon
{
    public static AffinePoint[] BatchNormalize(in ReadOnlySpan<Banderwagon> points)
    {
        // is it possible to remove this allocation here? by directly passing the banderwagon array?
        int numOfPoints = points.Length;
        FpE[] zs = new FpE[numOfPoints];
        for (int i = 0; i < numOfPoints; i++)
        {
            zs[i] = points[i].Z;
        }

        FpE[] inverses = FpE.MultiInverse(zs);

        AffinePoint[] normalizedPoints = new AffinePoint[numOfPoints];

        for (int i = 0; i < numOfPoints; i++)
        {
            normalizedPoints[i] = points[i].ToAffine(inverses[i]);
        }

        return normalizedPoints;
    }
    public static Banderwagon MultiScalarMul(in ReadOnlySpan<Banderwagon> points, Span<FrE> scalars)
    {
        AffinePoint[] normalizedPoint = BatchNormalize(points);
        return MultiScalarMulFast(normalizedPoint, scalars.ToArray());
    }

    private static FrE[] BatchConvertFromMontgomery(FrE[] scalarsMont)
    {
        FrE[] scalars = new FrE[scalarsMont.Length];
        Parallel.For(0, scalarsMont.Length, i =>
        {
            FrE.FromMontgomery(in scalarsMont[i], out scalars[i]);
        });
        return scalars;
    }


    private static Banderwagon MultiScalarMulFast(IReadOnlyList<AffinePoint> points, FrE[] scalars)
    {
        int numOfPoints = points.Count;
        int windowsSize = numOfPoints < 32 ? 3 : (int)((Math.Log2(numOfPoints) * 69) / 100) + 2;
        // const int windowsSize = 3;

        int i = 0;
        List<int> windowsStart = new();

        while (i < 253)
        {
            windowsStart.Add(i);
            i += windowsSize;
        }

        ulong bucketSize = ((ulong)1 << windowsSize) - 1;

        FrE[] scalarsReg = BatchConvertFromMontgomery(scalars);

        Banderwagon[] windowSums = new Banderwagon[windowsStart.Count];

        Parallel.For(0, windowsStart.Count, w =>
        {
            int winStart = windowsStart[w];

            Banderwagon res = Identity;
            Banderwagon[] buckets = new Banderwagon[bucketSize];

            for (int j = 0; j < buckets.Length; j++)
            {
                buckets[j] = Identity;
            }

            for (int j = 0; j < points.Count; j++)
            {
                if (scalarsReg[j].IsRegularOne)
                {
                    if (winStart == 0) res = Add(res, points[j]);
                }
                else
                {
                    FrE scalar = scalarsReg[j];
                    scalar >>= winStart;

                    ulong sc = scalar.u0;
                    sc %= ((ulong)1 << windowsSize);

                    if (sc != 0)
                    {
                        buckets[sc - 1] = Add(buckets[sc - 1], points[j]);
                    }
                }
            }

            Banderwagon runningSum = Identity;
            for (int j = (buckets.Length - 1); j >= 0; j--)
            {
                runningSum += buckets[j];
                res += runningSum;
            }

            windowSums[w] = res;
        });

        Banderwagon lowest = windowSums[0];

        Banderwagon result = Identity;
        for (int j = (windowSums.Length - 1); j > 0; j--)
        {
            result += windowSums[j];
            for (int k = 0; k < windowsSize; k++)
            {
                result = Double(result);
            }
        }

        result += lowest;
        return result;
    }
}
