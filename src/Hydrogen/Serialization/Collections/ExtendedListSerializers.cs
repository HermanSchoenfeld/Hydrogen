﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

namespace Hydrogen;

public class ExtendedListSerializer<T> : CollectionSerializerBase<ExtendedList<T>, T> {

	public ExtendedListSerializer(IItemSerializer<T> itemSerializer, SizeDescriptorStrategy sizeDescriptorStrategy = SizeDescriptorStrategy.UseCVarInt) 
		: base(itemSerializer, sizeDescriptorStrategy) {
	}

	protected override long GetLength(ExtendedList<T> collection) => collection.Count;

	protected override ExtendedList<T> Activate(long capacity) => new ExtendedList<T>(capacity);

	protected override void SetItem(ExtendedList<T> collection, long index, T item) {
		Guard.Ensure(collection.Count == index, "Unexpected index");
		collection.Add(item);
	}
}
