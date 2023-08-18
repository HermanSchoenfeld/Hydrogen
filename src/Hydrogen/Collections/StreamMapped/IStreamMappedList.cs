﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System.Collections.Generic;

namespace Hydrogen;

public interface IStreamMappedList<TItem> : IExtendedList<TItem>, ILoadable {
	StreamContainer Streams { get; }

	IItemSerializer<TItem> ItemSerializer { get; }

	IEqualityComparer<TItem> ItemComparer { get; }

	ClusteredStream EnterAddScope(TItem item);

	ClusteredStream EnterInsertScope(long index, TItem item);

	ClusteredStream EnterUpdateScope(long index, TItem item);
}
