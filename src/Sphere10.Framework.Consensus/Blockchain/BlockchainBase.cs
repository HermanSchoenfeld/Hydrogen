// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;

namespace Sphere10.Framework.Consensus;

/// <summary>
/// Abstract base implementation of <see cref="IBlockchain{TBlock, TState, TBlockID, TWeight, TOperationID}"/>.
/// Provides event wiring and shared members; subclasses implement Apply/Undo.
/// </summary>
public abstract class BlockchainBase<TBlock, TState, TBlockID, TWeight, TOperationID> : IBlockchain<TBlock, TState, TBlockID, TWeight, TOperationID>
	where TBlock : IBlockchainBlock<TState, TBlockID, TWeight, TOperationID>
	where TState : IBlockchainState {

	public event EventHandlerEx<LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>> BlockApplied;
	public event EventHandlerEx<LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>> BlockUndone;

	public abstract TState State { get; }

	public abstract IWeightAggregator<TWeight> WeightAggregator { get; }

	public abstract long Height { get; }

	public abstract IReadOnlyList<LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>> Blocks { get; }

	public abstract LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID> Head { get; }

	public abstract TWeight AggregatedWeight { get; }

	public abstract void ApplyBlock(TBlock block);

	public abstract LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID> UndoBlock();

	protected virtual void NotifyBlockApplied(LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID> block)
		=> BlockApplied?.Invoke(block);

	protected virtual void NotifyBlockUndone(LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID> block)
		=> BlockUndone?.Invoke(block);
}
