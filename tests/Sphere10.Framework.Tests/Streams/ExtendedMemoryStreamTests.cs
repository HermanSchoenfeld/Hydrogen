//-----------------------------------------------------------------------
// <copyright file="LargeCollectionTests.cs" company="Sphere 10 Software">
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
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Sphere10.Framework.NUnit;

namespace Sphere10.Framework.Tests {

	[TestFixture]
	[Parallelizable]
	public class ExtendedMemoryStreamTests {

		[Test]
		public void Empty([Values] InnerListType listType) {
			using (CreateTestStream(listType, 10, out var stream)) {
				var expected = new MemoryStream();
				Assert.AreEqual(expected.Position, stream.Position);
				Assert.AreEqual(expected.Length, stream.Length);
				Assert.AreEqual(expected.ToArray(), stream.ToArray());
			}
		}

		[Test]
		public void ReadAll([Values] InnerListType listType) {
			using (CreateTestStream(listType, 10, out var stream)) {
				var expected = new MemoryStream();
				var data = new Random(31337).NextBytes(100);
				stream.WriteBytes(data);
				expected.WriteBytes(data);
				Assert.AreEqual(expected.ReadBytes(100), stream.ReadBytes(100));
				Assert.AreEqual(expected.Position, stream.Position);
				Assert.AreEqual(expected.Length, stream.Length);
				Assert.AreEqual(expected.ToArray(), stream.ToArray());
			}
		}

		[Test]
		public void RemoveAll([Values] InnerListType listType) {
			using (CreateTestStream(listType, 10, out var stream)) {
				var expected = new MemoryStream();
				var data = new Random(31337).NextBytes(100);
				stream.WriteBytes(data);
				expected.WriteBytes(data);
				stream.SetLength(0);
				expected.SetLength(0);
				Assert.AreEqual(expected.Position, stream.Position);
				Assert.AreEqual(expected.Length, stream.Length);
				Assert.AreEqual(expected.ToArray(), stream.ToArray());
			}
		}

		[Test]
		public void SeekEnd_Empty([Values] InnerListType listType) {
			using (CreateTestStream(listType, 10, out var stream)) {
				var expected = new MemoryStream();
				stream.Seek(0, SeekOrigin.End);
				expected.Seek(0, SeekOrigin.End);
				Assert.AreEqual(expected.Position, stream.Position);
			}
		}

		[Test]
		public void SeekEnd_NotEmpty([Values] InnerListType listType) {
			using (CreateTestStream(listType, 10, out var stream)) {
				var expected = new MemoryStream();
				var data = new Random(31337).NextBytes(100);
				stream.WriteBytes(data);
				expected.WriteBytes(data);
				stream.Seek(0, SeekOrigin.End);
				expected.Seek(0, SeekOrigin.End);
				Assert.AreEqual(expected.Position, stream.Position);
			}
		}

		[Test]
		public void IntegrationTests([Values] InnerListType listType, [Values(0, 3, 111, 9371)] int maxSize) {
			var RNG = new Random(maxSize);
			using (CreateTestStream(listType, maxSize, out var stream)) 
				AssertEx.StreamIntegrationTests(maxSize, stream, RNG: RNG);
		}

		private IDisposable CreateTestStream(InnerListType listType, int maxSize,  out ExtendedMemoryStream stream) {
			var pageSize = Math.Max(1, maxSize / 5);
			var maxOpenPages = 2;
			var disposables = new Disposables();

			switch (listType) {
				case InnerListType.List:
					stream = new ExtendedMemoryStream(new ExtendedListAdapter<byte>(new List<byte>()));
					return Disposables.None;
				case InnerListType.ExtendedList:
					stream = new ExtendedMemoryStream(new ExtendedList<byte>());
					return Disposables.None;
				case InnerListType.MemoryBuffer:
					stream = new ExtendedMemoryStream(new MemoryBuffer());
					return Disposables.None;
				case InnerListType.MemoryPagedBuffer:
					var memPagedBuffer = new MemoryPagedBuffer(pageSize, maxOpenPages*pageSize);
					stream = new ExtendedMemoryStream(memPagedBuffer);
					return new Disposables(memPagedBuffer);
				case InnerListType.BinaryFile:
					var tmpFile = Tools.FileSystem.GetTempFileName(false);
					var binaryFile = new FileMappedBuffer(tmpFile, pageSize, maxOpenPages*pageSize);
					stream = new ExtendedMemoryStream(binaryFile);
					return new Disposables(new ActionScope(() => File.Delete(tmpFile)));
				
				case InnerListType.TransactionalBinaryFile:
					var baseDir = Tools.FileSystem.GetTempEmptyDirectory(true);
					var fileName = Path.Combine(baseDir, "File.dat");
					var transactionalBinaryFile = new TransactionalFileMappedBuffer(fileName, baseDir, Guid.NewGuid(), pageSize, maxOpenPages*pageSize);
					stream = new ExtendedMemoryStream(transactionalBinaryFile);
					return new Disposables(new ActionScope(() => Tools.FileSystem.DeleteDirectory(baseDir)));
				default:
					throw new ArgumentOutOfRangeException(nameof(listType), listType, null);
			}
		}

		public enum InnerListType {
			List,
			ExtendedList,
			MemoryBuffer,
			MemoryPagedBuffer,
			BinaryFile,
			TransactionalBinaryFile
		}
	}
}
