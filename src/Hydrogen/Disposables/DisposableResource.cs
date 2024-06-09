﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT NON-AI software license, see the accompanying file
// LICENSE or visit https://sphere10.com/legal/NON-AI-MIT.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System.Threading.Tasks;

namespace Hydrogen;

public class DisposableResource : Disposable {

	protected Disposables Disposables = new Disposables();

	protected override async ValueTask FreeManagedResourcesAsync() {
		Disposables.Dispose();
	}

	protected override void FreeManagedResources() {
		Disposables.Dispose();
	}
}
