﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using NUnit.Framework;

namespace Hydrogen.Tests;

internal class ClusteredStoragePolicyTestValuesAttribute : ValuesAttribute {
	public ClusteredStoragePolicyTestValuesAttribute()
		: base(ClusteredStoragePolicy.Default, ClusteredStoragePolicy.BlobOptimized, ClusteredStoragePolicy.Debug, ClusteredStoragePolicy.Default | ClusteredStoragePolicy.CacheRecords) {
	}
}
