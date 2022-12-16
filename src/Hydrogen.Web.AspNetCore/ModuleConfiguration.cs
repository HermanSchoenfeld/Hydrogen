//-----------------------------------------------------------------------
// <copyright file="ModuleConfiguration.cs" company="Sphere 10 Software">
//
// Copyright (c) Sphere 10 Software. All rights reserved. (http://www.sphere10.com)
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// <author>Herman Schoenfeld</author>
// <date>2018</date>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using Hydrogen.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Hydrogen.Web.AspNetCore {
	public class ModuleConfiguration : ModuleConfigurationBase {

		public override int Priority => int.MinValue; // last to execute

		public override void RegisterComponents(IServiceCollection serviceCollection) {

			// register initializers
			serviceCollection.AddInitializer<AspNetCoreLifecycleMonitorInitializer>();
		}

		public override void OnInitialize(IServiceProvider serviceProvider) {
			base.OnInitialize(serviceProvider);
		}
	}
}
