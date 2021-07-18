﻿using System;
using System.IO;

namespace Sphere10.Framework.Values {
	
	//Based on CompactVarInt LEB128 compression from NBitcoin.
	public class CVarInt {
		private readonly ulong _value;
		private readonly int _size;
		
		public CVarInt(ulong value, int size) {
			_value = value;
			_size = size;
		}

		public static CVarInt FromStream(int size, Stream stream) {
			ulong n = 0;
			while (true)
			{
				var chData = (byte)stream.ReadByte();
				var a = n << 7;
				var b = (byte)(chData & 0x7F);
				n = (a | b);
				if ((chData & 0x80) != 0)
					n++;
				else
					break;
			}
			return new CVarInt(n, size);
		}

		public byte[] ToBytes() {
			var n = _value;
			Span<byte> tmp = stackalloc byte[(_size * 8 + 6) / 7];
			var builder = new ByteArrayBuilder();

			var len = 0;
			while (true)
			{
				var a = (byte)(n & 0x7F);
				var b = (byte)(len != 0 ? 0x80 : 0x00);
				tmp[len] = (byte)(a | b);
				if (n <= 0x7F)
					break;
				n = (n >> 7) - 1;
				len++;
			}
			do
			{
				builder.Append(tmp[len]);
			} while (len-- != 0);

			return builder.ToArray();
		}
		
		public static implicit operator ulong(CVarInt v) => v._value;

		public ulong ToLong() => _value;
	}
}
