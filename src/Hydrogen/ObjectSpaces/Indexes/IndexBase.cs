﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Runtime.CompilerServices;

namespace Hydrogen.ObjectSpaces;

/// <summary>
/// Base implementation for an index on an <see cref="ObjectContainer{TItem}"/>.
/// </summary>
/// <typeparam name="TItem">Type of item being stored in <see cref="ObjectContainer{T}"/></typeparam>
/// <typeparam name="TKey">Type of property in <see cref="TItem"/> that is the key</typeparam>
public abstract class IndexBase<TData, TStore> : ContainerObserverBase, IObjectContainerAttachment where TStore : IMetaDataStore<TData> {

	protected IndexBase(ObjectContainer container, TStore keyStore)
		: base(container) {
		Guard.ArgumentNotNull(container, nameof(container));
		KeyStore = keyStore;
	}

	public TStore KeyStore { get; }

	protected override void OnRemoved(long index) {
		CheckAttached();
		KeyStore.Remove(index);
	}

	protected override void OnReaped(long index) {
		CheckAttached();
		KeyStore.Reap(index);
	}

	protected override void OnContainerClearing() {
		// When the container about to be cleared, we detach the observer
		CheckAttached();
		KeyStore.Detach();
	}

	protected override void OnContainerCleared() {
		// After container was cleared, we reboot the index
		CheckDetached();
		KeyStore.Attach();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void CheckAttached()
		=> Guard.Ensure(KeyStore.IsAttached, "Index is not attached");

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void CheckDetached()
		=> Guard.Ensure(!KeyStore.IsAttached, "Index is attached");

	#region IObjectContainerAttachment Implementation

	// NOTE: use of backing field _keyStore to avoid attached check

	ObjectContainer IObjectContainerAttachment.Container => KeyStore.Container;

	long IObjectContainerAttachment.ReservedStreamIndex => KeyStore.ReservedStreamIndex; 

	bool IObjectContainerAttachment.IsAttached => KeyStore.IsAttached;

	void IObjectContainerAttachment.Attach() => KeyStore.Attach();

	void IObjectContainerAttachment.Detach() => KeyStore.Detach();

	#endregion
}
