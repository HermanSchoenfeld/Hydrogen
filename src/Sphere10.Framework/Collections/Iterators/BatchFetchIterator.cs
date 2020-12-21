using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sphere10.Framework {

	public class BatchFetchIterator<T> : IEnumerable<T> {
		private readonly IEnumerable<Func<IEnumerable<T>>> _batches;

		public BatchFetchIterator(int batchSize, int totalItems, Func<int, int, IEnumerable<T>> batchFetcher) {
			var numBatches = (int)Math.Ceiling((totalItems / (float)batchSize));
			_batches =
				Enumerable
					.Range(0, numBatches)
					.Select(batch => new Func<IEnumerable<T>>(
						() => {
							var startIndex = batch * batchSize;
							var readCount = (totalItems - startIndex).ClipTo(0, batchSize);
							return batchFetcher(startIndex, readCount);
						}
					));
		}

		public BatchFetchIterator(IEnumerable<Func<IEnumerable<T>>> batches) {
			_batches = batches;
		}


		public IEnumerator<T> GetEnumerator() {
			foreach (var batchFetcher in _batches) {
				var batchItems = batchFetcher();
				if (!batchItems.Any())
					yield break;
				foreach (var item in batchItems)
					yield return item;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

}