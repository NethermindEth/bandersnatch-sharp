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

using System.Runtime.CompilerServices;
using Nethermind.Utils.Crypto;
using Nethermind.Utils.Extensions;

namespace Nethermind.Utils.Caching
{
    /// <summary>
    ///     https://stackoverflow.com/questions/754233/is-it-there-any-lru-implementation-of-idictionary
    /// </summary>
    public class MemCountingCache : ICache<Commitment, byte[]>
    {

        private const int PreInitMemorySize =
            48 /* LinkedList */ +
            80 /* Dictionary */ +
            MemorySizes.SmallObjectOverhead +
            8 /* sizeof(int) aligned */;

        private const int PostInitMemorySize =
            52 /* lazy loaded dictionary.Items */ + PreInitMemorySize;

        private const int DictionaryItemSize = 28;
        private readonly Dictionary<Commitment, LinkedListNode<LruCacheItem>> _cacheMap;
        private readonly LinkedList<LruCacheItem> _lruList;
        private readonly int _maxCapacity;
        private int _currentDictionaryCapacity;

        public MemCountingCache(int maxCapacity, int startCapacity, string name)
        {
            _maxCapacity = maxCapacity;
            _cacheMap = typeof(Commitment) == typeof(byte[])
                ? new Dictionary<Commitment, LinkedListNode<LruCacheItem>>((IEqualityComparer<Commitment>)Bytes.EqualityComparer)
                : new Dictionary<Commitment, LinkedListNode<LruCacheItem>>(startCapacity); // do not initialize it at the full capacity
            _lruList = new LinkedList<LruCacheItem>();
        }

        public MemCountingCache(int maxCapacity, string name)
            : this(maxCapacity, 0, name)
        {
        }

        public long MemorySize { get; private set; } = PreInitMemorySize;

        public void Clear()
        {
            _cacheMap?.Clear();
            _lruList?.Clear();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[]? Get(Commitment key)
        {
            if (_cacheMap.TryGetValue(key, out LinkedListNode<LruCacheItem>? node))
            {
                byte[] value = node.Value.Value;
                _lruList.Remove(node);
                _lruList.AddLast(node);
                return value;
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool TryGet(Commitment key, out byte[]? value)
        {
            if (_cacheMap.TryGetValue(key, out LinkedListNode<LruCacheItem>? node))
            {
                value = node.Value.Value;
                _lruList.Remove(node);
                _lruList.AddLast(node);
                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Set(Commitment key, byte[]? val)
        {
            if (val is null)
            {
                Delete(key);
                return;
            }

            if (_cacheMap.TryGetValue(key, out LinkedListNode<LruCacheItem>? node))
            {
                node.Value.Value = val;
                _lruList.Remove(node);
                _lruList.AddLast(node);
            }
            else
            {
                long cacheItemMemory = LruCacheItem.FindMemorySize(val);
                int newCount = _lruList.Count + 1;
                int capacityRemembered = _currentDictionaryCapacity;
                long dictionaryNewMemory = CalculateDictionaryPartMemory(_currentDictionaryCapacity, newCount);
                int initialGrowth = newCount == 1 ? PostInitMemorySize - PreInitMemorySize : 0;
                long newMemorySize =
                    MemorySizes.Align(
                        MemorySize +
                        initialGrowth +
                        dictionaryNewMemory +
                        cacheItemMemory
                    );

                if (newMemorySize <= _maxCapacity)
                {
                    MemorySize = newMemorySize;
                    LruCacheItem cacheItem = new LruCacheItem(key, val);
                    LinkedListNode<LruCacheItem> newNode = new LinkedListNode<LruCacheItem>(cacheItem);
                    _lruList.AddLast(newNode);
                    _cacheMap.Add(key, newNode);
                }
                else
                {
                    _currentDictionaryCapacity = capacityRemembered;
                    Replace(key, val);
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Delete(Commitment key)
        {
            if (_cacheMap.TryGetValue(key, out LinkedListNode<LruCacheItem>? node))
            {
                MemorySize -= node.Value.MemorySize;
                _lruList.Remove(node);
                _cacheMap.Remove(key);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Contains(Commitment key)
        {
            return _cacheMap.ContainsKey(key);
        }

        private void Replace(Commitment key, byte[] value)
        {
            LinkedListNode<LruCacheItem>? node = _lruList.First;

            // ReSharper disable once PossibleNullReferenceException
            MemorySize += MemorySizes.Align(value.Length) - MemorySizes.Align(node?.Value.Value.Length ?? 0);

            _lruList.RemoveFirst();
            _cacheMap.Remove(node!.Value.Key);

            node.Value.Value = value;
            node.Value.Key = key;
            _lruList.AddLast(node);
            _cacheMap.Add(key, node);
        }

        private long CalculateDictionaryPartMemory(int currentCapacity, int newCount)
        {
            int previousSize = _currentDictionaryCapacity * DictionaryItemSize;
            int newSize = previousSize;
            if (newCount > currentCapacity)
            {
                _currentDictionaryCapacity = MemorySizes.FindNextPrime(Math.Max(currentCapacity, 1) * 2);
                newSize = _currentDictionaryCapacity * DictionaryItemSize;
            }

            return newSize - previousSize;
        }

        private class LruCacheItem
        {

            public Commitment Key;
            public byte[] Value;
            public LruCacheItem(Commitment k, byte[] v)
            {
                Key = k;
                Value = v;
            }

            public long MemorySize => FindMemorySize(Value);

            public static long FindMemorySize(byte[] withValue)
            {
                return MemorySizes.Align(
                    Commitment.MemorySize +
                    MemorySizes.ArrayOverhead +
                    withValue.Length);
            }
        }
    }
}
