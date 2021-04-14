﻿namespace Sphere10.Framework {

	public abstract class ObjectSerializerBase<TItem> : ObjectSizer<TItem>, IObjectSerializer<TItem> {
		public abstract TItem Deserialize(int size, EndianBinaryReader reader);

		public abstract int Serialize(TItem item, EndianBinaryWriter writer);

	}


}