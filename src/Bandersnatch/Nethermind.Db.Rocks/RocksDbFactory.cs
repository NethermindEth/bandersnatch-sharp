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

using Nethermind.Db.Rocks.Config;

namespace Nethermind.Db.Rocks;

public class RocksDbFactory : IRocksDbFactory
{
    private readonly IDbConfig _dbConfig;

    private readonly string _basePath;

    public RocksDbFactory(IDbConfig dbConfig, string basePath)
    {
        _dbConfig = dbConfig;
        _basePath = basePath;
    }

    public IDb CreateDb(DbSettings dbSettings) =>
        new DbOnTheRocks(_basePath, dbSettings, _dbConfig);

    public IColumnsDb<T> CreateColumnsDb<T>(DbSettings dbSettings) where T : struct, Enum =>
        new ColumnsDb<T>(_basePath, dbSettings, _dbConfig, Array.Empty<T>());

    public string GetFullDbPath(DbSettings dbSettings) => DbOnTheRocks.GetFullDbPath(dbSettings.DbPath, _basePath);
}
