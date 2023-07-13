// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System.Linq;
using System.Diagnostics;

namespace Hydrogen.Application;

public class StandardProductInstancesCounter : IProductInstancesCounter {

	public int CountNumberOfRunningInstances() {
		return Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count();
	}

}
