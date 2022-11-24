using System.Runtime.CompilerServices;

namespace Nethermind.Field.Montgomery.FpEElement;

public readonly partial struct FpE
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(in FpE a, long b) => b >= 0 && a.u3 == 0 && a.u2 == 0 && a.u1 == 0 && a.u0 < (ulong)b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(long a, in FpE b) => a < 0 || b.u1 != 0 || b.u2 != 0 || b.u3 != 0 || (ulong)a < b.u0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(in FpE a, ulong b) => a.u3 == 0 && a.u2 == 0 && a.u1 == 0 && a.u0 < b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(ulong a, in FpE b) => b.u3 != 0 || b.u2 != 0 || b.u1 != 0 || a < b.u0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(in FpE a, in FpE b)
    {
        if (a.u3 != b.u3)
            return a.u3 < b.u3;
        if (a.u2 != b.u2)
            return a.u2 < b.u2;
        if (a.u1 != b.u1)
            return a.u1 < b.u1;
        return a.u0 < b.u0;
    }

    public static bool LessThanSubModulus(FpE x) => LessThan(x, qElement);
    public bool Equals(int other) => other >= 0 && u0 == (uint)other && u1 == 0 && u2 == 0 && u3 == 0;

    public bool Equals(uint other) => u0 == other && u1 == 0 && u2 == 0 && u3 == 0;

    public bool Equals(long other) => other >= 0 && u0 == (ulong)other && u1 == 0 && u2 == 0 && u3 == 0;

    public bool Equals(ulong other) => u0 == other && u1 == 0 && u2 == 0 && u3 == 0;

    public override bool Equals(object? obj) => obj is FpE other && Equals(other);

    public bool Equals(FpE other) => u0 == other.u0 && u1 == other.u1 && u2 == other.u2 && u3 == other.u3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Equals(in FpE other) =>
        u0 == other.u0 &&
        u1 == other.u1 &&
        u2 == other.u2 &&
        u3 == other.u3;

    public int CompareTo(FpE b) => this < b ? -1 : Equals(b) ? 0 : 1;

    public override int GetHashCode() => HashCode.Combine(u0, u1, u2, u3);
}
