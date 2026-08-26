// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using System.Linq;
using Sphere10.Framework;

namespace Sphere10.Framework.Consensus;

public class BlockchainBlock<TState, TBlockID, TWeight, TOperationID> : IBlockchainBlock<TState, TBlockID, TWeight, TOperationID> where TState : IBlockchainState {
	public BlockchainBlock(IEnumerable<IBlockchainOperation<TState, TOperationID>> operations) {
		Guard.ArgumentNotNull(operations, nameof(operations));

		Operations = operations as IReadOnlyList<IBlockchainOperation<TState, TOperationID>> ?? operations.ToArray();
	}

	public virtual TBlockID ID => default;

	public virtual TWeight Weight => default;

	public IReadOnlyList<IBlockchainOperation<TState, TOperationID>> Operations { get; }

	public virtual void Apply(TState state) {
		Guard.ArgumentNotNull(state, nameof(state));

		foreach (var operation in Operations)
			operation.Apply(state);
	}

	public virtual void Undo(TState state) {
		Guard.ArgumentNotNull(state, nameof(state));

		for (var i = Operations.Count - 1; i >= 0; i--)
			Operations[i].Undo(state);
	}
}
