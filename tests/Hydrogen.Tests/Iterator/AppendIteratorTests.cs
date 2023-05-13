// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using NUnit.Framework.Constraints;
using Hydrogen;

namespace Hydrogen.Tests {

    [TestFixture]
	[Parallelizable(ParallelScope.Children)]
    public class AppendIteratorTests {

        [Test]
        public void TestUnionAntiPattern() {
            var data = new[] {"one"};
            var union = data.Union("one");
            var result = union.ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("one", result[0]);
        }

        [Test]
        public void TestConcat() {
            var data = new[] { "one" };
            var union = data.Concat("one");
            var result = union.ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("one", result[0]);
            Assert.AreEqual("one", result[0]);
        }


    }

}
