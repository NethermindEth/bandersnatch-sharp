using Curve;
using Field;

namespace Verkle;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public struct Committer
{
    private static readonly CRS Constants = CRS.Default();

    public static Banderwagon Commit(Fr[] value)
    {
        Banderwagon elem = Banderwagon.Identity();
        for (int i = 0; i < value.Length; i++)
        {
            elem += value[i] * Constants.BasisG[i];
        }

        return elem;
    }

    public static Banderwagon ScalarMul(Fr value, int index)
    {
        return Constants.BasisG[index] * value;
    }
}

public class Commitment
{
    public Banderwagon Point;
    public Fr? PointAsField;

    public Commitment(Banderwagon point)
    {
        Point = point;
    }

    public Commitment()
    {
        Point = Banderwagon.Identity();
    }

    public Fr CommitmentToField()
    {
        if (PointAsField is not null) return PointAsField;
        byte[] mapToBytes = Point.MapToField();
        PointAsField = Fr.FromBytesReduced(mapToBytes);
        return PointAsField;
    }

    public void AddPoint(Banderwagon point)
    {
        Point += point;
        PointAsField = null;
        CommitmentToField();
    }
}

