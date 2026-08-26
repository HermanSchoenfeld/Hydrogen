// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using NUnit.Framework;
using Sphere10.Framework.Consensus;
using Sphere10.Framework.Consensus.Tests.ReferenceChain;

namespace Sphere10.Framework.Consensus.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
public class PrimitivesTests {

	[Test]
	public void Operation_ApplyAndUndo_AreInverse() {
		var state = new AccountState();
		var operation = new AccountOperation("alice", 100);

		operation.Apply(state);
		Assert.That(state.GetBalance("alice"), Is.EqualTo(100L));

		operation.Undo(state);
		Assert.That(state.GetBalance("alice"), Is.EqualTo(0L));
	}

	[Test]
	public void Block_Apply_AppliesAllOperationsInOrder() {
		var state = new AccountState();
		var block = new AccountBlock(
			new AccountOperation("alice", 100),
			new AccountOperation("bob", 50),
			new AccountOperation("alice", -30)
		);

		block.Apply(state);

		Assert.That(state.GetBalance("alice"), Is.EqualTo(70L));
		Assert.That(state.GetBalance("bob"), Is.EqualTo(50L));
	}

	[Test]
	public void Block_Undo_ReversesAllOperationsInReverseOrder() {
		var state = new AccountState();
		var block = new AccountBlock(
			new AccountOperation("alice", 100),
			new AccountOperation("bob", 50),
			new AccountOperation("alice", -30)
		);

		block.Apply(state);
		block.Undo(state);

		Assert.That(state.GetBalance("alice"), Is.EqualTo(0L));
		Assert.That(state.GetBalance("bob"), Is.EqualTo(0L));
	}

	[Test]
	public void Block_ApplyUndoApply_RestoresSameState() {
		var state = new AccountState();
		var block = new AccountBlock(
			new AccountOperation("alice", 100),
			new AccountOperation("bob", 50)
		);

		block.Apply(state);
		var balanceAfterFirstApply = state.GetBalance("alice");

		block.Undo(state);
		block.Apply(state);

		Assert.That(state.GetBalance("alice"), Is.EqualTo(balanceAfterFirstApply));
	}

	[Test]
	public void BlockchainOperationBase_ThrowsOnNullState() {
		var operation = new AccountOperation("alice", 100);

		Assert.That(() => operation.Apply(null!), Throws.ArgumentNullException);
		Assert.That(() => operation.Undo(null!), Throws.ArgumentNullException);
	}
}
