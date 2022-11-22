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
//

namespace Nethermind.Db
{
    public class DbSettings
    {
        public DbSettings(string name, string path)
        {
            DbName = name;
            DbPath = path;
        }

        public string DbName { get; private set; }
        public string DbPath { get; private set; }

        public Action? UpdateReadMetrics { get; init; }
        public Action? UpdateWriteMetrics { get; init; }

        public ulong? WriteBufferSize { get; init; }
        public uint? WriteBufferNumber { get; init; }
        public ulong? BlockCacheSize { get; init; }
        public bool? CacheIndexAndFilterBlocks { get; init; }

        public bool DeleteOnStart { get; set; }
        public bool CanDeleteFolder { get; set; } = true;

        public DbSettings Clone(string name, string path)
        {
            DbSettings settings = (DbSettings)MemberwiseClone();
            settings.DbName = name;
            settings.DbPath = path;
            return settings;
        }

        public DbSettings Clone() => (DbSettings)MemberwiseClone();

        public override string ToString() => $"{DbName}:{DbPath}";
    }
}
