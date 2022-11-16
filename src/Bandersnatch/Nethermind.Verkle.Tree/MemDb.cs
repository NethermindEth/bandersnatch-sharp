// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using System.Collections.Concurrent;

namespace Nethermind.Verkle.Tree;

public class MemDb : IFullDb, IDbWithSpan
{
    private readonly int _writeDelay; // for testing scenarios
    private readonly int _readDelay; // for testing scenarios
    public long ReadsCount { get; private set; }
    public long WritesCount { get; private set; }

    private readonly ConcurrentDictionary<byte[], byte[]?> _db;

    public MemDb(string name)
        : this(0, 0)
    {
        Name = name;
    }

    public MemDb() : this(0, 0)
    {
    }

    public MemDb(int writeDelay, int readDelay)
    {
        _writeDelay = writeDelay;
        _readDelay = readDelay;
        _db = new ConcurrentDictionary<byte[], byte[]>(new ByteArrayComparer());
    }

    public string Name { get; }

    public byte[]? this[byte[] key]
    {
        get
        {
            if (_readDelay > 0)
            {
                Thread.Sleep(_readDelay);
            }

            ReadsCount++;
            return _db.ContainsKey(key) ? _db[key] : null;
        }
        set
        {
            if (_writeDelay > 0)
            {
                Thread.Sleep(_writeDelay);
            }

            WritesCount++;
            _db[key] = value;
        }
    }

    public KeyValuePair<byte[], byte[]>[] this[byte[][] keys]
    {
        get
        {
            if (_readDelay > 0)
            {
                Thread.Sleep(_readDelay);
            }

            ReadsCount += keys.Length;
            return keys.Select(k => new KeyValuePair<byte[], byte[]>(k, _db.TryGetValue(k, out var value) ? value : null)).ToArray();
        }
    }

    public void Remove(byte[] key)
    {
        _db.TryRemove(key, out _);
    }

    public bool KeyExists(byte[] key)
    {
        return _db.ContainsKey(key);
    }

    public IDb Innermost => this;

    public void Flush()
    {
    }

    public void Clear()
    {
        _db.Clear();
    }

    public IEnumerable<KeyValuePair<byte[], byte[]?>> GetAll(bool ordered = false) => _db;

    public IEnumerable<byte[]> GetAllValues(bool ordered = false) => Values;

    public IBatch StartBatch()
    {
        return this.LikeABatch();
    }

    public ICollection<byte[]> Keys => _db.Keys;
    public ICollection<byte[]> Values => _db.Values;

    public int Count => _db.Count;

    public void Dispose()
    {
    }

    public Span<byte> GetSpan(byte[] key)
    {
        return this[key].AsSpan();
    }

    public void DangerousReleaseMemory(in Span<byte> span)
    {
    }
}

public interface IFullDb : IDb
{
    ICollection<byte[]> Keys { get; }

    ICollection<byte[]?> Values { get; }

    int Count { get; }
}

public interface IDb : IKeyValueStoreWithBatching, IDisposable
{
    string Name { get; }
    KeyValuePair<byte[], byte[]?>[] this[byte[][] keys] { get; }
    IEnumerable<KeyValuePair<byte[], byte[]>> GetAll(bool ordered = false);
    IEnumerable<byte[]> GetAllValues(bool ordered = false);
    void Remove(byte[] key);
    bool KeyExists(byte[] key);

    void Flush();

    void Clear();

    public IReadOnlyDb CreateReadOnly(bool createInMemWriteStore) => new ReadOnlyDb(this, createInMemWriteStore);
}

public interface IKeyValueStoreWithBatching : IKeyValueStore
{
    IBatch StartBatch();
}


public interface IKeyValueStore : IReadOnlyKeyValueStore
{
    new byte[]? this[byte[] key] { get; set; }
}

public interface IReadOnlyKeyValueStore
{
    byte[]? this[byte[] key] { get; }
}

public interface IBatch : IDisposable, IKeyValueStore { }


public class ReadOnlyDb : IReadOnlyDb, IDbWithSpan
{
    private readonly MemDb _memDb = new();

    private readonly IDb _wrappedDb;
    private readonly bool _createInMemWriteStore;

    public ReadOnlyDb(IDb wrappedDb, bool createInMemWriteStore)
    {
        _wrappedDb = wrappedDb;
        _createInMemWriteStore = createInMemWriteStore;
    }

    public void Dispose()
    {
        _memDb.Dispose();
    }

    public string Name { get; } = "ReadOnlyDb";

    public byte[]? this[byte[] key]
    {
        get => _memDb[key] ?? _wrappedDb[key];
        set
        {
            if (!_createInMemWriteStore)
            {
                throw new InvalidOperationException($"This {nameof(ReadOnlyDb)} did not expect any writes.");
            }

            _memDb[key] = value;
        }
    }

    public KeyValuePair<byte[], byte[]>[] this[byte[][] keys]
    {
        get
        {
            var result = _wrappedDb[keys];
            var memResult = _memDb[keys];
            for (int i = 0; i < memResult.Length; i++)
            {
                var memValue = memResult[i];
                if (memValue.Value is not null)
                {
                    result[i] = memValue;
                }
            }

            return result;
        }
    }

    public IEnumerable<KeyValuePair<byte[], byte[]>> GetAll(bool ordered = false) => _memDb.GetAll();

    public IEnumerable<byte[]> GetAllValues(bool ordered = false) => _memDb.GetAllValues();

    public IBatch StartBatch()
    {
        return this.LikeABatch();
    }

    public void Remove(byte[] key) { }

    public bool KeyExists(byte[] key)
    {
        return _memDb.KeyExists(key) || _wrappedDb.KeyExists(key);
    }

    public void Flush()
    {
        _wrappedDb.Flush();
        _memDb.Flush();
    }

    public void Clear() { throw new InvalidOperationException(); }

    public virtual void ClearTempChanges()
    {
        _memDb.Clear();
    }

    public Span<byte> GetSpan(byte[] key) => this[key].AsSpan();

    public void DangerousReleaseMemory(in Span<byte> span) { }
}

public interface IReadOnlyDb : IDb
{
    void ClearTempChanges();
}

public static class KeyValueStoreExtensions
{
    public static IBatch LikeABatch(this IKeyValueStoreWithBatching keyValueStore)
    {
        return LikeABatch(keyValueStore, null);
    }

    public static IBatch LikeABatch(this IKeyValueStoreWithBatching keyValueStore, Action? onDispose)
    {
        return new FakeBatch(keyValueStore, onDispose);
    }
}

public class FakeBatch : IBatch
{
    private readonly IKeyValueStore _storePretendingToSupportBatches;

    private readonly Action? _onDispose;

    public FakeBatch(IKeyValueStore storePretendingToSupportBatches)
        : this(storePretendingToSupportBatches, null)
    {
    }

    public FakeBatch(IKeyValueStore storePretendingToSupportBatches, Action? onDispose)
    {
        _storePretendingToSupportBatches = storePretendingToSupportBatches;
        _onDispose = onDispose;
    }

    public void Dispose()
    {
        _onDispose?.Invoke();
        GC.SuppressFinalize(this);
    }

    public byte[]? this[byte[] key]
    {
        get => _storePretendingToSupportBatches[key];
        set => _storePretendingToSupportBatches[key] = value;
    }
}

public interface IDbWithSpan : IDb
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <returns>Can return null or empty Span on missing key</returns>
    Span<byte> GetSpan(byte[] key);
    void DangerousReleaseMemory(in Span<byte> span);
}

