//-----------------------------------------------------------------------
// <copyright file="ILicenseKeyDecoder.cs" company="Sphere 10 Software">
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

#if !__MOBILE__


namespace Sphere10.Framework.Application {
	public interface ILicenseKeyDecoder {

		ProductLicense Decode(string key);

		bool TryDecode(string key, out ProductLicense license);
	}
}

#endif
