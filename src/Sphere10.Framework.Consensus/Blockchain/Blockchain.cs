// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using Sphere10.Framework;

namespace Sphere10.Framework.Consensus;

/// <summary>
/// A concrete linear blockchain backed by an in-memory list of linked blocks.
/// </summary>
public class InMemoryBlockchain<TBlock, TState, TBlockID, TWeight, TOperationID> : BlockchainBase<TBlock, TState, TBlockID, TWeight, TOperationID>
	where TBlock : IBlockchainBlock<TState, TBlockID, TWeight, TOperationID>
	where TState : IBlockchainState {

	private readonly List<LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>> _blocks;
	private readonly IWeightAggregator<TWeight> _weightAggregator;

	public InMemoryBlockchain(TState state, IWeightAggregator<TWeight> weightAggregator)
		: this(state, weightAggregator, Array.Empty<TBlock>()) {
	}

	public InMemoryBlockchain(TState state, IWeightAggregator<TWeight> weightAggregator, IEnumerable<TBlock> blocks) {
		State = state ?? throw new ArgumentNullException(nameof(state));
		_weightAggregator = weightAggregator ?? throw new ArgumentNullException(nameof(weightAggregator));
		_blocks = new List<LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>>();

		if (blocks == null)
			return;

		foreach (var block in blocks)
			ApplyBlock(block);
	}

	public override TState State { get; }

	public override IWeightAggregator<TWeight> WeightAggregator => _weightAggregator;

	public override long Height => _blocks.Count;

	public override IReadOnlyList<LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>> Blocks => _blocks;

	public override LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID> Head => _blocks.Count > 0 ? _blocks[_blocks.Count - 1] : default;

	public override TWeight AggregatedWeight => _blocks.Count > 0 ? _blocks[_blocks.Count - 1].AggregatedWeight : default;

	public override void ApplyBlock(TBlock block) {
		Guard.ArgumentNotNull(block, nameof(block));

		var aggregatedWeight = _blocks.Count > 0
			? _weightAggregator.Aggregate(_blocks[_blocks.Count - 1].AggregatedWeight, block.Weight)
			: block.Weight;

		var parentID = _blocks.Count > 0 ? _blocks[_blocks.Count - 1].Block.ID : default;
		var linkedBlock = new LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>(block, parentID, _blocks.Count > 0, aggregatedWeight);

		using (State.EnterUpdateScope()) {
			block.Apply(State);
			_blocks.Add(linkedBlock);
		}
		NotifyBlockApplied(linkedBlock);
	}

	public override LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID> UndoBlock() {
		Guard.Ensure(_blocks.Count > 0, "Cannot undo because blockchain is empty.");

		var linkedBlock = _blocks[_blocks.Count - 1];
		using (State.EnterUpdateScope()) {
			linkedBlock.Block.Undo(State);
			_blocks.RemoveAt(_blocks.Count - 1);
		}
		NotifyBlockUndone(linkedBlock);
		return linkedBlock;
	}
}
