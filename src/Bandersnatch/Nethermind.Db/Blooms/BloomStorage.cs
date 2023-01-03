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

using System.Collections;
using Nethermind.Serialization.Rlp;
using Nethermind.Utils;
using Nethermind.Utils.Caching;
using Nethermind.Utils.Crypto;
using Nethermind.Utils.Extensions;

namespace Nethermind.Db.Blooms
{
    public class BloomStorage : IBloomStorage
    {

        internal static readonly Commitment MinBlockNumberKey = Commitment.Compute(nameof(MinBlockNumber));
        internal static readonly Commitment MaxBlockNumberKey = Commitment.Compute(nameof(MaxBlockNumber));
        private static readonly Commitment MigrationBlockNumberKey = Commitment.Compute(nameof(MigratedBlockNumber));
        private static readonly Commitment LevelsKey = Commitment.Compute(nameof(LevelsKey));
        private readonly IDb _bloomInfoDb;
        private readonly IBloomConfig _config;
        private readonly IFileStoreFactory _fileStoreFactory;

        private readonly BloomStorageLevel[] _storageLevels;
        private long _maxBlockNumber;
        private long _migratedBlockNumber;
        private long _minBlockNumber;

        public BloomStorage(IBloomConfig config, IDb bloomDb, IFileStoreFactory fileStoreFactory)
        {
            long Get(Commitment key, long defaultValue)
            {
                return bloomDb.Get(key)?.ToLongFromBigEndianByteArrayWithoutLeadingZeros() ?? defaultValue;
            }

            _config = config ?? throw new ArgumentNullException(nameof(config));
            _bloomInfoDb = bloomDb ?? throw new ArgumentNullException(nameof(_bloomInfoDb));
            _fileStoreFactory = fileStoreFactory;
            _storageLevels = CreateStorageLevels(config);
            Levels = (byte)_storageLevels.Length;
            _minBlockNumber = Get(MinBlockNumberKey, long.MaxValue);
            _maxBlockNumber = Get(MaxBlockNumberKey, -1);
            _migratedBlockNumber = Get(MigrationBlockNumberKey, -1);
        }
        public byte Levels { get; }
        public int MaxBucketSize => _storageLevels.FirstOrDefault()?.LevelElementSize ?? 1;

        public long MaxBlockNumber
        {
            get => _maxBlockNumber;
            set
            {
                _maxBlockNumber = value;
                Set(MaxBlockNumberKey, MaxBlockNumber);
            }
        }

        public long MinBlockNumber
        {
            get => _minBlockNumber;
            private set
            {
                _minBlockNumber = value;
                Set(MinBlockNumberKey, MinBlockNumber);
            }
        }

        public long MigratedBlockNumber
        {
            get => _migratedBlockNumber;
            private set
            {
                _migratedBlockNumber = value;
                Set(MigrationBlockNumberKey, MigratedBlockNumber);
            }
        }

        public bool ContainsRange(in long fromBlockNumber, in long toBlockNumber)
        {
            return Contains(fromBlockNumber) && Contains(toBlockNumber);
        }

        public IEnumerable<Average> Averages => _storageLevels.Select(l => l.Average);

        public void Store(long blockNumber, Bloom bloom)
        {
            for (int i = 0; i < _storageLevels.Length; i++)
            {
                _storageLevels[i].Store(blockNumber, bloom);
            }

            if (blockNumber < MinBlockNumber)
            {
                MinBlockNumber = blockNumber;
            }

            if (blockNumber > MaxBlockNumber)
            {
                MaxBlockNumber = blockNumber;
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < _storageLevels.Length; i++)
            {
                _storageLevels[i].Dispose();
            }
        }

        public IBloomEnumeration GetBlooms(long fromBlock, long toBlock)
        {
            return new BloomEnumeration(_storageLevels, Math.Max(fromBlock, MinBlockNumber), Math.Min(toBlock, MaxBlockNumber));
        }

