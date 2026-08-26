// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using NUnit.Framework;
using Sphere10.Framework.Consensus;

namespace Sphere10.Framework.Consensus.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
public class CryptoToolTests {

	[Test]
	public void DeriveSecureChecksum_IsDeterministic() {
		var secret = new byte[] { 1, 2, 3, 4, 5 };

		var checksum1 = CryptoTool.DeriveSecureChecksum(secret);
		var checksum2 = CryptoTool.DeriveSecureChecksum(secret);

		Assert.That(checksum1, Is.EqualTo(checksum2));
	}

	[Test]
	public void DeriveSecureChecksum_DifferentSecrets_DifferentChecksums() {
		var secret1 = new byte[] { 1, 2, 3 };
		var secret2 = new byte[] { 4, 5, 6 };

		var checksum1 = CryptoTool.DeriveSecureChecksum(secret1);
		var checksum2 = CryptoTool.DeriveSecureChecksum(secret2);

		Assert.That(checksum1, Is.Not.EqualTo(checksum2));
	}

	[Test]
	public void DeriveChildDigest_IsDeterministic() {
		var digest = new byte[32];
		for (var i = 0; i < 32; i++)
			digest[i] = (byte)i;

		var child1 = CryptoTool.DeriveChildDigest(digest, 0UL);
		var child2 = CryptoTool.DeriveChildDigest(digest, 0UL);

		Assert.That(child1, Is.EqualTo(child2));
	}

	[Test]
	public void DeriveChildDigest_DifferentIndexes_DifferentDigests() {
		var digest = new byte[32];
		for (var i = 0; i < 32; i++)
			digest[i] = (byte)i;

		var child0 = CryptoTool.DeriveChildDigest(digest, 0UL);
		var child1 = CryptoTool.DeriveChildDigest(digest, 1UL);

		Assert.That(child0, Is.Not.EqualTo(child1));
	}

	[Test]
	public void DeriveChildDigest_DifferentParents_DifferentDigests() {
		var digest1 = new byte[32];
		var digest2 = new byte[32];
		digest2[0] = 1;

		var child1 = CryptoTool.DeriveChildDigest(digest1, 0UL);
		var child2 = CryptoTool.DeriveChildDigest(digest2, 0UL);

		Assert.That(child1, Is.Not.EqualTo(child2));
	}
}
