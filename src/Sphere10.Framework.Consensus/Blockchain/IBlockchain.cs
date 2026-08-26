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
/// A blockchain is a linear sequence of linked blocks with a single live state.
/// It is neither finalized nor unfinalized — those semantics are provided by decorators.
/// </summary>
public interface IBlockchain<TBlock, TState, TBlockID, TWeight, TOperationID>
	where TBlock : IBlockchainBlock<TState, TBlockID, TWeight, TOperationID>
	where TState : IBlockchainState {

	event EventHandlerEx<LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>> BlockApplied;

	event EventHandlerEx<LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>> BlockUndone;

	TState State { get; }

	IWeightAggregator<TWeight> WeightAggregator { get; }

	long Height { get; }

	IReadOnlyList<LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>> Blocks { get; }

	LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID> Head { get; }

	TWeight AggregatedWeight { get; }

	void ApplyBlock(TBlock block);

	LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID> UndoBlock();
}
