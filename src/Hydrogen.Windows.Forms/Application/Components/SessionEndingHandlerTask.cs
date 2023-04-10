//-----------------------------------------------------------------------
// <copyright file="SessionEndingHandlerTask.cs" company="Sphere 10 Software">
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using Hydrogen.Application;
using Microsoft.Win32;

namespace Hydrogen.Windows.Forms;


/// <summary>
/// Sets a handler to catch the system shutdown event, so the application can close properly.
///
/// 
/// C:\Program Files (x86)\Microsoft Corporation\Logo Testing Tools for Windows\Restart Manager\x86>rmtool.exe -p PIDHERE -S
/// </summary>
public class SessionEndingHandlerInitializer : ApplicationInitializerBase {

	public SessionEndingHandlerInitializer(IServiceProvider serviceProvider) {
		ServiceProvider = serviceProvider;
	}

	protected IServiceProvider ServiceProvider { get; }

	public override void Initialize() {
		SystemEvents.SessionEnding += SystemEventsOnSessionEnding;
		SystemEvents.SessionEnded += SystemEventsOnSessionEnded;
	}

	protected virtual void SystemEventsOnSessionEnded(object sender, SessionEndedEventArgs sessionEndedEventArgs) {
		// Note: have to resolve here since if passed in as constructor then has issues with
		// WinForms applications that register the IUserInterfaceServices when main form is created (and after framework
		// initialization which this component is created in). Must be resolved here as a result.
		if (ServiceProvider.TryGetService<IUserInterfaceServices>(out var userInterfaceServices)) {
			userInterfaceServices.Exit(true);
		} else {
			HydrogenFramework.Instance.EndFramework();
			Environment.Exit(-1);
		}
	}

	protected virtual void SystemEventsOnSessionEnding(object sender, SessionEndingEventArgs sessionEndingEventArgs) {
	}

}
