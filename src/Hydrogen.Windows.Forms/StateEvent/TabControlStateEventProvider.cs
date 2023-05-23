﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Hydrogen.Windows.Forms;

public class TabControlStateEventProvider : ContainerControlStateEventProviderBase<TabControl> {

	protected override IEnumerable<Control> GetChildControls(TabControl control) 
		=> base.GetChildControls(control).Union(control.TabPages.Cast<TabPage>());

}