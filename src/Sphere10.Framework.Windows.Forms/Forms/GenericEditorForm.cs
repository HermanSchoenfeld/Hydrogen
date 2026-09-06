// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sphere10.Framework.Windows.Forms;

public partial class GenericEditorForm : Form {
	public GenericEditorForm() : this(null, false) {

	}

	public GenericEditorForm(object entity, bool readOnly) {
		InitializeComponent();
		if (entity != null) {
			_propertyGrid.SelectedObject = entity;
		}
		_propertyGrid.Enabled = !readOnly;
	}

	/// <summary>Awaitably shows the generic editor without blocking the calling context.</summary>
	public static async Task ShowFormAsync(object entity, bool readOnly) {
		using var form = new GenericEditorForm(entity, readOnly);
		await form.ShowDialogAsync();
	}

	private void _closeButton_Click(object sender, EventArgs e) {
		Close();
	}
}

