using Field;
using Nethermind.Int256;

namespace Curve;

public struct BandersnatchBaseFieldStruct: IFieldDefinition
{
    private static readonly byte[] ModBytes =
    {
        1, 0, 0, 0, 255, 255, 255, 255, 254, 91, 254, 255, 2, 164, 189, 83, 5, 216, 161, 9, 8, 216, 57, 51, 72, 125,
        157, 41, 83, 167, 237, 115
    };
    public readonly UInt256 FieldMod => new (ModBytes);
}