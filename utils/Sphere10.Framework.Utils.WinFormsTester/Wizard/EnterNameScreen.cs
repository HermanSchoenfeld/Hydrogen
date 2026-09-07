// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System.Threading.Tasks;

namespace Sphere10.Framework.Utils.WinFormsTester.Wizard;

public partial class EnterNameScreen : DemoWizardScreenBase {
	public EnterNameScreen() {
		InitializeComponent();
	}

	public override Task<Result> Validate() {
		var Validation = Result.Default;
		if (string.IsNullOrWhiteSpace(textBox1.Text))
			Validation.AddError("Enter your name.");
		if (!checkBox1.Checked)
			Validation.AddError("Check the confirmation box to proceed.");
		return Task.FromResult(Validation);
	}

	public override Task OnNext() {
		CopyUIToModel();
		return Task.CompletedTask;
	}

	protected override void CopyUIToModel() => Model.Name = textBox1.Text.Trim();

	protected override void CopyModelToUI() {
		if (Wizard != null)
			textBox1.Text = Model.Name;
	}
}
