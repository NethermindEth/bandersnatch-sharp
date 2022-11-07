using System.Security.Cryptography;
using System.Text;
using Curve;
using Field;

namespace Proofs;
using Fp = FixedFiniteField<BandersnatchBaseFieldStruct>;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class Transcript
{
    public List<byte> CurrentHash = new ();
    
    public Transcript(IEnumerable<byte> label)
    {
        CurrentHash.AddRange(label);
    }
    
    public Transcript(string label)
    {
        CurrentHash.AddRange(Encoding.ASCII.GetBytes(label));
    }

    public static Fr ByteToField(byte[] bytes)
    {
        return Fr.FromBytesReduced(bytes);
    }

    public void AppendBytes(IEnumerable<byte> message, IEnumerable<byte> label)
    {
        CurrentHash.AddRange(label);
        CurrentHash.AddRange(message);
    }

    public void AppendBytes(string message, string label) =>
        AppendBytes(Encoding.ASCII.GetBytes(message), Encoding.ASCII.GetBytes(label));

    public void AppendScalar(Fr scalar, IEnumerable<byte> label)
    {
        AppendBytes(scalar.ToBytes(), label);
    }
    public void AppendScalar(Fr scalar, string label) => AppendScalar(scalar, Encoding.ASCII.GetBytes(label));

    public void AppendPoint(Banderwagon point, byte[] label)
    {
        AppendBytes(point.ToBytes(), label);
    }
    public void AppendPoint(Banderwagon point, string label) => AppendPoint(point, Encoding.ASCII.GetBytes(label));

    public Fr ChallengeScalar(byte[] label)
    {
        DomainSep(label);
        var hash = SHA256.Create().ComputeHash(CurrentHash.ToArray());
        var challenge = ByteToField(hash);
        CurrentHash = new List<byte>();
        
        AppendScalar(challenge, label);
        return challenge;
    }
    public Fr ChallengeScalar(string label) => ChallengeScalar(Encoding.ASCII.GetBytes(label));

    public void DomainSep(byte[] label)
    {
        CurrentHash.AddRange(label);
    }
    public void DomainSep(string label) => DomainSep(Encoding.ASCII.GetBytes(label));
}