        private BloomStorageLevel[] CreateStorageLevels(IBloomConfig config)
        {
            void ValidateConfigValue()
            {
                if (config.IndexLevelBucketSizes.Length == 0)
                {
                    throw new ArgumentException($"Can not create bloom index when there are no {nameof(config.IndexLevelBucketSizes)} provided.", nameof(config.IndexLevelBucketSizes));
                }
            }

            List<int> InsertBaseLevelIfNeeded()
            {
                List<int> sizes = config.IndexLevelBucketSizes.ToList();
                if (sizes.FirstOrDefault() != 1)
                {
                    sizes.Insert(0, 1);
                }

                return sizes;
            }

            void ValidateCurrentDbStructure(IList<int> sizes)
            {
                byte[]? levelsFromDb = _bloomInfoDb.Get(LevelsKey);

                if (levelsFromDb is null)
                {
                    _bloomInfoDb.Set(LevelsKey, Rlp.Encode(sizes.ToArray()).Bytes);
                }
                else
                {
                    RlpStream stream = new RlpStream(levelsFromDb);
                    int[] dbBucketSizes = stream.DecodeArray(x => x.DecodeInt());

                    if (!dbBucketSizes.SequenceEqual(sizes))
                    {
                        throw new ArgumentException($"Can not load bloom db. {nameof(config.IndexLevelBucketSizes)} changed without rebuilding bloom db. Db structure is [{string.Join(",", dbBucketSizes)}]. Current config value is [{string.Join(",", sizes)}]. " +
                                                    $"If you want to rebuild {DbNames.Bloom} db, please delete db folder. If not, please change config value to reflect current db structure", nameof(config.IndexLevelBucketSizes));
                    }
                }
            }

            ValidateConfigValue();
            List<int> configIndexLevelBucketSizes = InsertBaseLevelIfNeeded();
            ValidateCurrentDbStructure(configIndexLevelBucketSizes);

            int lastLevelSize = 1;
            return configIndexLevelBucketSizes
                .Select((size, i) =>
                {
                    byte level = (byte)(configIndexLevelBucketSizes.Count - i - 1);
                    int levelElementSize = lastLevelSize * size;
                    lastLevelSize = levelElementSize;
                    return new BloomStorageLevel(_fileStoreFactory.Create(level.ToString()), level, levelElementSize, size, _config.MigrationStatistics);
                })
                .Reverse()
                .ToArray();
        }

        private bool Contains(long blockNumber)
        {
            return blockNumber >= MinBlockNumber && blockNumber <= MaxBlockNumber;
        }

        // public void Migrate(IEnumerable<BlockHeader> headers)
        // {
        //     var batchSize = _storageLevels.First().LevelElementSize;
        //     (BloomStorageLevel Level, Bloom Bloom)[] levelBlooms = _storageLevels.SkipLast(1).Select(l => (l, new Bloom())).ToArray();
        //     BloomStorageLevel lastLevel = _storageLevels.Last();
        //
        //     long i = 0;
        //     long lastBlockNumber = -1;
        //
        //     foreach (BlockHeader blockHeader in headers)
        //     {
        //         i++;
        //
        //         Bloom blockHeaderBloom = blockHeader.Bloom ?? Bloom.Empty;
        //         lastLevel.Migrate(blockHeader.Number, blockHeaderBloom);
        //
        //         for (int index = 0; index < levelBlooms.Length; index++)
        //         {
        //             (BloomStorageLevel level, Bloom bloom) = levelBlooms[index];
        //             bloom.Accumulate(blockHeaderBloom);
        //
        //             if (i % level.LevelElementSize == 0)
        //             {
        //                 level.Migrate(blockHeader.Number, bloom);
        //                 levelBlooms[index] = (level, new Bloom());
        //
        //                 if (level.LevelElementSize == batchSize)
        //                 {
        //                     MigratedBlockNumber += batchSize;
        //                 }
        //             }
        //         }
        //
        //         lastBlockNumber = blockHeader.Number;
        //     }
        //
        //     if (i % batchSize != 0)
        //     {
        //         for (int index = 0; index < levelBlooms.Length; index++)
        //         {
        //             (BloomStorageLevel level, Bloom bloom) = levelBlooms[index];
        //             level.Store(lastBlockNumber, bloom);
        //         }
        //
        //         MigratedBlockNumber += i;
        //     }
        //
        //     if (MigratedBlockNumber >= MinBlockNumber - 1)
        //     {
        //         MinBlockNumber = 0;
        //     }
        // }

