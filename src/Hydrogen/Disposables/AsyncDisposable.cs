// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Threading.Tasks;

namespace Hydrogen;

public abstract class AsyncDisposable : Disposable {

	protected sealed override void FreeManagedResources() => FreeManagedResourcesAsync().AsTask().WaitSafe();
}

