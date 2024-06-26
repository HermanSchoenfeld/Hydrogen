﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT NON-AI software license, see the accompanying file
// LICENSE or visit https://sphere10.com/legal/NON-AI-MIT.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.
//
// NOTE: This file is part of the reference implementation for Dynamic Merkle-Trees. Read the paper at:
// Web: https://sphere10.com/tech/dynamic-merkle-trees
// e-print: https://vixra.org/abs/2305.0087

using System;

namespace Hydrogen;

public record MerkleNode : IEquatable<MerkleNode> {
	public readonly MerkleCoordinate Coordinate;
	public readonly byte[] Hash;

	public MerkleNode(MerkleCoordinate coordinate, byte[] hash) {
		Coordinate = coordinate;
		Hash = hash;
	}

	public override int GetHashCode() {
		unchecked {
			return Coordinate.GetHashCode() * 397 ^ ByteArrayEqualityComparer.Instance.GetHashCode(Hash);
		}
	}
}
