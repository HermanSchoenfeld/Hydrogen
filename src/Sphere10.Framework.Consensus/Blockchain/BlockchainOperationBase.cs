// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using Sphere10.Framework;

namespace Sphere10.Framework.Consensus;

/// <summary>
/// Base implementation of <see cref="IBlockchainOperation{TState, TOperationID}"/> that enforces non-null state
/// and delegates the actual mutation to overridable methods.
/// </summary>
public abstract class BlockchainOperationBase<TState, TOperationID> : IBlockchainOperation<TState, TOperationID> where TState : IBlockchainState {

	public abstract TOperationID ID { get; }

	public void Apply(TState state) {
		Guard.ArgumentNotNull(state, nameof(state));
		ApplyInternal(state);
	}

	public void Undo(TState state) {
		Guard.ArgumentNotNull(state, nameof(state));
		UndoInternal(state);
	}

	protected abstract void ApplyInternal(TState state);

	protected abstract void UndoInternal(TState state);
}
