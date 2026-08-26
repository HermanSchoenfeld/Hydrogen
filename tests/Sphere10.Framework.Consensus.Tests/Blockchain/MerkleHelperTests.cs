// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using NUnit.Framework;
using Sphere10.Framework;
using Sphere10.Framework.Consensus;
using Sphere10.Framework.Consensus.Tests.ReferenceChain;

namespace Sphere10.Framework.Consensus.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
public class MerkleHelperTests {

	[Test]
	public void ComputeBlockMerkleRoot_IsDeterministic() {
		var block = new AccountBlock(
			new AccountOperation("alice", 100),
			new AccountOperation("bob", 50)
		);

		var root1 = MerkleHelper.ComputeBlockMerkleRoot(block);
		var root2 = MerkleHelper.ComputeBlockMerkleRoot(block);

		Assert.That(root1, Is.EqualTo(root2));
	}

	[Test]
	public void ComputeOperationsMerkleRoot_EmptyCollection_ReturnsNull() {
		var root = MerkleHelper.ComputeOperationsMerkleRoot(System.Array.Empty<byte[]>());

		Assert.That(root, Is.Null);
	}

	[Test]
	public void ComputeOperationsMerkleRoot_SingleDigest_ReturnsDigest() {
		var digest = Hashers.Hash(CHF.SHA2_256, new byte[] { 1, 2, 3 });
		var root = MerkleHelper.ComputeOperationsMerkleRoot(new[] { digest });

		Assert.That(root, Is.EqualTo(digest));
	}
}
