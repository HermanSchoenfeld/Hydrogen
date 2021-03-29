﻿using System;
using System.IO;

namespace Sphere10.Framework {

	internal abstract class StreamPageBase<TItem> : PageBase<TItem> {
		protected StreamPageBase(StreamMappedPagedList<TItem> parent) {
			Parent = parent;
		}

		public long StartPosition { get; protected set; }
		
		public abstract int ReadItemRaw(int itemIndex, int byteOffset, int byteLength, out Span<byte> result);

		protected StreamMappedPagedList<TItem> Parent { get; }

		protected Stream Stream => Parent.Stream;

		protected EndianBinaryReader Reader => Parent.Reader;

		protected int ItemSize => Parent.Serializer.FixedSize;

		protected IObjectSerializer<TItem> Serializer => Parent.Serializer;

		protected EndianBinaryWriter Writer => Parent.Writer;
	}

}
