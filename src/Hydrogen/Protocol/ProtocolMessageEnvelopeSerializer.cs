﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;

namespace Hydrogen.Communications;

public class ProtocolMessageEnvelopeSerializer : ItemSerializer<ProtocolMessageEnvelope> {
	private readonly IItemSerializer<object> _payloadSerializer;
	private static readonly byte[] MessageEnvelopeMarker = { 0, 1, 2, 3 };
	private static readonly int MessageEnvelopeLength = MessageEnvelopeMarker.Length + sizeof(byte) + sizeof(int) + sizeof(int); // magicID + dispatchType + requestID + messageLength + payload 

	public ProtocolMessageEnvelopeSerializer(IItemSerializer<object> payloadSerializer) {
		_payloadSerializer = payloadSerializer;
	}

	public override long CalculateSize(ProtocolMessageEnvelope item)
		=> MessageEnvelopeMarker.Length + _payloadSerializer.CalculateSize(item.Message);

	public override bool TrySerialize(ProtocolMessageEnvelope item, EndianBinaryWriter writer, out long bytesWritten) {
		writer.Write(MessageEnvelopeMarker);
		writer.Write((byte)item.DispatchType);
		writer.Write(item.RequestID);
		writer.Write(_payloadSerializer.CalculateSize(item.Message));
		bytesWritten = MessageEnvelopeLength;
		if (!_payloadSerializer.TrySerialize(item.Message, writer, out var itemBytesWritten))
			return false;
		bytesWritten += itemBytesWritten;
		return true;
	}

	public override bool TryDeserialize(long byteSize, EndianBinaryReader reader, out ProtocolMessageEnvelope envelope) {
		//using var readStream = new MemoryStream(bytes.ToArray()); // TODO: uses slow ToArray
		envelope = null;

		if (reader.BaseStream.Length < MessageEnvelopeLength)
			return false; // Not a message envelope

		// Read magic header for message object 
		var magicID = reader.ReadBytes(MessageEnvelopeMarker.Length);
		if (!magicID.AsSpan().SequenceEqual(MessageEnvelopeMarker))
			return false; // Message Magic ID not found, so not a message

		// Read envelope
		var dispatchType = (ProtocolDispatchType)reader.ReadByte();
		var requestID = reader.ReadInt32();
		var messageLength = reader.ReadInt32();

		if (reader.BaseStream.Length < MessageEnvelopeLength + messageLength)
			return false; // message not present

		// Deserialize message
		if (!_payloadSerializer.TryDeserialize((int)messageLength, reader, out var message))
			return false; //  Malformed message payload(unable to deserialize message

		envelope = new ProtocolMessageEnvelope {
			DispatchType = dispatchType,
			RequestID = requestID,
			Message = message
		};

		return true;
	}

}
