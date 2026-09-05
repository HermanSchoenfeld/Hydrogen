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
public class BlockchainTests {

	[Test]
	public void Blockchain_ApplyBlock_AdvancesHeightAndState() {
		var state = new AccountState();
		var chain = new Blockchain<AccountBlock, AccountState, int, long, int>(state, new LongWeightAggregator());

		var block1 = new AccountBlock(new AccountOperation("alice", 100));
		var block2 = new AccountBlock(new AccountOperation("alice", 50));

		chain.ApplyBlock(block1);
		chain.ApplyBlock(block2);

		Assert.That(chain.Height, Is.EqualTo(2L));
		Assert.That(chain.Head.Block, Is.SameAs(block2));
		Assert.That(chain.Head.AggregatedWeight, Is.EqualTo(2L));
		Assert.That(state.GetBalance("alice"), Is.EqualTo(150L));
	}

	[Test]
	public void Blockchain_UndoBlock_RevertsStateAndHeight() {
		var state = new AccountState();
		var chain = new Blockchain<AccountBlock, AccountState, int, long, int>(state, new LongWeightAggregator());

		chain.ApplyBlock(new AccountBlock(new AccountOperation("alice", 100)));
		chain.ApplyBlock(new AccountBlock(new AccountOperation("alice", 50)));

		var undone = chain.UndoBlock();

		Assert.That(chain.Height, Is.EqualTo(1L));
		Assert.That(state.GetBalance("alice"), Is.EqualTo(100L));
		Assert.That(undone, Is.Not.Null);
	}

	[Test]
	public void Blockchain_UndoBlock_OnEmptyChain_Throws() {
		var state = new AccountState();
		var chain = new Blockchain<AccountBlock, AccountState, int, long, int>(state, new LongWeightAggregator());

		Assert.That(() => chain.UndoBlock(), Throws.InvalidOperationException);
	}

	[Test]
	public void Blockchain_BlockAppliedEvent_FiresOnApply() {
		var state = new AccountState();
		var chain = new Blockchain<AccountBlock, AccountState, int, long, int>(state, new LongWeightAggregator());
		LinkedBlock<AccountBlock, AccountState, int, long, int>? appliedBlock = null;
		chain.BlockApplied += block => appliedBlock = block;

		var block = new AccountBlock(new AccountOperation("alice", 100));
		chain.ApplyBlock(block);

		Assert.That(appliedBlock, Is.Not.Null);
		Assert.That(appliedBlock!.Block, Is.SameAs(block));
	}

	[Test]
	public void Blockchain_BlockUndoneEvent_FiresOnUndo() {
		var state = new AccountState();
		var chain = new Blockchain<AccountBlock, AccountState, int, long, int>(state, new LongWeightAggregator());
		LinkedBlock<AccountBlock, AccountState, int, long, int>? undoneBlock = null;
		chain.BlockUndone += block => undoneBlock = block;

		var block = new AccountBlock(new AccountOperation("alice", 100));
		chain.ApplyBlock(block);
		chain.UndoBlock();

		Assert.That(undoneBlock, Is.Not.Null);
		Assert.That(undoneBlock!.Block, Is.SameAs(block));
	}

	[Test]
	public void Blockchain_ConstructorWithBlocks_AppliesAll() {
		var state = new AccountState();
		var blocks = new[] {
			new AccountBlock(new AccountOperation("alice", 100)),
			new AccountBlock(new AccountOperation("alice", 50))
		};

		var chain = new Blockchain<AccountBlock, AccountState, int, long, int>(state, new LongWeightAggregator(), blocks);

		Assert.That(chain.Height, Is.EqualTo(2L));
		Assert.That(state.GetBalance("alice"), Is.EqualTo(150L));
	}
}
