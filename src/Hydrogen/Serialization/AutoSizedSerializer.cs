﻿namespace Hydrogen;

public class AutoSizedSerializer<TItem> : ItemSerializerDecorator<TItem>  {
		
	public AutoSizedSerializer(IItemSerializer<TItem> internalSerializer) 
		: base(internalSerializer){
	}

	public override int CalculateSize(TItem item) => sizeof(int) + base.CalculateSize(item);

	public bool TrySerialize(TItem item, EndianBinaryWriter writer) 
		=> TrySerialize(item, writer, out _);

	public override bool TrySerialize(TItem item, EndianBinaryWriter writer, out int bytesWritten) {
		var len = item != null ? base.CalculateSize(item) : 0;
		writer.Write(len);
		var res = base.TrySerialize(item, writer, out bytesWritten);
		bytesWritten += sizeof(int);
		return res;
	}

	public override bool TryDeserialize(int byteSize, EndianBinaryReader reader, out TItem item) {
		// Caller trying to read item passing explicit size, so check length matches
		var len = reader.Read();
		Guard.ArgumentEquals(byteSize, sizeof(int) + len, nameof(byteSize), "Read overflow"); 
		return base.TryDeserialize(byteSize - sizeof(uint), reader, out item);
	}

	public bool TryDeserialize(EndianBinaryReader reader, out TItem item) {
		var len = reader.Read();
		return base.TryDeserialize(len, reader, out item);
	}
}
