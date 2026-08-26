// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

namespace Sphere10.Framework.Consensus.Tests.ReferenceChain;

/// <summary>
/// A single account balance adjustment that is fully invertible.
/// </summary>
public class AccountOperation : BlockchainOperationBase<AccountState, int> {
	private static int _nextId;
	private readonly int _id;

	public AccountOperation(string account, long amount) {
		_id = System.Threading.Interlocked.Increment(ref _nextId);
		Account = account;
		Amount = amount;
	}

	public override int ID => _id;

	public string Account { get; }

	public long Amount { get; }

	protected override void ApplyInternal(AccountState state)
		=> state.AdjustBalance(Account, Amount);

	protected override void UndoInternal(AccountState state)
		=> state.AdjustBalance(Account, -Amount);
}
