﻿using System.IO;

namespace Sphere10.Framework.Collections.StreamMapped
{

    public abstract class StreamPageBase<TItem> : PageBase<TItem>
    {
        protected StreamPageBase(StreamMappedList<TItem> parent)
        {
            Parent = parent;
        }
        
        public long StartPosition { get; protected set; }

        protected StreamMappedList<TItem> Parent { get; }
        
        protected Stream Stream => Parent.Stream;

        protected EndianBinaryReader Reader => Parent.Reader;

        protected int ItemSize => Parent.Serializer.FixedSize;

        protected IObjectSerializer<TItem> Serializer => Parent.Serializer;

        protected EndianBinaryWriter Writer => Parent.Writer;
    }

}