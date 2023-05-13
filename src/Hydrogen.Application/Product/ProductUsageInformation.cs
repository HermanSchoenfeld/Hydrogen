// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.IO;
using System.Reflection;

namespace Hydrogen.Application {

	public class ProductUsageInformation {

		public DateTime FirstUsedDateBySystemUTC { get; set; }
		public int DaysUsedBySystem { get; set; }
		public int NumberOfUsesBySystem { get; set; }
		public DateTime FirstUsedDateByUserUTC { get; set; }
		public int DaysUsedByUser { get; set; }
		public int NumberOfUsesByUser { get; set; }


	}
}
