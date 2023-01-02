// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Core.Crypto;
using Nethermind.Int256;
using Nethermind.Utils.Crypto;

namespace Nethermind.Tree.Forest
{
    public class Account
    {
        public static Account TotallyEmpty = new();

        private static UInt256 _accountStartNonce = UInt256.Zero;

        // todo: change codeHash when this set
        public byte[]? Code;

        /// <summary>
        /// This is a special field that was used by some of the testnets (namely - Morden and Mordor).
        /// It makes all the account nonces start from a different number then zero,
        /// hence preventing potential signature reuse.
        /// It is no longer needed since the replay attack protection on chain ID is used now.
        /// We can remove it now but then we also need to remove any historical Mordor / Morden tests.
        /// </summary>
        public static UInt256 AccountStartNonce
        {
            set
            {
                _accountStartNonce = value;
                TotallyEmpty = new Account();
            }
        }

        public Account(UInt256 balance)
        {
            Balance = balance;
            Nonce = _accountStartNonce;
            CodeHash = Commitment.OfAnEmptyString;
            StorageRoot = Commitment.EmptyTreeHash;
            IsTotallyEmpty = Balance.IsZero;
            CodeSize = 0;
            Version = UInt256.Zero;
        }

        public Account(UInt256 balance, UInt256 nonce, Commitment codeHash, UInt256 codeSize, UInt256 version)
        {
            Balance = balance;
            Nonce = nonce;
            CodeHash = codeHash;
            StorageRoot = Commitment.EmptyTreeHash;
            IsTotallyEmpty = Balance.IsZero && Nonce == _accountStartNonce && CodeHash == Commitment.OfAnEmptyString && StorageRoot == Commitment.EmptyTreeHash;
            CodeSize = codeSize;
            Version = version;
        }

        public Account(UInt256 balance, UInt256 nonce, Commitment codeHash)
        {
            Balance = balance;
            Nonce = nonce;
            CodeHash = codeHash;
            StorageRoot = Commitment.EmptyTreeHash;
            IsTotallyEmpty = Balance.IsZero && Nonce == _accountStartNonce && CodeHash == Commitment.OfAnEmptyString && StorageRoot == Commitment.EmptyTreeHash;
            CodeSize = 0;
            Version = UInt256.Zero;
        }

        private Account()
        {
            Balance = UInt256.Zero;
            Nonce = _accountStartNonce;
            CodeHash = Commitment.OfAnEmptyString;
            StorageRoot = Commitment.EmptyTreeHash;
            IsTotallyEmpty = true;
            CodeSize = 0;
            Version = UInt256.Zero;
        }

        public Account(in UInt256 nonce, in UInt256 balance, Commitment storageRoot, Commitment codeHash)
        {
            Nonce = nonce;
            Balance = balance;
            StorageRoot = storageRoot;
            CodeHash = codeHash;
            IsTotallyEmpty = Balance.IsZero && Nonce == _accountStartNonce && CodeHash == Commitment.OfAnEmptyString && StorageRoot == Commitment.EmptyTreeHash;
            CodeSize = 0;
            Version = UInt256.Zero;
        }

        private Account(in UInt256 nonce, in UInt256 balance, Commitment storageRoot, Commitment codeHash, bool isTotallyEmpty)
        {
            Nonce = nonce;
            Balance = balance;
            StorageRoot = storageRoot;
            CodeHash = codeHash;
            IsTotallyEmpty = isTotallyEmpty;
            CodeSize = 0;
            Version = UInt256.Zero;
        }

        public bool HasCode => !CodeHash.Equals(Commitment.OfAnEmptyString);

        public bool HasStorage => !StorageRoot.Equals(Commitment.EmptyTreeHash);

        public UInt256 Nonce { get; }
        public UInt256 Balance { get; }
        public UInt256 CodeSize { get; }
        public UInt256 Version { get; }
        public Commitment StorageRoot { get; }
        public Commitment CodeHash { get; }
        public bool IsTotallyEmpty { get; }
        public bool IsEmpty => IsTotallyEmpty || (Balance.IsZero && Nonce == _accountStartNonce && CodeHash == Commitment.OfAnEmptyString);
        public bool IsContract => CodeHash != Commitment.OfAnEmptyString;

        public Account WithChangedBalance(in UInt256 newBalance)
        {
            return new(Nonce, newBalance, StorageRoot, CodeHash, IsTotallyEmpty && newBalance.IsZero);
        }

        public Account WithChangedNonce(in UInt256 newNonce)
        {
            return new(newNonce, Balance, StorageRoot, CodeHash, IsTotallyEmpty && newNonce == _accountStartNonce);
        }

        public Account WithChangedStorageRoot(Commitment newStorageRoot)
        {
            return new(Nonce, Balance, newStorageRoot, CodeHash, IsTotallyEmpty && newStorageRoot == Commitment.EmptyTreeHash);
        }

        public Account WithChangedCodeHash(Commitment newCodeHash)
        {
            // TODO: does the code and codeHash match?
            return new(Nonce, Balance, StorageRoot, newCodeHash, IsTotallyEmpty && newCodeHash == Commitment.OfAnEmptyString);
        }
    }
}
