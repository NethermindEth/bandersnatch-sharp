using System.Diagnostics;
using Nethermind.Field;
using Nethermind.Verkle.Curve;

namespace Nethermind.Verkle.Utils;
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
    public Banderwagon Point { get; private set; }
    private Fr? _pointAsField;
    public Fr PointAsField
    {
        get
        {
            if (_pointAsField is null) SetCommitmentToField();
            Debug.Assert(_pointAsField is not null, nameof(_pointAsField) + " != null");
            return _pointAsField;
        }
        private set => _pointAsField = value;
    }

    public Commitment(Banderwagon point)
    {
        Point = point;
    }

    public Commitment Dup()
    {
        return new Commitment(Point.Dup());
    }

    public Commitment()
    {
        Point = Banderwagon.Identity();
    }

    private void SetCommitmentToField()
    {
        byte[] mapToBytes = Point.MapToField();
        PointAsField = Fr.FromBytesReduced(mapToBytes);
    }

    public void AddPoint(Banderwagon point)
    {
        Point += point;
        _pointAsField = null;
        SetCommitmentToField();
    }
}