        private void Set(Commitment key, long value)
        {
            _bloomInfoDb.Set(key, value.ToBigEndianByteArrayWithoutLeadingZeros());
        }

        private class BloomStorageLevel : IDisposable
        {
            private readonly ICache<long, Bloom> _cache;

            private readonly IFileStore _fileStore;
            private readonly bool _migrationStatistics;

            public BloomStorageLevel(IFileStore fileStore, in byte level, in int levelElementSize, in int levelMultiplier, bool migrationStatistics)
            {
                _fileStore = fileStore;
                Level = level;
                LevelElementSize = levelElementSize;
                LevelMultiplier = levelMultiplier;
                _migrationStatistics = migrationStatistics;
                _cache = new LruCache<long, Bloom>(levelMultiplier, levelMultiplier, "blooms");
            }

            public void Dispose()
            {
                _fileStore?.Dispose();
            }

            public void Store(long blockNumber, Bloom bloom)
            {
                long bucket = GetBucket(blockNumber);

                try
                {
                    lock (_fileStore)
                    {
                        Bloom? existingBloom = _cache.Get(bucket);
                        if (existingBloom is null)
                        {
                            byte[] bytes = new byte[Bloom.ByteLength];
                            int bytesRead = _fileStore.Read(bucket, bytes);
                            bool bloomRead = bytesRead == Bloom.ByteLength;
                            existingBloom = bloomRead ? new Bloom(bytes) : new Bloom();
                        }

                        existingBloom.Accumulate(bloom);

                        _fileStore.Write(bucket, existingBloom.Bytes);
                        _cache.Set(bucket, existingBloom);
                    }
                }
                catch (InvalidOperationException e)
                {
                    InvalidOperationException exception = new InvalidOperationException(e.Message + $" Trying to write bloom index for block {blockNumber} at bucket {bucket}", e)
                    {
                        Data =
                        {
                            {
                                "Bucket", bucket
                            },
                            {
                                "Block", blockNumber
                            }
                        }
                    };

                    throw exception;
                }
            }

            private static uint CountBits(Bloom bloom)
            {
                return bloom.Bytes.AsSpan().CountBits();
            }

            public long GetBucket(long blockNumber)
            {
                return blockNumber / LevelElementSize;
            }

            public IFileReader CreateReader()
            {
                return _fileStore.CreateFileReader();
            }

            public void Migrate(in long blockNumber, Bloom bloom)
            {
                if (_migrationStatistics)
                {
                    Average.Increment(CountBits(bloom));
                }

                _fileStore.Write(GetBucket(blockNumber), bloom.Bytes);
            }
            // ReSharper disable InconsistentNaming
            private readonly byte Level;
            public readonly int LevelElementSize;
            private readonly int LevelMultiplier;
            public readonly Average Average = new Average();
            // ReSharper restore InconsistentNaming
        }

        private class BloomEnumeration : IBloomEnumeration
        {
            private readonly long _fromBlock;
            private readonly BloomStorageLevel[] _storageLevels;
            private readonly long _toBlock;
            private BloomEnumerator _current;

            public BloomEnumeration(BloomStorageLevel[] storageLevels, long fromBlock, long toBlock)
            {
                _storageLevels = storageLevels;
                _fromBlock = fromBlock;
                _toBlock = toBlock;
            }

            public IEnumerator<Bloom> GetEnumerator()
            {
                return _current = new BloomEnumerator(_storageLevels, _fromBlock, _toBlock);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public bool TryGetBlockNumber(out long blockNumber)
            {
                return _current.TryGetBlockNumber(out blockNumber);
            }
            public (long FromBlock, long ToBlock) CurrentIndices => _current.CurrentIndices;

            public override string ToString()
            {
                return _current.ToString();
            }
        }

        private class BloomEnumerator : IEnumerator<Bloom>
        {
            private readonly Bloom _bloom = new Bloom();
            private readonly long _fromBlock;
            private readonly int _maxLevel;
            private readonly (BloomStorageLevel Storage, IFileReader Reader)[] _storageLevels;
            private readonly long _toBlock;
            private byte _currentLevel;

