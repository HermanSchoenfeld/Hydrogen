// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System.Collections.Generic;
using System.Linq;
using Sphere10.Framework;

namespace Sphere10.Framework.Consensus;

/// <summary>
/// Helper methods for computing merkle roots over blockchain blocks and operations.
/// </summary>
public static class MerkleHelper {

	/// <summary>
	/// Computes the merkle root of a block's operations using SHA2-256.
	/// </summary>
	public static byte[] ComputeBlockMerkleRoot<TState, TBlockID, TWeight, TOperationID>(IBlockchainBlock<TState, TBlockID, TWeight, TOperationID> block) where TState : IBlockchainState
		=> ComputeBlockMerkleRoot<TState, TBlockID, TWeight, TOperationID>(block, CHF.SHA2_256);

	/// <summary>
	/// Computes the merkle root of a block's operations using the specified hash function.
	/// </summary>
	public static byte[] ComputeBlockMerkleRoot<TState, TBlockID, TWeight, TOperationID>(IBlockchainBlock<TState, TBlockID, TWeight, TOperationID> block, CHF chf) where TState : IBlockchainState {
		Guard.ArgumentNotNull(block, nameof(block));
		return MerkleTree.ComputeMerkleRoot(block.Operations.Select(op => op.GetHashCode()), chf);
	}

	/// <summary>
	/// Computes the merkle root of a collection of pre-hashed operation digests.
	/// </summary>
	public static byte[] ComputeOperationsMerkleRoot(IEnumerable<byte[]> operationDigests)
		=> ComputeOperationsMerkleRoot(operationDigests, CHF.SHA2_256);

	/// <summary>
	/// Computes the merkle root of a collection of pre-hashed operation digests using the specified hash function.
	/// </summary>
	public static byte[] ComputeOperationsMerkleRoot(IEnumerable<byte[]> operationDigests, CHF chf) {
		Guard.ArgumentNotNull(operationDigests, nameof(operationDigests));
		return MerkleTree.ComputeMerkleRoot(operationDigests, chf);
	}
}
