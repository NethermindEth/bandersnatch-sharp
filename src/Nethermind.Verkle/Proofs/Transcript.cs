using System.Security.Cryptography;
using System.Text;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Curve;

namespace Nethermind.Verkle.Proofs
{
    public class Transcript
    {
        private List<byte> _currentHash = new List<byte>();

        public Transcript(IEnumerable<byte> label)
        {
            _currentHash.AddRange(label);
        }

        public Transcript(string label)
        {
            _currentHash.AddRange(Encoding.ASCII.GetBytes(label));
        }

        private static FrE ByteToField(byte[] bytes)
        {
            return FrE.FromBytesReduced(bytes);
        }

        private void AppendBytes(IEnumerable<byte> message, IEnumerable<byte> label)
        {
            _currentHash.AddRange(label);
            _currentHash.AddRange(message);
        }

        public void AppendBytes(string message, string label)
        {
            AppendBytes(Encoding.ASCII.GetBytes(message), Encoding.ASCII.GetBytes(label));
        }

        public void AppendScalar(FrE scalar, IEnumerable<byte> label)
        {
            AppendBytes(scalar.ToBytes().ToArray(), label);
        }
        public void AppendScalar(FrE scalar, string label)
        {
            AppendScalar(scalar, Encoding.ASCII.GetBytes(label));
        }

        public void AppendPoint(Banderwagon point, byte[] label)
        {
            AppendBytes(point.ToBytes(), label);
        }
        public void AppendPoint(Banderwagon point, string label)
        {
            AppendPoint(point, Encoding.ASCII.GetBytes(label));
        }

        public FrE ChallengeScalar(byte[] label)
        {
            DomainSep(label);
            byte[] hash = SHA256.HashData(_currentHash.ToArray());
            FrE challenge = ByteToField(hash);
            _currentHash = new List<byte>();

            AppendScalar(challenge, label);
            return challenge;
        }
        public FrE ChallengeScalar(string label)
        {
            return ChallengeScalar(Encoding.ASCII.GetBytes(label));
        }

        public void DomainSep(byte[] label)
        {
            _currentHash.AddRange(label);
        }
        public void DomainSep(string label)
        {
            DomainSep(Encoding.ASCII.GetBytes(label));
        }
    }
}
