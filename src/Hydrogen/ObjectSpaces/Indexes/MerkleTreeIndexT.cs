﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;

namespace Hydrogen.ObjectSpaces;

/// <inheritdoc />
internal class MerkleTreeIndex<TItem> : MerkleTreeIndex {
	public MerkleTreeIndex(ObjectContainer<TItem> objectContainer, Func<TItem, long, byte[]> digestor, CHF hashAlgorithm, long reservedStreamIndex)
		: base(
			objectContainer,
			reservedStreamIndex,
			index => digestor(objectContainer.LoadItem(index), index),
			hashAlgorithm
		) {
	}

	protected new ObjectContainer<TItem> Container => (ObjectContainer<TItem>)((IObjectContainerAttachment)this).Container;
}
