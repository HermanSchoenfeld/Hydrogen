// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrogen;

/// <summary>
/// A dictionary whose items are stored on a file and which has ACID transactional commit/rollback capability as well as built-in memory caching for efficiency. There are
/// no restrictions on this dictionary, items may be added, mutated and removed arbitrarily. This class is essentially a light-weight database.
/// </summary>
public abstract class TransactionalDictionaryBase<TKey, TValue> : ObservableDictionary<TKey, TValue>, ITransactionalDictionary<TKey, TValue> {

	public const int DefaultTransactionalPageSize = 1 << 18; // 256kb
	public const int DefaultClusterSize = 256; // 256b
	public const int DefaultMaxMemory = int.MaxValue; //  10 * (1 << 20);// 10mb

	public event EventHandlerEx<object> Loading {
		add => StreamMappedDictionary.Loading += value;
		remove => StreamMappedDictionary.Loading -= value;
	}

	public event EventHandlerEx<object> Loaded {
		add => StreamMappedDictionary.Loaded += value;
		remove => StreamMappedDictionary.Loaded -= value;
	}

	public event EventHandlerEx<object> Committing {
		add => _transactionalBuffer.Committing += value;
		remove => _transactionalBuffer.Committing -= value;
	}

	public event EventHandlerEx<object> Committed {
		add => _transactionalBuffer.Committed += value;
		remove => _transactionalBuffer.Committed -= value;
	}

	public event EventHandlerEx<object> RollingBack {
		add => _transactionalBuffer.RollingBack += value;
		remove => _transactionalBuffer.RollingBack -= value;
	}

	public event EventHandlerEx<object> RolledBack {
		add => _transactionalBuffer.RolledBack += value;
		remove => _transactionalBuffer.RolledBack -= value;
	}

	private readonly TransactionalFileMappedBuffer _transactionalBuffer;
	private readonly SynchronizedDictionary<TKey, TValue> _dictionary;
	private bool _disposed;


	/// <summary>
	/// Creates a <see cref="TransactionalList{T}" />.
	/// </summary>
	/// <param name="filename">File which will contain the serialized objects.</param>
	/// <param name="uncommittedPageFileDir">A working directory which stores uncommitted transactional pages. Must be the same across system restarts for transaction resumption.</param>
	/// <param name="serializer">Serializer for the objects</param>
	/// <param name="comparer"></param>
	/// <param name="transactionalPageSizeBytes">Size of transactional page</param>
	/// <param name="maxMemory">How much of the list can be kept in memory at any time</param>
	/// <param name="clusterSize">To support random access reads/writes the file is broken into discontinuous clusters of this size (similar to how disk storage) works. <remarks>Try to fit your average object in 1 cluster for performance. However, spare space in a cluster cannot be used.</remarks> </param>
	/// <param name="readOnly">Whether or not file is opened in readonly mode.</param>
	protected TransactionalDictionaryBase(string filename, string uncommittedPageFileDir, Func<IBuffer, IStreamMappedDictionary<TKey, TValue>> streamMappedDictionaryActivator, int transactionalPageSizeBytes = DefaultTransactionalPageSize,
	                                      long maxMemory = DefaultMaxMemory, bool readOnly = false) {
		Guard.ArgumentNotNull(filename, nameof(filename));
		Guard.ArgumentNotNull(uncommittedPageFileDir, nameof(uncommittedPageFileDir));

		_disposed = false;

		_transactionalBuffer = new TransactionalFileMappedBuffer(filename, uncommittedPageFileDir, transactionalPageSizeBytes, maxMemory, readOnly);
		_transactionalBuffer.Committing += _ => OnCommitting();
		_transactionalBuffer.Committed += _ => OnCommitted();
		_transactionalBuffer.RollingBack += _ => OnRollingBack();
		_transactionalBuffer.RolledBack += _ => OnRolledBack();

		// NOTE: needs removal
		//if (_transactionalBuffer.RequiresLoad)
		//	_transactionalBuffer.Load();

		StreamMappedDictionary = streamMappedDictionaryActivator(_transactionalBuffer);

		_dictionary = new SynchronizedDictionary<TKey, TValue>(StreamMappedDictionary);
		InternalCollection = _dictionary;
	}

	protected IStreamMappedDictionary<TKey, TValue> StreamMappedDictionary { get; }

	public bool RequiresLoad => _transactionalBuffer.RequiresLoad || StreamMappedDictionary.RequiresLoad;

	public ISynchronizedObject ParentSyncObject {
		get => _dictionary.ParentSyncObject;
		set => _dictionary.ParentSyncObject = value;
	}

	public ReaderWriterLockSlim ThreadLock => _dictionary.ThreadLock;

	public string Path => _transactionalBuffer.Path;

	public TransactionalFileMappedBuffer AsBuffer => _transactionalBuffer;

	public IClusteredStorage Storage => StreamMappedDictionary.Storage;

	public void Load() {
		if (_transactionalBuffer.RequiresLoad)
			_transactionalBuffer.Load();

		if (StreamMappedDictionary.RequiresLoad)
			StreamMappedDictionary.Load();
	}

	public Task LoadAsync() => Task.Run(Load);

	public IDisposable EnterReadScope() => _dictionary.EnterReadScope();

	public IDisposable EnterWriteScope() => _dictionary.EnterWriteScope();

	public void Commit() => _transactionalBuffer.Commit();

	public Task CommitAsync() => Task.Run(Commit);

	public void Rollback() => _transactionalBuffer.Rollback();

	public Task RollbackAsync() => Task.Run(Rollback);

	public void Dispose() {
		_transactionalBuffer?.Dispose();
		_disposed = true;
	}


	protected override void OnAccessing(EventTraits eventType) {
		if (_disposed)
			throw new InvalidOperationException($"{GetType().Name} has been disposed");
	}

	protected virtual void OnCommitting() {
	}

	protected virtual void OnCommitted() {
	}

	protected virtual void OnRollingBack() {
	}

	protected virtual void OnRolledBack() {
	}
}