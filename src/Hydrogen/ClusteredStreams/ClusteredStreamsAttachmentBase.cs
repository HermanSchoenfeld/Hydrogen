﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Hydrogen.ObjectSpaces;

namespace Hydrogen;

/// <summary>
/// Base implementation for an <see cref="IClusteredStreamsAttachment"/>
/// </summary>
public abstract class ClusteredStreamsAttachmentBase : IDisposable, IClusteredStreamsAttachment {
	private Stream _stream;
	private readonly long _streamOffset;
	private bool _attached;

	protected ClusteredStreamsAttachmentBase(ClusteredStreams streams, long reservedStreamIndex) {
		Streams = streams;
		ReservedStreamIndex = reservedStreamIndex;
		_attached = false;
		_streamOffset = 0;
	}

	public ClusteredStreams Streams { get; }

	public long ReservedStreamIndex { get; }

	protected Stream AttachmentStream {
		get {
			CheckAttached();
			return _stream;
		}
		private set => _stream = value;
	}

	public bool IsAttached => _attached;

	public void Attach() {
		CheckNotAttached();
		Guard.Ensure(!Streams.RequiresLoad, "Unable to attach to an unloaded Object Container");
		Guard.Ensure(Streams.Header.ReservedStreams > 0, "Stream Container has no reserved streams available");
		Guard.Ensure(ReservedStreamIndex < Streams.Header.ReservedStreams, $"Stream at index {ReservedStreamIndex} is not reserved");
		using (Streams.EnterAccessScope()) {
			_attached = true;

			// Open the stream used by the index. No access scope is acquired for the stream
			// and thus all use of the index must take place within an explicit access scope.
			AttachmentStream =
				Streams
					.Open(ReservedStreamIndex, false, false)
					.AsBounded(_streamOffset, long.MaxValue, allowInnerResize: true);

			// Ensures the stream is at least as long as the offset (the space prior to offset can
			// be used to store header information (i.e. factory info to decide what type of index to load)
			if (AttachmentStream.Position < 0)
				AttachmentStream.SetLength(0);
			
			// User-defined attachment done now
			AttachInternal();

			// Integrity check
			VerifyIntegrity();
		}
	}

	protected abstract void AttachInternal();

	protected abstract void VerifyIntegrity();

	public void Detach() {
		CheckAttached();
		Flush();
		DetachInternal();
		AttachmentStream.Dispose();
		AttachmentStream = null;
		_attached = false;
	}

	public virtual void Flush() {
	}

	protected abstract void DetachInternal();

	public virtual void Dispose() {
		if (IsAttached)
			Detach();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void CheckAttached() {
		if (!_attached)
			throw new InvalidOperationException("Index is not attached");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void CheckNotAttached() {
		if (_attached)
			throw new InvalidOperationException("Index is already attached");
	}


}
