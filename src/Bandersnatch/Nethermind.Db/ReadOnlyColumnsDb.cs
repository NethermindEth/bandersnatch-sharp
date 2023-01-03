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

namespace Nethermind.Db
{
    public class ReadOnlyColumnsDb<T> : ReadOnlyDb, IColumnsDb<T>
    {
        private readonly IDictionary<T, ReadOnlyDb> _columnDbs = new Dictionary<T, ReadOnlyDb>();
        private readonly bool _createInMemWriteStore;
        private readonly IColumnsDb<T> _wrappedDb;

        public ReadOnlyColumnsDb(IColumnsDb<T> wrappedDb, bool createInMemWriteStore) : base(wrappedDb, createInMemWriteStore)
        {
            _wrappedDb = wrappedDb;
            _createInMemWriteStore = createInMemWriteStore;
        }

        public IDbWithSpan GetColumnDb(T key)
        {
            return _columnDbs.TryGetValue(key, out ReadOnlyDb? db) ? db : _columnDbs[key] = new ReadOnlyDb(_wrappedDb.GetColumnDb(key), _createInMemWriteStore);
        }

        public IEnumerable<T> ColumnKeys => _wrappedDb.ColumnKeys;

        public IReadOnlyDb CreateReadOnly(bool createInMemWriteStore)
        {
            return new ReadOnlyColumnsDb<T>(this, createInMemWriteStore);
        }

        public override void ClearTempChanges()
        {
            base.ClearTempChanges();
            foreach (ReadOnlyDb columnDbsValue in _columnDbs.Values)
            {
                columnDbsValue.ClearTempChanges();
            }
        }
    }
}
