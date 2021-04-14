﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sphere10.Framework {

	public class StreamMappedItemAccessedArgs<TItem, TListing> : EventArgs where TListing : IItemListing {
		public ListOperationType OperationType { get; init; }
		public int ListingIndex { get; init; }
		public TListing Listing { get; init; }
		public TItem Item { get; init; }
		public byte[] SerializedItem { get; init; }
	}

	public abstract class StreamMappedClusteredListBase<TItem, TListing> : RangedListBase<TItem> where TListing : IItemListing {

		protected readonly IObjectSerializer<TListing> ListingSerializer;
		protected readonly IObjectSerializer<TItem> ItemSerializer;
		protected readonly IEqualityComparer<TItem> ItemComparer;
		protected readonly Stream InnerStream;
		protected readonly int ClusterDataSize;
		
		public event EventHandlerEx<object, StreamMappedItemAccessedArgs<TItem, TListing>> ItemAccess;
	

		public StreamMappedClusteredListBase(int clusterDataSize, Stream stream, IObjectSerializer<TItem> itemSerializer, IObjectSerializer<TListing> listingSerializer,  IEqualityComparer<TItem> itemComparer = null) {
			Guard.ArgumentNotNull(listingSerializer, nameof(listingSerializer));
			Guard.Argument(listingSerializer.IsFixedSize, nameof(listingSerializer), "Listing objects must be fixed size");
			ItemSerializer = itemSerializer;
			ListingSerializer = listingSerializer;
			ItemComparer = itemComparer ?? EqualityComparer<TItem>.Default;
			InnerStream = stream;
			ClusterDataSize = clusterDataSize;
		}

		public virtual bool RequiresLoad => InnerStream.Length > 0 && !Loaded;

		public bool Loaded { get; protected set; }

		protected abstract IEnumerable<int> GetFreeClusterNumbers(int numberRequired);

		protected bool SuppressNotifications;

		internal abstract StreamMappedPagedList<Cluster> Clusters { get; set; }

		public abstract void Load();

		protected abstract TListing NewListingInstance(int itemSizeBytes, int clusterStartIndex);

		protected abstract void MarkClusterFree(int clusterNumber);

		protected TListing AddItemToClusters(int listingIndex, TItem item) {
			if (item is null) {
				return NewListingInstance(-1, -1);
			}
			
			byte[] data = SerializeItem(item);
			var listing = AddItemInternal(data);
			NotifyItemAccess(ListOperationType.Add, listingIndex, listing, item, data);
			return listing;
		}

		protected TListing UpdateItemInClusters(int listingIndex, TListing itemListing, TItem update) {
			RemoveItemFromClusters(listingIndex, itemListing);
			//NotifyItemAccess(ListOperationType.Remove, itemListing, default, default);   // an update does not mean old item was removed, it means overwritten
			byte[] updatedData = SerializeItem(update);
			var listing = AddItemInternal(updatedData);
			NotifyItemAccess(ListOperationType.Update, listingIndex, listing, update, updatedData);
			return listing;
		}

		protected TItem ReadItemFromClusters(int listingIndex, TListing listing) {
			var size = listing.Size;
			var startCluster = listing.ClusterStartIndex;
			if (size == -1 && startCluster == -1)
				return default;

			int? next = startCluster;
			var remaining = size;

			var builder = new ByteArrayBuilder();

			while (next != -1) {
				var cluster = Clusters[next.Value];
				next = cluster.Next;

				if (cluster.Next < 0) {
					builder.Append(cluster.Data.Take(remaining).ToArray());
				} else {
					builder.Append(cluster.Data);
					remaining -= cluster.Data.Length;
				}
			}

			var data = builder.ToArray();

			var item = ItemSerializer.Deserialize(size,
				new EndianBinaryReader(EndianBitConverter.Little, new MemoryStream(data)));

			NotifyItemAccess(ListOperationType.Read, listingIndex, listing, item, data);

			return item;
		}

		protected void RemoveItemFromClusters(int listingIndex, TListing listing) {
			var startClusterNumber = listing.ClusterStartIndex;
			var size = listing.Size;
			var next = startClusterNumber;


			while (next != -1) {
				var cluster = Clusters[next];
				// Removed since old data is unnecessary burden (for now)
				//var data = cluster.Data.Take(Math.Min(size, cluster.Data.Length)).ToArray();
				//builder.Append(data);
				//size -= data.Length;

				MarkClusterFree(cluster.Number);
				next = cluster.Next;
			}

			NotifyItemAccess(ListOperationType.Remove, listingIndex, listing, default, default);
		}

		private TListing AddItemInternal(byte[] data) {
			var clusters = new List<Cluster>();
			var segments = data.Partition(ClusterDataSize)
				.ToList();

			var numbers = GetFreeClusterNumbers(segments.Count).ToArray();

			for (var i = 0; i < segments.Count; i++) {
				var segment = segments[i].ToArray();
				var clusterData = new byte[ClusterDataSize];
				segment.CopyTo(clusterData, 0);

				clusters.Add(new Cluster {
					Number = numbers[i],
					Data = clusterData,
					Next = segments.Count - 1 == i ? -1 : numbers[i + 1]
				});
			}

			foreach (var cluster in clusters)
				if (!Clusters.Any())
					Clusters.Add(cluster);
				else if (cluster.Number >= Clusters.Count)
					Clusters.Add(cluster);
				else
					Clusters[cluster.Number] = cluster;

			return NewListingInstance(data.Length, clusters.FirstOrDefault()?.Number ?? -1);
		}

		private byte[] SerializeItem(TItem item) {
			using var stream = new MemoryStream();
			ItemSerializer.Serialize(item, new EndianBinaryWriter(EndianBitConverter.Little, stream));
			return stream.ToArray();
		}

		protected void CheckRange(int index, int count) {
			Guard.Argument(count >= 0, nameof(index), "Must be greater than or equal to 0");
			if (index == Count && count == 0) return; // special case: at index of "next item" with no count, this is valid
			Guard.ArgumentInRange(index, 0, Count - 1, nameof(index));
			if (count > 0)
				Guard.ArgumentInRange(index + count - 1, 0, Count - 1, nameof(count));
		}

		protected void CheckLoaded() {
			if (RequiresLoad) {
				throw new InvalidOperationException("List requires loading as stream contains existing data.");
			}
		}

		protected virtual void OnItemAccess(ListOperationType operationType, int listingIndex, TListing listing, TItem item, byte[] serializedItem) {
		}

		protected void NotifyItemAccess(ListOperationType operationType, int listingIndex, TListing listing, TItem item, byte[] serializedItem) {
			if (SuppressNotifications)
				return;



			OnItemAccess(operationType, listingIndex, listing, item, serializedItem);
			var args = new StreamMappedItemAccessedArgs<TItem, TListing>() { ListingIndex = listingIndex, Listing = listing, Item = item, SerializedItem = serializedItem };
			ItemAccess?.Invoke(this, args);
		}

	}
}