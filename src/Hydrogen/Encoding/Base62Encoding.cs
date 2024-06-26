// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT NON-AI software license, see the accompanying file
// LICENSE or visit https://sphere10.com/legal/NON-AI-MIT.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System.IO;
using System.Text;

namespace Hydrogen;

public static class Base62Encoding {
	private static string Base62CodingSpace = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

	/// <summary>
	/// Convert a byte array
	/// </summary>
	/// <param name="original">Byte array</param>
	/// <returns>Base62 string</returns>
	public static string ToBase62String(byte[] original) {
		var sb = new StringBuilder();
		var stream = new BitStream(original); // Set up the BitStream
		byte[] read = new byte[1]; // Only read 6-bit at a time
		while (true) {
			read[0] = 0;
			var length = stream.Read(read, 0, 6); // Try to read 6 bits
			if (length == 6) // Not reaching the end
			{
				if ((int)(read[0] >> 3) == 0x1f) // First 5-bit is 11111
				{
					sb.Append(Base62CodingSpace[61]);
					stream.Seek(-1, SeekOrigin.Current); // Leave the 6th bit to next group
				} else if ((int)(read[0] >> 3) == 0x1e) // First 5-bit is 11110
				{
					sb.Append(Base62CodingSpace[60]);
					stream.Seek(-1, SeekOrigin.Current);
				} else // Encode 6-bit
				{
					sb.Append(Base62CodingSpace[(int)(read[0] >> 2)]);
				}
			} else if (length == 0) // Reached the end completely
			{
				break;
			} else // Reached the end with some bits left
			{
				// Padding 0s to make the last bits to 6 bit
				sb.Append(Base62CodingSpace[(int)(read[0] >> (int)(8 - length))]);
				break;
			}
		}
		return sb.ToString();
	}

	public static string ToBase62String(int number) {
		return ToBase62String(EndianBitConverter.Little.GetBytes(number));
	}

	public static string ToBase62String(uint number) {
		return ToBase62String((int)number);
	}

	/// <summary>
	/// Convert a Base62 string to byte array
	/// </summary>
	/// <param name="base62">Base62 string</param>
	/// <returns>Byte array</returns>
	public static byte[] FromBase62String(string base62) {
		// Character count
		int count = 0;

		// Set up the BitStream
		BitStream stream = new BitStream(base62.Length * 6 / 8);

		foreach (char c in base62) {
			// Look up coding table
			int index = Base62CodingSpace.IndexOf(c);

			// If end is reached
			if (count == base62.Length - 1) {
				// Check if the ending is good
				int mod = (int)(stream.Position % 8);
				if (mod == 0)
					throw new InvalidDataException("an extra character was found");

				if ((index >> (8 - mod)) > 0)
					throw new InvalidDataException("invalid ending character was found");

				stream.Write(new byte[] { (byte)(index << mod) }, 0, 8 - mod);
			} else {
				// If 60 or 61 then only write 5 bits to the stream, otherwise 6 bits.
				if (index == 60) {
					stream.Write(new byte[] { 0xf0 }, 0, 5);
				} else if (index == 61) {
					stream.Write(new byte[] { 0xf8 }, 0, 5);
				} else {
					stream.Write(new byte[] { (byte)index }, 2, 6);
				}
			}
			count++;
		}

		// Dump out the bytes
		byte[] result = new byte[stream.Position / 8];
		stream.Seek(0, SeekOrigin.Begin);
		stream.Read(result, 0, result.Length * 8);
		return result;
	}


}
