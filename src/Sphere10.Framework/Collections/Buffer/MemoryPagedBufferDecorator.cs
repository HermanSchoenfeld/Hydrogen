﻿using System;
using System.Collections.Generic;

namespace Sphere10.Framework {
	public abstract class MemoryPagedBufferDecorator<TMemoryPagedBuffer> : MemoryPagedListDecorator<byte, TMemoryPagedBuffer>, IMemoryPagedBuffer where TMemoryPagedBuffer : IMemoryPagedBuffer {

        public MemoryPagedBufferDecorator(TMemoryPagedBuffer internalBuffer)
            : base(internalBuffer) {
        }

        IReadOnlyList<IBufferPage> IMemoryPagedBuffer.Pages => InternalExtendedList.Pages;

        public virtual void AddRange(ReadOnlySpan<byte> span) => InternalExtendedList.AddRange(span);

        public virtual Span<byte> AsSpan(int index, int count) => InternalExtendedList.AsSpan(index, count);

        public virtual void InsertRange(int index, ReadOnlySpan<byte> items) => InternalExtendedList.InsertRange(index, items);

        public virtual ReadOnlySpan<byte> ReadSpan(int index, int count) => InternalExtendedList.ReadSpan(index, count);

        public virtual void UpdateRange(int index, ReadOnlySpan<byte> items) => InternalExtendedList.UpdateRange(index, items);
    }

	public abstract class MemoryPagedBufferDecorator : MemoryPagedBufferDecorator<IMemoryPagedBuffer> {
		public MemoryPagedBufferDecorator(IMemoryPagedBuffer internalBuffer)
			: base(internalBuffer) {
		}
	}

}