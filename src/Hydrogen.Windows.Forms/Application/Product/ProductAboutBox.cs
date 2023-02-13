//-----------------------------------------------------------------------
// <copyright file="ProductAboutBox.cs" company="Sphere 10 Software">
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
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Hydrogen.Application;
using Hydrogen;
using Microsoft.Extensions.DependencyInjection;

namespace Hydrogen.Windows.Forms;

public partial class ProductAboutBox : ApplicationForm, IAboutBox {

	public ProductAboutBox() {
		InitializeComponent();
	}

	protected override void OnLoad(EventArgs e) {
		base.OnLoad(e);
		if (!Tools.Runtime.IsDesignMode) {
			Text = StringFormatter.FormatEx(this.Text);
			_label1.Text = StringFormatter.FormatEx(_label1.Text);
			_label4.Text = StringFormatter.FormatEx(_label4.Text);
			_label2.Text = StringFormatter.FormatEx(_label2.Text);
			_label3.Text = StringFormatter.FormatEx(_label3.Text);
			_link1.Text = StringFormatter.FormatEx(_link1.Text).TrimStart("https://").TrimStart("http://");
			_label9.Text = StringFormatter.FormatEx(_label9.Text);
			_label10.Text = StringFormatter.FormatEx(_label10.Text);
			_label11.Text = StringFormatter.FormatEx(_label11.Text);
			_label12.Text = StringFormatter.FormatEx(_label12.Text);
			_companyNumberLabel.Text = StringFormatter.FormatEx(_companyNumberLabel.Text);
		}
	}

	private void _productLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
		var url = StringFormatter.FormatEx("{CompanyUrl}");
		var websiteLauncher = HydrogenFramework.Instance.ServiceProvider.GetService<IWebsiteLauncher>();
		websiteLauncher.LaunchWebsite(url);
	}

}
