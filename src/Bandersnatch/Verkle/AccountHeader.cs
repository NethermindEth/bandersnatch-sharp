using Nethermind.Int256;

namespace Verkle;

public readonly struct AccountHeader
{
    private const int Version = 0;
    private const int Balance = 1;
    private const int Nonce = 2;
    private const int CodeHash = 3;
    private const int CodeSize = 4;

    private const int MainStorageOffsetExponent = 31;
    private static readonly UInt256 MainStorageOffsetBase = 256;
    private static readonly UInt256 MainStorageOffset = MainStorageOffsetBase << MainStorageOffsetExponent;

    private static readonly UInt256 HeaderStorageOffset = 64;
    private static readonly UInt256 CodeOffset = 128;
    private static readonly UInt256 VerkleNodeWidth = 256;
    
    public static byte[] ToAddress32(byte[] address20)
    {
        byte[] address32 = new byte[32];
        address20.CopyTo(address32, 0);
        return address32;
    }
    
    public static byte[] GetTreeKeyPrefix(byte[] address20, UInt256 treeIndex)
    {
        var address32 = ToAddress32(address20);
        return PedersenHash.Hash(address32, treeIndex);
    }
    
    public static byte[] GetTreeKeyPrefixAccount(byte[] address) => GetTreeKeyPrefix(address, 0);
    
    public static byte[] GetTreeKey(byte[] address, UInt256 treeIndex, byte subIndexBytes)
    {
        byte[] treeKeyPrefix = GetTreeKeyPrefix(address, treeIndex);
        treeKeyPrefix[31] = subIndexBytes;
        return treeKeyPrefix;
    }
    
    public static byte[] GetTreeKeyForVersion(byte[] address) => GetTreeKey(address, UInt256.Zero, Version);
    public static byte[] GetTreeKeyForBalance(byte[] address) => GetTreeKey(address, UInt256.Zero, Balance);
    public static byte[] GetTreeKeyForNonce(byte[] address) => GetTreeKey(address, UInt256.Zero, Nonce);
    public static byte[] GetTreeKeyForCodeKeccak(byte[] address) => GetTreeKey(address, UInt256.Zero, CodeHash);
    public static byte[] GetTreeKeyForCodeSize(byte[] address) => GetTreeKey(address, UInt256.Zero, CodeSize);
    
    
    public static byte[] GetTreeKeyForCodeChunk(byte[] address, UInt256 chunk)
    {
        UInt256 chunkOffset = CodeOffset + chunk;
        UInt256 treeIndex = chunkOffset / VerkleNodeWidth;
        UInt256.Mod(chunkOffset, VerkleNodeWidth, out UInt256 subIndex);
        return GetTreeKey(address, treeIndex, subIndex.ToBigEndian()[31]);
    }

    public static byte[] GetTreeKeyForStorageSlot(byte[] address, UInt256 storageKey)
    {
        UInt256 pos;
        
        if (storageKey < CodeOffset - HeaderStorageOffset) pos = HeaderStorageOffset + storageKey;
        else pos = MainStorageOffset + storageKey;

        UInt256 treeIndex = pos / VerkleNodeWidth;

        UInt256.Mod(pos, VerkleNodeWidth, out UInt256 subIndex);
        return GetTreeKey(address, treeIndex, subIndex.ToBigEndian()[31]);
    }


    public static void FillTreeAndSubIndexForChunk(UInt256 chunkId, ref Span<byte> subIndexBytes, out UInt256 treeIndex)
    {
        UInt256 chunkOffset = CodeOffset + chunkId;
        treeIndex = chunkOffset / VerkleNodeWidth;
        UInt256.Mod(chunkOffset, VerkleNodeWidth, out UInt256 subIndex);
        subIndex.ToBigEndian(subIndexBytes);
    }

    public ref struct CodeChunkEnumerator
    {
        const byte PushOffset = 95;
        const byte Push1 = PushOffset + 1;
        const byte Push32 = PushOffset + 32;

        private Span<byte> _code;
        private byte _rollingOverPushLength = 0;
        private readonly byte[] _bufferChunk = new byte[32];
        private readonly Span<byte> _bufferChunkCodePart;

        public CodeChunkEnumerator(Span<byte> code)
        {
            _code = code;
            _bufferChunkCodePart = _bufferChunk.AsSpan().Slice(1);
        }

        // Try get next chunk
        public bool TryGetNextChunk(out byte[] chunk)
        {
            chunk = _bufferChunk;

            // we don't have chunks left
            if (_code.IsEmpty)
            {
                return false;
            }

            // we don't have full chunk
            if (_code.Length < 31)
            {
                // need to have trailing zeroes
                _bufferChunkCodePart.Fill(0);

                // set number of push bytes
                _bufferChunk[0] = _rollingOverPushLength;

                // copy main bytes
                _code.CopyTo(_bufferChunkCodePart);

                // we are done
                _code = Span<byte>.Empty;
            }
            else
            {
                // fill up chunk to store

                // get current chunk of code
                Span<byte> currentChunk = _code.Slice(0, 31);

                // copy main bytes
                currentChunk.CopyTo(_bufferChunkCodePart);

                switch (_rollingOverPushLength)
                {
                    case 32 or 31: // all bytes are roll over

                        // set number of push bytes
                        _bufferChunk[0] = 31;

                        // if 32, then we will roll over with 1 to even next chunk
                        _rollingOverPushLength -= 31;
                        break;
                    default:
                        // set number of push bytes
                        _bufferChunk[0] = _rollingOverPushLength;
                        _rollingOverPushLength = 0;

                        // check if we have a push instruction in remaining code
                        // ignore the bytes we rolled over, they are not instructions
                        for (int i = _bufferChunk[0]; i < 31;)
                        {
                            byte instruction = currentChunk[i];
                            i++;
                            if (instruction is >= Push1 and <= Push32)
                            {
                                // we calculate data to ignore in code
                                i += instruction - PushOffset;

                                // check if we rolled over the chunk
                                _rollingOverPushLength = (byte)Math.Max(i - 31, 0);
                            }
                        }
                        break;
                }
                
                // move to next chunk
                _code = _code.Slice(31);
            }
            return true;
        }
    }
}