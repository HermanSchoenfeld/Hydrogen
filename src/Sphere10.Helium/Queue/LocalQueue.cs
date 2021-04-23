﻿using Sphere10.Framework;
using Sphere10.Helium.Message;

namespace Sphere10.Helium.Queue {
	/// <summary>
	/// CRITICAL:
	/// 1) This is a FIFO queue.
	/// 2) This is a local queue for a Helium Service.
	/// 3) Every Helium Service has it's own local queue.
	/// 4) ALL input into a Helium Service goes into this queue.
	/// 5) The local queue is also used for Send-Local messages.
	/// </summary>

	public class LocalQueue : TransactionalList<IMessage>, ILocalQueue {
		
		private readonly QueueConfigDto _queueConfigDto;

		public LocalQueue(QueueConfigDto queueConfigDto)
			: base(
				new BinaryFormattedSerializer<IMessage>(),
				queueConfigDto.Path,
				queueConfigDto.TempDirPath,
				queueConfigDto.FileId,
				queueConfigDto.TransactionalPageSizeBytes,
				queueConfigDto.MaxStorageSizeBytes,
				queueConfigDto.AllocatedMemory,
				queueConfigDto.ClusterSize,
				queueConfigDto.MaxItems
			) {
			_queueConfigDto = queueConfigDto;
		}

		public void AddMessage(IMessage message) {
			Add(message);
			Commit();
		}

		public void DeleteMessage(IMessage message) {
			Remove(message);
			Commit();
		}


		public IMessage RetrieveMessage() {
			//No transaction-scope required here.
			//At any stage during catastrophic failure the message shall remain on the queue.
			//That is critical because the next time the service runs (after recovery, fixing the problem)
			//it will just process the message as per normal.
			//So the message MUST remain in the queue during a catastrophic failure.

			var message = Read(0);
			Remove(message);
			Commit();

			return message;
		}
	}
}
