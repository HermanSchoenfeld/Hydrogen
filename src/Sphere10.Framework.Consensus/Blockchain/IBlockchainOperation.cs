// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

namespace Sphere10.Framework.Consensus;

public interface IBlockchainOperation<in TState, out TOperationID> where TState : IBlockchainState {
	TOperationID ID { get; }

	void Apply(TState state);

	void Undo(TState state);
}
