﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using System.Linq;
using Hydrogen.ObjectSpaces;
using NUnit.Framework;


namespace Hydrogen.Tests.ObjectSpaces;

[TestFixture]
public class PersistenceIgnoranceTests {


	[Test]
	[TestCaseSource(typeof(TestsHelper), nameof(TestsHelper.PersistentIgnorantTestCases))]
	public void PI_Simple(TestTraits testTraits) {
		using var objectSpace = TestsHelper.CreateObjectSpace(testTraits);
		
		using (objectSpace.EnterAccessScope()) {
			var account = objectSpace.New<Account>();
			account.Name = "Herman";
			objectSpace.Flush();
		}
		Assert.That(objectSpace.Count<Account>(), Is.EqualTo(1));

		var acc = objectSpace.Get<Account>(0);

		Assert.That(acc.Name, Is.EqualTo("Herman"));

	}
}
