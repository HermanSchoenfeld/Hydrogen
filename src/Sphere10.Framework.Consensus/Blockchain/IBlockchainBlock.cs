// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System.Collections.Generic;

namespace Sphere10.Framework.Consensus;

public interface IBlockchainBlock<in TState, out TBlockID, out TWeight, TOperationID> where TState : IBlockchainState {
	TBlockID ID { get; }

	TWeight Weight { get; }

	IReadOnlyList<IBlockchainOperation<TState, TOperationID>> Operations { get; }

	void Apply(TState state);

	void Undo(TState state);
}
