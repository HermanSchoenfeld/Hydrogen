//-----------------------------------------------------------------------
// <copyright file="AssemblyProductLicenseAttribute.cs" company="Sphere 10 Software">
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

using System.Runtime.InteropServices;
using System;

namespace Hydrogen.Application;

[ComVisible(true)]
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyProductLicenseAttribute : Attribute {
	private readonly string _license;
	public AssemblyProductLicenseAttribute(string userProductLicenseJson) {
		_license = userProductLicenseJson;
	}

	public ProductLicenseActivationDTO License => Tools.Json.ReadFromString<ProductLicenseActivationDTO>(_license);

}