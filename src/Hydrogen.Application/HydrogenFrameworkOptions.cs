// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;

namespace Hydrogen.Application;

[Flags]
public enum HydrogenFrameworkOptions {
	EnableDrm = 1 << 0,
	BackgroundLicenseVerify = 1 << 1,
	EnsureSystemDataDirGloballyAccessible = 1 << 2,
	Default = 0,
}
