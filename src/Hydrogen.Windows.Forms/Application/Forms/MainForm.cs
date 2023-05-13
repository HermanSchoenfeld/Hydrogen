// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;
using Hydrogen;
using Hydrogen.Application;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Design;

namespace Hydrogen.Windows.Forms {

	public partial class MainForm : LiteMainForm {

		public MainForm() {
			InitializeComponent();
		}

		#region Form Properties

		public ToolStrip ToolStrip => _toolStrip;

		public MenuStrip MenuStrip => _menuStrip;

		public ToolStripStatusLabel StatusLabel { get => _statusLabel; }

		public ToolStripMenuItem PurchaseFullVersionToolStripMenuItem { get; private set; }

		public ToolStripMenuItem HelpToolStripMenuItem { get; private set; }

		#endregion

		#region Form Methods

		protected override void OnFirstActivated() {
			base.OnFirstActivated();
			if (!Tools.Runtime.IsDesignMode) {
				try {
					var licenseProvider = HydrogenFramework.Instance.ServiceProvider.GetService<IProductLicenseProvider>();
					// Show/Hide register menu item based on what's happened with the user nag screen
					if (licenseProvider.TryGetLicense(out var license) && license.License.Item.FeatureLevel == ProductLicenseFeatureLevelDTO.Free) {
						PurchaseFullVersionToolStripMenuItem.Visible = true;
					} else {
						PurchaseFullVersionToolStripMenuItem.Visible = false;
					}
				} catch (ProductLicenseTamperedException error) {
					ReportError(error);
					Exit(true);
				}
			}
		}

		#endregion

		#region IUserInterfaceServices Overrides

		public override string Status {
			get {
				return _statusLabel.Text;
			}
			set {
				ExecuteInUIFriendlyContext( () => _statusLabel.Text = value );
			}
		}

		#endregion

		#region Event Handlers

		protected virtual void RequestAFeature_Click(object sender, EventArgs e) {
			try {
				ShowRequestFeatureDialog();
			} catch (Exception error) {
				ReportError(error);
			}
		}

		protected virtual void SendComment_Click(object sender, EventArgs e) {
			try {
				ShowSendCommentDialog();
			} catch (Exception error) {
				ReportError(error);
			}
		}

		protected virtual void ReportABug_Click(object sender, EventArgs e) {
			try {
				ShowSubmitBugReportDialog();
			} catch (Exception error) {
				ReportError(error);
			}
		}

		protected virtual void About_Click(object sender, EventArgs e) {
			try {
				ShowAboutBox();
			} catch (Exception error) {
				ReportError(error);
			}
		}

		protected virtual void ContextHelp_Click(object sender, EventArgs e) {
			try {
				var helpServices = HydrogenFramework.Instance.ServiceProvider.GetService<IHelpServices>();
				helpServices.ShowHelp();
			} catch (Exception error) {
				ReportError(error);
			}
		}

		protected virtual void UserGuide_Click(object sender, EventArgs e) {
			try {
				var helpServices = HydrogenFramework.Instance.ServiceProvider.GetService<IHelpServices>();
				helpServices.ShowHelp();
			} catch (Exception error) {
				ReportError(error);
			}
		}

		protected virtual void PurchaseFullVersion_Click(object sender, EventArgs e) {
			try {
				var productLicenseEnforcer = HydrogenFramework.Instance.ServiceProvider.GetService<IProductLicenseEnforcer>();
				productLicenseEnforcer.CalculateRights(out var nagMessage);
				ShowNagScreen(nagMessage);
			} catch (Exception error) {
				ReportError(error);
			}
		}

		protected virtual void Exit_Click(object sender, EventArgs e) {
			try {
				Exit(false);
			} catch (Exception error) {
				ReportError(error);
			}
		}

		protected virtual void MainForm_HelpRequested(object sender, HelpEventArgs hlpevent) {
			try {
				var helpServices = HydrogenFramework.Instance.ServiceProvider.GetService<IHelpServices>();
				helpServices.ShowHelp();
			} catch (Exception error) {
				ReportError(error);
			}
		}

		#endregion


	
	}
}