            private bool _currentLevelRead;
            private long _currentPosition;

            public BloomEnumerator(BloomStorageLevel[] storageLevels, in long fromBlock, in long toBlock)
            {
                _storageLevels = GetStorageLevels(storageLevels, fromBlock, toBlock);
                _fromBlock = fromBlock;
                _toBlock = toBlock;
                _maxLevel = _storageLevels.Length - 1;
                Reset();
            }

            private byte CurrentLevel
            {
                get => _currentLevel;
                set
                {
                    _currentLevel = value;
                    _currentLevelRead = false;
                }
            }

            public (long FromBlock, long ToBlock) CurrentIndices
            {
                get
                {
                    BloomStorageLevel level = _storageLevels[_currentLevel].Storage;
                    long bucket = level.GetBucket(_currentPosition);
                    return (bucket * level.LevelElementSize, (bucket + 1) * level.LevelElementSize - 1);
                }
            }

            public void Reset()
            {
                _currentPosition = _fromBlock;
                CurrentLevel = 0;
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (_currentLevelRead)
                {
                    BloomStorageLevel currentStorageLevel = _storageLevels[CurrentLevel].Storage;
                    _currentPosition += _currentPosition == _fromBlock
                        ? currentStorageLevel.LevelElementSize - _currentPosition % currentStorageLevel.LevelElementSize
                        : currentStorageLevel.LevelElementSize;

                    while (CurrentLevel > 0 && _currentPosition % _storageLevels[CurrentLevel - 1].Storage.LevelElementSize == 0)
                    {
                        CurrentLevel--;
                    }
                }
                else
                {
                    _currentLevelRead = true;
                }

                return _currentPosition <= _toBlock;
            }

            public Bloom Current
            {
                get
                {
                    if (_currentPosition < _fromBlock || _currentPosition > _toBlock)
                    {
                        return null;
                    }

                    (BloomStorageLevel Storage, IFileReader Reader) storageLevel = _storageLevels[CurrentLevel];
                    return storageLevel.Reader.Read(storageLevel.Storage.GetBucket(_currentPosition), _bloom.Bytes) == Bloom.ByteLength ? _bloom : Bloom.Empty;
                }
            }

            public void Dispose()
            {
                foreach ((BloomStorageLevel Storage, IFileReader Reader) storageLevel in _storageLevels)
                {
                    storageLevel.Reader.Dispose();
                }
            }

            private (BloomStorageLevel Storage, IFileReader Reader)[] GetStorageLevels(BloomStorageLevel[] storageLevels, long fromBlock, long toBlock)
            {
                // Skip higher levels if we would do only 1 or 2 lookups in them. Thanks to that we can skip a lot of IO operations on that file
                IList<BloomStorageLevel> levels = new List<BloomStorageLevel>(storageLevels.Length);
                for (int i = 0; i < storageLevels.Length; i++)
                {
                    BloomStorageLevel level = storageLevels[i];

                    if (i != storageLevels.Length - 1)
                    {
                        long fromBucket = level.GetBucket(fromBlock);
                        long toBucket = level.GetBucket(toBlock);
                        if (toBucket - fromBucket + 1 <= 2)
                        {
                            continue;
                        }
                    }

                    levels.Add(level);
                }

                return levels.Select(l => (l, l.CreateReader())).AsParallel().ToArray();
            }

            public bool TryGetBlockNumber(out long blockNumber)
            {
                blockNumber = _currentPosition;
                if (CurrentLevel == _maxLevel)
                {
                    return true;
                }

                CurrentLevel++;
                return false;
            }

            public override string ToString()
            {
                (long FromBlock, long ToBlock) indices = CurrentIndices;
                return $"From: {_fromBlock}, To: {_toBlock}, MaxLevel {_maxLevel}, CurrentBloom {indices.FromBlock}...{indices.ToBlock}, CurrentLevelSize {indices.ToBlock - indices.FromBlock + 1} CurrentLevel {CurrentLevel}, LevelRead {_currentLevelRead}";
            }
        }
    }
}
