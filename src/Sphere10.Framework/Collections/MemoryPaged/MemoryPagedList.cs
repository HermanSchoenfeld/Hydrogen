﻿using System;

namespace Sphere10.Framework {
    public class MemoryPagedList<TItem> : MemoryPagedListBase<TItem> {
	    private readonly IObjectSizer<TItem> _sizer;

	    public MemoryPagedList(int pageSize, int maxOpenPages, int fixedItemSize)
		    : this(pageSize, maxOpenPages, new ConstantObjectSizer<TItem>(fixedItemSize)) {
	    }

	    public MemoryPagedList(int pageSize, int maxOpenPages, Func<TItem, int> itemSizer)
		    : this(pageSize, maxOpenPages, new ActionObjectSizer<TItem>(itemSizer)) {
	    }

	    private MemoryPagedList(int pageSize, int maxOpenPages, IObjectSizer<TItem> sizer)
		    : base(pageSize, maxOpenPages, CacheCapacityPolicy.CapacityIsMaxOpenPages) {
		    _sizer = sizer;
	    }

	    protected override IPage<TItem> NewPageInstance(int pageNumber) {
		    return new BinaryFormattedPage<TItem>(this.PageSize, _sizer);
	    }

		protected override IPage<TItem>[] LoadPages() {
			throw new NotSupportedException("Pages are not loadable across runtime sessions in this implementation. See FileMappedList class."); 
		}
	}

}