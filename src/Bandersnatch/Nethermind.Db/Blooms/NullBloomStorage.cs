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
using Nethermind.Utils;

namespace Nethermind.Db.Blooms
{
    public class NullBloomStorage : IBloomStorage
    {
        private NullBloomStorage()
        {
        }

        public static NullBloomStorage Instance { get; } = new NullBloomStorage();
        public long MaxBlockNumber { get; } = 0;
        public long MinBlockNumber { get; } = long.MaxValue;
        public long MigratedBlockNumber { get; } = -1;

        public void Store(long blockNumber, Bloom bloom) { }

        public IBloomEnumeration GetBlooms(long fromBlock, long toBlock)
        {
            return new NullBloomEnumerator();
        }

        public bool ContainsRange(in long fromBlockNumber, in long toBlockNumber)
        {
            return false;
        }

        public IEnumerable<Average> Averages { get; } = Array.Empty<Average>();

        public void Dispose() { }


        private class NullBloomEnumerator : IBloomEnumeration
        {
            public IEnumerator<Bloom> GetEnumerator()
            {
                return Enumerable.Empty<Bloom>().GetEnumerator();
            }

            public bool TryGetBlockNumber(out long blockNumber)
            {
                blockNumber = default;
                return false;
            }

            public (long FromBlock, long ToBlock) CurrentIndices { get; } = (0, 0);

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
