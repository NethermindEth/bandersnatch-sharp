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
    private Fr? _pointAsField;
    public Fr PointAsField
    {
        get
        {
            if (_pointAsField is null) SetCommitmentToField();
            return _pointAsField;
        }
        private set => _pointAsField = value;
    }

    public Commitment(Banderwagon point)
    {
        Point = point;
    }

    public Commitment()
    {
        Point = Banderwagon.Identity();
    }

    private void SetCommitmentToField()
    {
        var mapToBytes = Point.MapToField();
        PointAsField = Fr.FromBytesReduced(mapToBytes);
    }

    public void AddPoint(Banderwagon point)
    {
        Point += point;
        _pointAsField = null;
        SetCommitmentToField();
    }
}

