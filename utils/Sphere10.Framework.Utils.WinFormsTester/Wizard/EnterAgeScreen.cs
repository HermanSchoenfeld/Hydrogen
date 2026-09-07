// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System.Threading.Tasks;

namespace Sphere10.Framework.Utils.WinFormsTester.Wizard;

public partial class EnterAgeScreen : DemoWizardScreenBase {
	public EnterAgeScreen() {
		InitializeComponent();
	}

	public override Task<Result> Validate() {
		var Validation = int.TryParse(textBox1.Text, out var Age) && Age >= 0
			? Result.Success
			: Result.Error("Enter your age as a non-negative whole number.");
		return Task.FromResult(Validation);
	}

	public override Task OnNext() {
		CopyUIToModel();
		return Task.CompletedTask;
	}

	protected override void CopyUIToModel() => Model.Age = int.Parse(textBox1.Text);

	protected override void CopyModelToUI() {
		if (Wizard != null)
			textBox1.Text = Model.Age?.ToString() ?? string.Empty;
	}
}
