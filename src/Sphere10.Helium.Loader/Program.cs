﻿using Sphere10.Helium.PluginFramework;
using EnumModeOfOperationType = Sphere10.Helium.Framework.EnumModeOfOperationType;

namespace Sphere10.Helium.Loader {

	/// <summary>
	/// IMPORTANT: This simulates the Sphere10 node.
	/// This Project will be deleted once Helium is integrated into Hydrogen.
	/// </summary>
	public class Program {
		private static Router.IRouter _router;

		public static void Main(string[] args) {

			IHeliumPluginLoader heliumPluginLoader = new HeliumPluginLoader();

			var heliumFramework = heliumPluginLoader.GetHeliumFramework();
			heliumFramework.ModeOfOperation = EnumModeOfOperationType.HydrogenMode;
			heliumFramework.StartHeliumFramework();
			_router = heliumFramework.Router;

			heliumPluginLoader.LoadPlugins(GetPluginRelativePathNameList());

			SimulateMessagesBeingSentToThisNode();
		}

		private static string[] GetPluginRelativePathNameList() {
			return new[] {
					@"Sphere10.Helium.BlueService\bin\Debug\net5.0\Sphere10.Helium.BlueService.dll",
					@"Sphere10.Helium.Usage\bin\Debug\net5.0\Sphere10.Helium.Usage.dll"
			};
		}

		private static void SimulateMessagesBeingSentToThisNode() {
			Test_SendTestMessage1ToRouter();
			Test_SendTestMessage2ToRouter();
			Test_SendTestMessage3ToRouter();
			Test_SendTestMessage4ToRouter();
			Test_SendTestMessage5ToRouter();
		}

		private static void Test_SendTestMessage1ToRouter() {
			//_router.InputMessage()
		}

		private static void Test_SendTestMessage2ToRouter() {
			//_router.InputMessage()
		}

		private static void Test_SendTestMessage3ToRouter() {
			//_router.InputMessage()
		}

		private static void Test_SendTestMessage4ToRouter() {
			//_router.InputMessage()
		}

		private static void Test_SendTestMessage5ToRouter() {
			//_router.InputMessage()
		}
	}
}