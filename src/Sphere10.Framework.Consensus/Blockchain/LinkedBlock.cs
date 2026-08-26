// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;

namespace Sphere10.Framework.Consensus;

/// <summary>
/// Represents a block in the context of a blockchain, irrespective of whether it is
/// finalized (linear chain) or potential (unfinalized frontier). Contains the block,
/// its parent block ID, and its aggregated weight computed when added to a chain.
/// </summary>
public sealed class LinkedBlock<TBlock, TState, TBlockID, TWeight, TOperationID>
	where TBlock : IBlockchainBlock<TState, TBlockID, TWeight, TOperationID>
	where TState : IBlockchainState {

	public LinkedBlock(TBlock block, TBlockID parentID, bool hasParent, TWeight aggregatedWeight) {
		Block = block ?? throw new ArgumentNullException(nameof(block));
		ParentID = parentID;
		HasParent = hasParent;
		AggregatedWeight = aggregatedWeight;
	}

	public TBlock Block { get; }

	public TBlockID ParentID { get; }

	public bool HasParent { get; }

	public TWeight AggregatedWeight { get; }

	public bool IsRoot => !HasParent;
}
