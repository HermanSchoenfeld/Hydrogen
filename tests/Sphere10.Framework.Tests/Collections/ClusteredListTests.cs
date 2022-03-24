﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sphere10.Framework.NUnit;

namespace Sphere10.Framework.Tests {

	[TestFixture]
	[Parallelizable(ParallelScope.Children)]
	public class ClusteredListTests : StreamPersistedTestsBase {

		private IDisposable CreateList(ClusteredStoragePolicy policy, out ClusteredList<TestObject> clusteredList) {
			var stream = new MemoryStream();
			clusteredList = new ClusteredList<TestObject>(stream, 32, new TestObjectSerializer(), policy: policy);
			return stream;
		}

		[Test]
		public void AddOneTest([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			var rng = new Random(31337);
			using (CreateList(policy, out var clusteredList)) {
				var obj = new TestObject(rng);
				clusteredList.Add(obj);
				Assert.That(clusteredList.Count, Is.EqualTo(1));
				Assert.That(clusteredList[0], Is.EqualTo(obj).Using(new TestObjectComparer()));
			}
		}

		[Test]
		public void AddOneRepeat([Values(100)] int iterations, [ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			var rng = new Random(31337);
			using (CreateList(policy, out var clusteredList)) {
				for (var i = 0; i < iterations; i++) {
					var obj = new TestObject(rng);
					clusteredList.Add(obj);
					Assert.That(clusteredList.Count, Is.EqualTo(i + 1));
					Assert.That(clusteredList[i], Is.EqualTo(obj).Using(new TestObjectComparer()));
				}
			}
		}
	
		[Test]
		public void ConstructorArgumentsAreGuarded([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			Assert.Throws<ArgumentNullException>(() => new ClusteredList<int>(null, 1, new PrimitiveSerializer<int>(), policy: policy));
			Assert.Throws<ArgumentNullException>(() => new ClusteredList<int>(new MemoryStream(), 1,  null, policy: policy));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ClusteredList<int>(new MemoryStream(), 0, null, policy: policy));
		}


		[Test]
		public void ReadRange([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			var random = new Random(31337);
			string[] inputs = Enumerable.Range(0, random.Next(5, 10)).Select(x => random.NextString(1, 100)).ToArray();
			using var stream = new MemoryStream();
			var list = new ClusteredList<string>(stream, 32, new StringSerializer(Encoding.UTF8), policy: policy);

			list.AddRange(inputs);

			var read = list
				.ReadRange(0, 3)
				.ToArray();

			Assert.AreEqual(inputs[0], read[0]);
			Assert.AreEqual(inputs[1], read[1]);
			Assert.AreEqual(inputs[2], read[2]);
		}

		[Test]
		public void ReadRangeInvalidArguments([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			using var stream = new MemoryStream();
			var list = new ClusteredList<int>(stream, 1,  new PrimitiveSerializer<int>(), policy: policy);
			list.AddRange(999, 1000, 1001, 1002);

			Assert.Throws<ArgumentOutOfRangeException>(() => _ = list.ReadRange(-1, 1).ToList());
			Assert.Throws<ArgumentOutOfRangeException>(() => _ = list.ReadRange(0, 5).ToList());
		}

		[Test]
		public void ReadRangeEmpty([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			using var stream = new MemoryStream();
			var list = new ClusteredList<int>(stream, 1,  new PrimitiveSerializer<int>(), policy: policy);
			list.AddRange(999, 1000, 1001, 1002);
			Assert.IsEmpty(list.ReadRange(0, 0));

			list.Clear();
			Assert.IsEmpty(list.ReadRange(0, 0));
		}

		[Test]
		public void AddRangeEmptyNullStrings([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			using var stream = new MemoryStream();
			var list = new ClusteredList<string>(stream, 32,  new StringSerializer(Encoding.UTF8), policy: policy);
			string[] input = { string.Empty, null, string.Empty, null };
			list.AddRange(input);
			Assert.AreEqual(4, list.Count);

			var read = list.ReadRange(0, 4);
			Assert.AreEqual(input, read);
		}

		[Test]
		public void AddRangeNullEmptyCollections([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			using var stream = new MemoryStream();
			var list = new ClusteredList<string>(stream, 32,  new StringSerializer(Encoding.UTF8), policy: policy);
			Assert.Throws<ArgumentNullException>(() => list.AddRange(null));
			Assert.DoesNotThrow(() => list.AddRange(new string[0]));
		}

		[Test]
		public void UpdateRange([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			var random = new Random(31337);
			string[] inputs = Enumerable.Range(0, random.Next(1, 100)).Select(x => random.NextString(1, 100)).ToArray();
			using var stream = new MemoryStream();
			var list = new ClusteredList<string>(stream, 32,  new StringSerializer(Encoding.UTF8), policy: policy);

			list.AddRange(inputs);

			string update = random.NextString(0, 100);
			list.UpdateRange(0, new[] { update });
			var value = list.Read(0);
			Assert.AreEqual(update, value);
			Assert.AreEqual(inputs.Length, list.Count);
		}

		[Test]
		public void UpdateRangeInvalidArguments([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			using var stream = new MemoryStream();
			var list = new ClusteredList<int>(stream, 32,  new PrimitiveSerializer<int>(), policy: policy);

			list.AddRange(999, 1000, 1001, 1002);
			list.UpdateRange(0, new[] { 998 });
			int read = list[0];

			Assert.AreEqual(998, read);
			Assert.AreEqual(4, list.Count);
		}

		[Test]
		public void RemoveRange([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			var random = new Random(31337);
			string[] inputs = Enumerable.Range(0, random.Next(1, 100)).Select(x => random.NextString(1, 100)).ToArray();
			using var stream = new MemoryStream();
			var list = new ClusteredList<string>(stream, 32, new StringSerializer(Encoding.UTF8), policy: policy);

			list.AddRange(inputs);
			list.RemoveRange(0, 1);

			Assert.AreEqual(inputs[1], list[0]);
			Assert.AreEqual(inputs.Length - 1, list.Count);
		}

		[Test]
		public void RemoveRangeInvalidArguments([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			using var stream = new MemoryStream();
			var list = new ClusteredList<int>(stream, 32, new PrimitiveSerializer<int>(), policy: policy);

			list.Add(999);

			Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveRange(1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveRange(-1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveRange(0, -1));

			Assert.DoesNotThrow(() => list.RemoveRange(0, 0));
		}

		[Test]
		public void IndexOf([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			var random = new Random(31337);
			string[] inputs = Enumerable.Range(1, 10).Select(x => random.NextString(1, 100)).ToArray();
			using var stream = new MemoryStream();
			var list = new ClusteredList<string>(stream, 32, new StringSerializer(Encoding.UTF8), policy: policy);

			list.AddRange(inputs);

			IEnumerable<int> indexes = list.IndexOfRange(inputs[..5]);

			Assert.AreEqual(new[] { 0, 1, 2, 3, 4 }, indexes);
		}

		[Test]
		public void IndexOfInvalidArguments([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			using var stream = new MemoryStream();
			var list = new ClusteredList<int>(stream, 32,  new PrimitiveSerializer<int>(), policy: policy);
			list.AddRange(999, 1000, 1001, 1002);

			Assert.Throws<ArgumentNullException>(() => list.IndexOfRange(null));
			Assert.DoesNotThrow(() => list.IndexOfRange(Array.Empty<int>()));
		}

		[Test]
		public void Count([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			var random = new Random(31337);
			string[] inputs = Enumerable.Range(0, random.Next(1, 100)).Select(x => random.NextString(1, 100)).ToArray();
			using var stream = new MemoryStream();
			var list = new ClusteredList<string>(stream, 32,  new StringSerializer(Encoding.UTF8), policy: policy);

			Assert.AreEqual(0, list.Count);
			list.AddRange(inputs);

			Assert.AreEqual(inputs.Length, list.Count);
		}

		[Test]
		public void InsertRange([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			var random = new Random(31337);
			string[] inputs = Enumerable.Range(1, random.Next(1, 5)).Select(x => random.NextString(1, 5)).ToArray();
			using var stream = new MemoryStream();
			var list = new ClusteredList<string>(stream, 32,  new StringSerializer(Encoding.UTF8), policy: policy);

			list.AddRange(inputs);
			list.InsertRange(0, new[] { random.NextString(1, 100) });

			Assert.AreEqual(inputs.Length + 1, list.Count);
			Assert.AreEqual(inputs[0], list[1]);
		}

		[Test]
		public void InsertRangeInvalidArguments([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			using var stream = new MemoryStream();
			var list = new ClusteredList<int>(stream, 32,  new PrimitiveSerializer<int>(), policy: policy);

			list.AddRange(999, 1000, 1001, 1002);

			Assert.Throws<ArgumentNullException>(() => list.InsertRange(0, null));
			Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(5, new int[] { 1 }));
			Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(-1, new int[] { 1 }));
			Assert.DoesNotThrow(() => list.InsertRange(0, Array.Empty<int>()));
		}

		[Test]
		public void Clear([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			var random = new Random(31337);
			string[] inputs = Enumerable.Range(0, random.Next(1, 100)).Select(x => random.NextString(1, 100)).ToArray();
			using var stream = new MemoryStream();
			var list = new ClusteredList<string>(stream, 32,  new StringSerializer(Encoding.UTF8), policy: policy);

			list.AddRange(inputs);
			list.Clear();
			Assert.AreEqual(0, list.Count);

			list.AddRange(inputs);
			Assert.AreEqual(inputs, list);
		}

		[Test]
		public void IntegrationTests_String([ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			using var stream = new MemoryStream();
			var list = new ClusteredList<string>(stream, 32, new StringSerializer(Encoding.UTF8), policy: policy);
			AssertEx.ListIntegrationTest(list,
				100,
				(rng, i) => Enumerable.Range(0, i)
					.Select(x => rng.NextString(1, 100))
					.ToArray());
		}


		[Test]
		public void LoadAndUseExistingStream([Values(1, 100)] int iterations, [ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			var random = new Random(31337 + iterations);
			var input = Enumerable.Range(0, random.Next(1, 100)).Select(x => random.NextString(0, 100)).ToArray();
			var fileName = Tools.FileSystem.GetTempFileName(true);
			using (Tools.Scope.ExecuteOnDispose(() => File.Delete(fileName))) {
				using (var fileStream = new FileStream(fileName, FileMode.Open)) {
					var list = new ClusteredList<string>(fileStream, 32, new StringSerializer(Encoding.UTF8), policy: policy);
					list.AddRange(input);
				}

				using (var fileStream = new FileStream(fileName, FileMode.Open)) {
					var list = new ClusteredList<string>(fileStream, 32, new StringSerializer(Encoding.UTF8), policy: policy);
					Assert.AreEqual(input.Length, list.Count);
					Assert.AreEqual(input, list);

					var secondInput = Enumerable.Range(0, random.Next(1, 100)).Select(x => random.NextString(1, 100)).ToArray();
					list.AddRange(secondInput);
					Assert.AreEqual(input.Concat(secondInput), list.ReadRange(0, list.Count));
					list.RemoveRange(0, list.Count);
					Assert.IsEmpty(list);
				}
			}
		}

		[Test]
		public void IntegrationTests([Values] StorageType storage, [ClusteredStoragePolicyTestValues] ClusteredStoragePolicy policy) {
			var rng = new Random(31337);
			using (CreateStream(storage, 5000, out Stream stream)) {
				var list = new ClusteredList<TestObject>(stream, 100, new TestObjectSerializer(), new TestObjectComparer(), policy: policy);
				AssertEx.ListIntegrationTest(list,
					100,
					(rng, i) => Enumerable.Range(0, i).Select(x => new TestObject(rng)).ToArray(),
					false,
					10,
					itemComparer: new TestObjectComparer()
				);
			}
		}

	}
}
