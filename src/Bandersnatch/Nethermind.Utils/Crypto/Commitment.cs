//  Copyright (c) 2021 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
//
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.

using System.Diagnostics;
using System.Runtime.InteropServices;
using Nethermind.Utils.Extensions;

namespace Nethermind.Utils.Crypto
{
    public unsafe struct ValueCommitment
    {
        internal const int Size = 32;
        public fixed byte Bytes[Size];

        public Span<byte> BytesAsSpan => MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref this, 1));

        /// <returns>
        ///     <string>0xc5d2460186f7233c927e7db2dcc703c0e500b653ca82273b7bfad8045d85a470</string>
        /// </returns>
        public static readonly ValueCommitment OfAnEmptyString = InternalCompute(new byte[] { });


        [DebuggerStepThrough]
        public static ValueCommitment Compute(ReadOnlySpan<byte> input)
        {
            if (input.Length == 0)
            {
                return OfAnEmptyString;
            }

            ValueCommitment result = new();
            byte* ptr = result.Bytes;
            Span<byte> output = new(ptr, CommitmentHash.HASH_SIZE);
            CommitmentHash.ComputeHashBytesToSpan(input, output);
            return result;
        }

        private static ValueCommitment InternalCompute(byte[] input)
        {
            ValueCommitment result = new();
            byte* ptr = result.Bytes;
            Span<byte> output = new(ptr, CommitmentHash.HASH_SIZE);
            CommitmentHash.ComputeHashBytesToSpan(input, output);
            return result;
        }
    }

    [DebuggerStepThrough]
    public class Commitment : IEquatable<Commitment>, IComparable<Commitment>
    {
        public const int Size = 32;

        public const int MemorySize =
            MemorySizes.SmallObjectOverhead +
            MemorySizes.RefSize +
            MemorySizes.ArrayOverhead +
            Size -
            MemorySizes.SmallObjectFreeDataSize;

        /// <returns>
        ///     <string>0xc5d2460186f7233c927e7db2dcc703c0e500b653ca82273b7bfad8045d85a470</string>
        /// </returns>
        public static readonly Commitment OfAnEmptyString = InternalCompute(new byte[] { });

        /// <returns>
        ///     <string>0x1dcc4de8dec75d7aab85b567b6ccd41ad312451b948a7413f0a142fd40d49347</string>
        /// </returns>
        public static readonly Commitment OfAnEmptySequenceRlp = InternalCompute(new byte[] { 192 });

        /// <summary>
        ///     0x56e81f171bcc55a6ff8345e692c0f86e5b48e01b996cadc001622fb5e363b421
        /// </summary>
        public static Commitment EmptyTreeHash = InternalCompute(new byte[] { 128 });

        /// <returns>
        ///     <string>0x0000000000000000000000000000000000000000000000000000000000000000</string>
        /// </returns>
        public static Commitment Zero { get; } = new(new byte[Size]);

        /// <summary>
        ///     <string>0xffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff</string>
        /// </summary>
        public static Commitment MaxValue { get; } = new("0xffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff");

        public byte[] Bytes { get; }

        public Commitment(string hexString)
            : this(Extensions.Bytes.FromHexString(hexString)) { }

        public Commitment(byte[] bytes)
        {
            if (bytes.Length != Size)
            {
                throw new ArgumentException($"{nameof(Commitment)} must be {Size} bytes and was {bytes.Length} bytes", nameof(bytes));
            }

            Bytes = bytes;
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public string ToShortString(bool withZeroX = true)
        {
            string hash = Bytes.ToHexString(withZeroX);
            return $"{hash.Substring(0, withZeroX ? 8 : 6)}...{hash.Substring(hash.Length - 6)}";
        }

        public string ToString(bool withZeroX)
        {
            return Bytes.ToHexString(withZeroX);
        }

        [DebuggerStepThrough]
        public static Commitment Compute(byte[]? input)
        {
            if (input is null || input.Length == 0)
            {
                return OfAnEmptyString;
            }

            return new Commitment(CommitmentHash.ComputeHashBytes(input));
        }

        [DebuggerStepThrough]
        public static Commitment Compute(ReadOnlySpan<byte> input)
        {
            if (input.Length == 0)
            {
                return OfAnEmptyString;
            }

            return new Commitment(CommitmentHash.ComputeHashBytes(input));
        }

        private static Commitment InternalCompute(byte[] input)
        {
            return new(CommitmentHash.ComputeHashBytes(input.AsSpan()));
        }

        [DebuggerStepThrough]
        public static Commitment Compute(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return OfAnEmptyString;
            }

            return InternalCompute(System.Text.Encoding.UTF8.GetBytes(input));
        }

        public bool Equals(Commitment? other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return Extensions.Bytes.AreEqual(other.Bytes, Bytes);
        }

        public int CompareTo(Commitment? other)
        {
            return Extensions.Bytes.Comparer.Compare(Bytes, other?.Bytes);
        }

        public override bool Equals(object? obj)
        {
            return obj?.GetType() == typeof(Commitment) && Equals((Commitment)obj);
        }

        public override int GetHashCode()
        {
            return MemoryMarshal.Read<int>(Bytes);
        }

        public static bool operator ==(Commitment? a, Commitment? b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }

            if (ReferenceEquals(b, null))
            {
                return false;
            }

            return Extensions.Bytes.AreEqual(a.Bytes, b.Bytes);
        }

        public static bool operator !=(Commitment? a, Commitment? b)
        {
            return !(a == b);
        }

        public static bool operator >(Commitment? k1, Commitment? k2)
        {
            return Extensions.Bytes.Comparer.Compare(k1?.Bytes, k2?.Bytes) > 0;
        }

        public static bool operator <(Commitment? k1, Commitment? k2)
        {
            return Extensions.Bytes.Comparer.Compare(k1?.Bytes, k2?.Bytes) < 0;
        }

        public static bool operator >=(Commitment? k1, Commitment? k2)
        {
            return Extensions.Bytes.Comparer.Compare(k1?.Bytes, k2?.Bytes) >= 0;
        }

        public static bool operator <=(Commitment? k1, Commitment? k2)
        {
            return Extensions.Bytes.Comparer.Compare(k1?.Bytes, k2?.Bytes) <= 0;
        }

        public CommitmentStructRef ToStructRef() => new(Bytes);
    }

    public ref struct CommitmentStructRef
    {
        public const int Size = 32;

        public int MemorySize => MemorySizes.ArrayOverhead + Size;

        public Span<byte> Bytes { get; }

        public CommitmentStructRef(Span<byte> bytes)
        {
            if (bytes.Length != Size)
            {
                throw new ArgumentException($"{nameof(Commitment)} must be {Size} bytes and was {bytes.Length} bytes", nameof(bytes));
            }

            Bytes = bytes;
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public string ToShortString(bool withZeroX = true)
        {
            string hash = Bytes.ToHexString(withZeroX);
            return $"{hash.Substring(0, withZeroX ? 8 : 6)}...{hash.Substring(hash.Length - 6)}";
        }

        public string ToString(bool withZeroX)
        {
            return Bytes.ToHexString(withZeroX);
        }

        [DebuggerStepThrough]
        public static CommitmentStructRef Compute(byte[]? input)
        {
            if (input is null || input.Length == 0)
            {
                return new CommitmentStructRef(Commitment.OfAnEmptyString.Bytes);
            }

            var result = new CommitmentStructRef();
            CommitmentHash.ComputeHashBytesToSpan(input, result.Bytes);
            return result;
        }

        [DebuggerStepThrough]
        public static CommitmentStructRef Compute(Span<byte> input)
        {
            if (input.Length == 0)
            {
                return new CommitmentStructRef(Commitment.OfAnEmptyString.Bytes);
            }

            var result = new CommitmentStructRef();
            CommitmentHash.ComputeHashBytesToSpan(input, result.Bytes);
            return result;
        }

        private static CommitmentStructRef InternalCompute(Span<byte> input)
        {
            var result = new CommitmentStructRef();
            CommitmentHash.ComputeHashBytesToSpan(input, result.Bytes);
            return result;
        }

        [DebuggerStepThrough]
        public static CommitmentStructRef Compute(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new CommitmentStructRef(Commitment.OfAnEmptyString.Bytes);
            }

            var result = new CommitmentStructRef();
            CommitmentHash.ComputeHashBytesToSpan(System.Text.Encoding.UTF8.GetBytes(input), result.Bytes);
            return result;
        }

        public bool Equals(Commitment? other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return Extensions.Bytes.AreEqual(other.Bytes, Bytes);
        }

        public bool Equals(CommitmentStructRef other) => Extensions.Bytes.AreEqual(other.Bytes, Bytes);

        public override bool Equals(object? obj)
        {
            return obj?.GetType() == typeof(Commitment) && Equals((Commitment)obj);
        }

        public override int GetHashCode()
        {
            return MemoryMarshal.Read<int>(Bytes);
        }

        public static bool operator ==(CommitmentStructRef a, Commitment? b)
        {
            if (ReferenceEquals(b, null))
            {
                return false;
            }

            return Extensions.Bytes.AreEqual(a.Bytes, b.Bytes);
        }

        public static bool operator ==(Commitment? a, CommitmentStructRef b)
        {
            if (ReferenceEquals(a, null))
            {
                return false;
            }

            return Extensions.Bytes.AreEqual(a.Bytes, b.Bytes);
        }

        public static bool operator ==(CommitmentStructRef a, CommitmentStructRef b)
        {
            return Extensions.Bytes.AreEqual(a.Bytes, b.Bytes);
        }

        public static bool operator !=(CommitmentStructRef a, Commitment b)
        {
            return !(a == b);
        }

        public static bool operator !=(Commitment a, CommitmentStructRef b)
        {
            return !(a == b);
        }

        public static bool operator !=(CommitmentStructRef a, CommitmentStructRef b)
        {
            return !(a == b);
        }

        public Commitment ToCommitment() => new(Bytes.ToArray());
    }
}
