using System.Diagnostics;
using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Verkle.Curve;

namespace Nethermind.Verkle.Utils
{
    public struct Committer
    {
        private static readonly CRS Constants = CRS.Instance;

        public static Banderwagon Commit(FrE[] value)
        {
            Banderwagon elem = Banderwagon.Identity();
            for (int i = 0; i < value.Length; i++)
            {
                elem += value[i] * Constants.BasisG[i];
            }

            return elem;
        }

        public static Banderwagon ScalarMul(FrE value, int index)
        {
            return Constants.BasisG[index] * value;
        }
    }

    public class Commitment
    {
        private FrE? _pointAsField;

        public Commitment(Banderwagon point)
        {
            Point = point;
        }

        public Commitment()
        {
            Point = Banderwagon.Identity();
        }
        public Banderwagon Point { get; private set; }
        public FrE PointAsField
        {
            get
            {
                if (_pointAsField is null) SetCommitmentToField();
                Debug.Assert(_pointAsField is not null, nameof(_pointAsField) + " != null");
                return _pointAsField.Value;
            }
            private set => _pointAsField = value;
        }

        public Commitment Dup()
        {
            return new Commitment(Point.Dup());
        }

        private void SetCommitmentToField()
        {
            byte[] mapToBytes = Point.MapToField();
            PointAsField = FrE.FromBytesReduced(mapToBytes);
        }

        public void AddPoint(Banderwagon point)
        {
            Point += point;
            _pointAsField = null;
            SetCommitmentToField();
        }
    }
}
