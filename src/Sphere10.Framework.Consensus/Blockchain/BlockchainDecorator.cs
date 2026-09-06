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
/// Decorator pattern for <see cref="IBlockchain{TBlock, TState, TBlockID, TWeight, TOperationID}"/>.
/// Routes all calls to the decorated internal blockchain. The <typeparamref name="TConcrete"/>
/// generic argument ensures sub-classes can retrieve the decorated blockchain in its type,
/// without an expensive chain of casts/retrieves.
/// </summary>
public abstract class BlockchainDecorator<TBlock, TState, TBlockID, TWeight, TOperationID, TConcrete> : IBlockchain<TBlock, TState, TBlockID, TWeight, TOperationID>
	where TBlock : IBlockchainBlock<TState, TBlockID, TWeight, TOperationID>
	where TState : IBlockchainState
	where TConcrete : IBlockchain<TBlock, TState, TBlockID, TWeight, TOperationID> {

	protected readonly TConcrete InternalBlockchain;

	protected BlockchainDecorator(TConcrete internalBlockchain) {
		Guard.ArgumentNotNull(internalBlockchain, nameof(internalBlockchain));
		InternalBlockchain = internalBlockchain;
	}

	public event EventHandlerEx<LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>> BlockApplied {
		add => InternalBlockchain.BlockApplied += value;
		remove => InternalBlockchain.BlockApplied -= value;
	}

	public event EventHandlerEx<LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>> BlockUndone {
		add => InternalBlockchain.BlockUndone += value;
		remove => InternalBlockchain.BlockUndone -= value;
	}

	public virtual TState State => InternalBlockchain.State;

	public virtual IWeightAggregator<TWeight> WeightAggregator => InternalBlockchain.WeightAggregator;

	public virtual long Height => InternalBlockchain.Height;

	public virtual IReadOnlyList<LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>> Blocks => InternalBlockchain.Blocks;

	public virtual LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID> Head => InternalBlockchain.Head;

	public virtual TWeight AggregatedWeight => InternalBlockchain.AggregatedWeight;

	public virtual void ApplyBlock(TBlock block) => InternalBlockchain.ApplyBlock(block);

	public virtual LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID> UndoBlock() => InternalBlockchain.UndoBlock();
}

/// <summary>
/// Decorator pattern for <see cref="IBlockchain{TBlock, TState, TBlockID, TWeight, TOperationID}"/>
/// with the default interface-typed internal reference.
/// </summary>
public abstract class BlockchainDecorator<TBlock, TState, TBlockID, TWeight, TOperationID> : BlockchainDecorator<TBlock, TState, TBlockID, TWeight, TOperationID, IBlockchain<TBlock, TState, TBlockID, TWeight, TOperationID>>
	where TBlock : IBlockchainBlock<TState, TBlockID, TWeight, TOperationID>
	where TState : IBlockchainState {

	protected BlockchainDecorator(IBlockchain<TBlock, TState, TBlockID, TWeight, TOperationID> internalBlockchain)
		: base(internalBlockchain) {
	}
}
