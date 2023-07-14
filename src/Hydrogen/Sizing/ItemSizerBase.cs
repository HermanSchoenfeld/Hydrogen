﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Hydrogen;

public abstract class ItemSizerBase<TItem> : IItemSizer<TItem> {

	public virtual bool IsStaticSize => false;

	public virtual long StaticSize => throw new InvalidOperationException("Item sizer is not static");

	public virtual long CalculateTotalSize(IEnumerable<TItem> items, bool calculateIndividualItems, out long[] itemSizes) {
		var sizes = items.Select(CalculateSize).ToArray();
		itemSizes = calculateIndividualItems ? sizes : null;
		return sizes.Sum();
	}

	public abstract long CalculateSize(TItem item);
}