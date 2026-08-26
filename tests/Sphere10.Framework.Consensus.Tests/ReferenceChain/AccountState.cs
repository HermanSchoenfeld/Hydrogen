// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;

namespace Sphere10.Framework.Consensus.Tests.ReferenceChain;

/// <summary>
/// A simple in-memory account-model ledger state for demonstration and testing.
/// </summary>
public class AccountState : BlockchainStateBase {
	private readonly Dictionary<string, long> _balances;

	public AccountState() {
		_balances = new Dictionary<string, long>();
	}

	public IReadOnlyDictionary<string, long> Balances => _balances;

	public long GetBalance(string account)
		=> _balances.TryGetValue(account, out var balance) ? balance : 0L;

	public void SetBalance(string account, long balance)
		=> _balances[account] = balance;

	public void AdjustBalance(string account, long delta)
		=> SetBalance(account, GetBalance(account) + delta);
}
