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
using Microsoft.Extensions.DependencyInjection;

namespace Hydrogen.Application;

public class ProductUsageInformationTokenResolver : ITokenResolver {

	public ProductUsageInformationTokenResolver() {
		// Since implementations of IProductUsageServices depend on token resolvers, we resort
		// to service locator pattern here. Exceptional case.
		ProductUsageServices = 
			Tools.Values.Future.LazyLoad( 
				() => HydrogenFramework.Instance.ServiceProvider.GetService<IProductUsageServices>()
			);
	}

	protected IFuture<IProductUsageServices> ProductUsageServices { get; }

	public bool TryResolve(string token, out object value) {
		var info = ProductUsageServices.Value.ProductUsageInformation;
		value = token.ToUpperInvariant() switch {
			"FIRSTUSEDDATEBYSYSTEMUTC" => string.Format("{0:yyyy-MM-dd}", info.FirstUsedDateBySystemUTC),
			"DAYSUSEDBYSYSTEM" => info.DaysUsedBySystem.ToString(),
			"NUMBEROFUSESBYSYSTEM" => info.NumberOfUsesBySystem.ToString(),
			"FIRSTUSEDDATEBYUSERUTC" => string.Format("{0:yyyy-MM-dd}", info.FirstUsedDateByUserUTC),
			"DAYSUSEDBYUSER" => info.DaysUsedByUser.ToString(),
			"NUMBEROFUSESBYUSER" => info.NumberOfUsesByUser.ToString(),

			// System specific stuff

			_ => null
		};
		return value != null;
	}
}
