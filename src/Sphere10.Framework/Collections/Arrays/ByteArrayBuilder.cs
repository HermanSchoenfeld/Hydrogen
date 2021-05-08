//-----------------------------------------------------------------------
// <copyright file="ByteArrayBuilder.cs" company="Sphere 10 Software">
//
// Copyright (c) Sphere 10 Software. All rights reserved. (http://www.sphere10.com)
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// <author>Herman Schoenfeld</author>
// <date>2018</date>
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Sphere10.Framework {
	public class ByteArrayBuilder {
        private const int DefaultCapacity = 4096;
        private readonly int _initialCapacity;
        private byte[] _buffer;
        public int Length { get; set; }

        public ByteArrayBuilder() : this(DefaultCapacity) {
        }

        public ByteArrayBuilder(int capacity) {
            _initialCapacity = capacity;
            _buffer = new byte[_initialCapacity];
            Clear();
        }

        public void Clear() {
            if (_buffer.Length > _initialCapacity)
                Array.Resize(ref _buffer, _initialCapacity);
            Length = 0;
		}

        public ByteArrayBuilder Append(byte b) {
            if (GetUnusedBufferLength() == 0) {
                _buffer = IncreaseCapacity(_buffer, _buffer.Length * 2);
            }
            _buffer[Length] = b;
            Length++;
            return this;
        }

        public ByteArrayBuilder Append(ReadOnlySpan<byte> buffer) {
            var unusedBufferLength = GetUnusedBufferLength();
            if (unusedBufferLength < buffer.Length) {
                _buffer = IncreaseCapacity(_buffer, _buffer.Length + buffer.Length + DefaultCapacity);
            }
            buffer.CopyTo(_buffer.AsSpan(Length));
            Length += buffer.Length;
            return this;
        }

        public byte[] ToArray() {
            var unusedBufferLength = GetUnusedBufferLength();
            if (unusedBufferLength == 0) {
                return _buffer;
            }
            var result = new byte[Length];
			_buffer.AsSpan(0, Length).CopyTo(result);
            return result;
        }

        private int GetUnusedBufferLength() {
            return _buffer.Length - Length;
        }

        private static byte[] IncreaseCapacity(byte[] buffer, int targetLength) {
            var result = new byte[targetLength];
            Array.Copy(buffer, result, buffer.Length);
            return result;
        }
    }
}
