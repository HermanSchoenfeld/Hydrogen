﻿using System;

namespace Sphere10.Helium.Router {
	public record MessageProcessed {

		public MessageProcessed(Guid messageId, ulong sequentialMessageSentNr, string messageBodyHash, DateTime messageCreateDateTime, TimeSpan messageTimeToLive) {
			MessageId = messageId;
			SequentialMessageSentNr = sequentialMessageSentNr;
			MessageBodyHash = messageBodyHash;
			MessageCreateDateTime = messageCreateDateTime;
			MessageTimeToLive = messageTimeToLive;
		}

		public Guid MessageId { get; init; }
		public ulong SequentialMessageSentNr { get; init; }
		public string MessageBodyHash { get; init; }
		public DateTime MessageCreateDateTime { get; init; }
		public TimeSpan MessageTimeToLive { get; init; }
	}
}