﻿using System;
using Sphere10.Helium.Bus;
using Sphere10.Helium.Endpoint;
using Sphere10.Helium.Queue;

namespace Sphere10.Helium.BlueService.Config {
	public class EndpointConfiguration : IConfigureThisEndpoint {
		public void SetupEndpoint(BusConfiguration busConfiguration) {
			busConfiguration.IsPersisted = false;
			busConfiguration.EndpointType = EnumEndpointType.SendAndForget;
			busConfiguration.FileName = "BlueQueue1";
			busConfiguration.FilePathForLocalQueuePersistence = "C:\\Users\\jake\\Desktop\\temp";

			busConfiguration.QueueConfigDto = new QueueConfigDto {
				ClusterSize = 12,
				ListingClusterCount = 12,
				StorageClusterCount = 12,
				AllocatedMemory = 12,
				AuditLogQueueName = "AuditLogQueue",
				ErrorQueueName = "ErrorQueue",
				FilePath = "DonNotKnowYet",
				InputQueueReadRatePerMinute = 12,
				MaxItems = 12,
				MaxSizeBytes = 12,
				TempDirectoryPath = "C:\\Users\\jake\\Desktop\\temp\\temp",
				TransactionalPageSize = 12,
				ID = Guid.NewGuid()
			};

			busConfiguration.RouteQueueReadRatePerMinute = 12;
			busConfiguration.SourceEndpointName = "DoNotKnowYet";
		}
	}
}
