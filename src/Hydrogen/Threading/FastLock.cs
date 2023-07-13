// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Threading;

namespace Hydrogen;

public sealed class FastLock {
	private readonly ReaderWriterLockSlim _lock;

	public FastLock() {
		_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
	}

	public IDisposable EnterLockScope() {
		_lock.EnterWriteLock();
		return new ActionScope(() => _lock.ExitWriteLock());
	}

}
