// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System.Collections.Generic;

namespace Sphere10.Framework.Consensus.Tests.ReferenceChain;

/// <summary>
/// A block of account operations for the reference account-model chain.
/// </summary>
public class AccountBlock : BlockchainBlock<AccountState, int, long, int> {
	private static int _nextId;
	private readonly int _id;

	public AccountBlock(IEnumerable<IBlockchainOperation<AccountState, int>> operations)
		: base(operations) {
		_id = System.Threading.Interlocked.Increment(ref _nextId);
	}

	public AccountBlock(params IBlockchainOperation<AccountState, int>[] operations)
		: base(operations) {
		_id = System.Threading.Interlocked.Increment(ref _nextId);
	}

	public override int ID => _id;

	public override long Weight => 1L;
}
