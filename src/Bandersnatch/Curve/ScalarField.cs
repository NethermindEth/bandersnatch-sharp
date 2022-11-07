using Field;
using Nethermind.Int256;

namespace Curve;

public struct BandersnatchScalarFieldStruct: IFieldDefinition
{
    private static readonly byte[] ModBytes =
    {
        225, 231, 118, 40, 181, 6, 253, 116, 113, 4, 25, 116, 0, 135, 143, 255, 0, 118, 104, 2, 2, 118, 206, 12, 82, 95,
        103, 202, 212, 105, 251, 28
    };
    public readonly UInt256 FieldMod => new (ModBytes);
}
