// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using System.IO.Abstractions;
using Nethermind.Config;
using Nethermind.Db;
using Nethermind.Db.Rocks;
using Nethermind.Db.Rocks.Config;
using Nethermind.Utils;

namespace Nethermind.Verkle.Db;

public enum DbMode
{
    [ConfigItem(Description = "Diagnostics mode which uses an in-memory DB")]
    MemDb,
    [ConfigItem(Description = "Diagnostics mode which uses an Persistant DB")]
    PersistantDb,
    [ConfigItem(Description = "Diagnostics mode which uses a read-only DB")]
    ReadOnlyDb,
}

public class DbFactory
{
    private static (IDbProvider DbProvider, RocksDbFactory RocksDbFactory, MemDbFactory MemDbFactory) InitDbApi(DbMode diagnosticMode, string baseDbPath, bool storeReceipts)
    {
        DbConfig dbConfig = new DbConfig();
        DisposableStack disposeStack = new DisposableStack();
        IDbProvider dbProvider;
        RocksDbFactory rocksDbFactory;
        MemDbFactory memDbFactory;
        switch (diagnosticMode)
        {
            case DbMode.ReadOnlyDb:
                DbProvider rocksDbProvider = new DbProvider(DbModeHint.Persisted);
                dbProvider = new ReadOnlyDbProvider(rocksDbProvider, storeReceipts); // ToDo storeReceipts as createInMemoryWriteStore - bug?
                disposeStack.Push(rocksDbProvider);
                rocksDbFactory = new RocksDbFactory(dbConfig, Path.Combine(baseDbPath, "debug"));
                memDbFactory = new MemDbFactory();
                break;
            case DbMode.MemDb:
                dbProvider = new DbProvider(DbModeHint.Mem);
                rocksDbFactory = new RocksDbFactory(dbConfig, Path.Combine(baseDbPath, "debug"));
                memDbFactory = new MemDbFactory();
                break;
            case DbMode.PersistantDb:
                dbProvider = new DbProvider(DbModeHint.Persisted);
                rocksDbFactory = new RocksDbFactory(dbConfig, baseDbPath);
                memDbFactory = new MemDbFactory();
                break;
            default:
                throw new ArgumentException();

        }

        return (dbProvider, rocksDbFactory, memDbFactory);
    }

    public static IDbProvider InitDatabase(DbMode dbMode, string? dbPath)
    {
        (IDbProvider dbProvider, RocksDbFactory rocksDbFactory, MemDbFactory memDbFactory) = InitDbApi(dbMode, dbPath ?? "testDb", true);
        StandardDbInitializer dbInitializer = new StandardDbInitializer(dbProvider, rocksDbFactory, memDbFactory, new FileSystem(), false);
        dbInitializer.InitStandardDbs(true);
        return dbProvider;
    }

}
