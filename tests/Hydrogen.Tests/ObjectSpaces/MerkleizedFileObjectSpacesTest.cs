﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using System.Linq;
using Hydrogen.ObjectSpaces;
using NUnit.Framework;


namespace Hydrogen.Tests;

[TestFixture, Timeout(60000)]
public class MerkleizedFileObjectSpacesTest : FileObjectSpacesTest {

	protected override FileObjectSpace CreateFileObjectSpace(string folder, bool keepFilesOnDispose, ObjectSpaceBuilder builder) 
		=> base.CreateFileObjectSpace(folder, keepFilesOnDispose, builder.Merkleized());

	[Test]
	public void CheckRootsChanged() {
		using var objectSpace = CreateObjectSpace();
		var chf = objectSpace.Definition.HashFunction;
		var digestSize = Hashers.GetDigestSizeBytes(chf);

		var savedAccount = CreateAccount();
		var accountDigest = Hashers.Hash(chf, objectSpace.Serializers.GetSerializer<Account>().SerializeBytesLE(savedAccount));
		objectSpace.Save(savedAccount);

		objectSpace.Commit();

		var dim1 = objectSpace.Dimensions[0];
		var dim2 = objectSpace.Dimensions[1];

		// Verify account dimension has single item root
		using var dim1Scope = dim1.ObjectStream.EnterAccessScope();
		var accountRoot = dim1.ObjectStream.Streams.Header.MapExtensionProperty(0, digestSize, new ConstantSizeByteArraySerializer(digestSize)).Value;
		Assert.That(accountRoot, Is.EqualTo(accountDigest).Using(ByteArrayEqualityComparer.Instance));

		// Verify identity dimension has null root  (note: null is stored as 0 bytes)
		using var dim2Scope = dim2.ObjectStream.EnterAccessScope();
		var identityRoot = dim2.ObjectStream.Streams.Header.MapExtensionProperty(0, digestSize, new ConstantSizeByteArraySerializer(digestSize)).Value;
		Assert.That(identityRoot, Is.EqualTo(new byte[digestSize]).Using(ByteArrayEqualityComparer.Instance));
			
		// Verify spatial root is both account/identity
		var spaceRoot = objectSpace.Streams.Header.MapExtensionProperty(0, digestSize, new ConstantSizeByteArraySerializer(digestSize)).Value;
		Assert.That(spaceRoot, Is.EqualTo(MerkleTree.ComputeMerkleRoot(new [] { accountRoot, identityRoot }, chf)).Using(ByteArrayEqualityComparer.Instance));

	}
}
