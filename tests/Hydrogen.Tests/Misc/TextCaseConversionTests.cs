//-----------------------------------------------------------------------
// <copyright file="UrlToolTests.cs" company="Sphere 10 Software">
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

using NUnit.Framework;
using Tools;

namespace Hydrogen.Tests {

    [TestFixture]
	[Parallelizable(ParallelScope.Children)]
    public class TextCasingTests {

        #region Pascal Case

        [Test]
        public void PascalTest_0() {
            Assert.That(Tools.Text.ToCasing(TextCasing.PascalCase, "123alpha"), Is.EqualTo("123Alpha"));
        }

        [Test]
        public void PascalTest_1() {
            Assert.That(Tools.Text.ToCasing(TextCasing.PascalCase, "123Alpha"), Is.EqualTo("123Alpha"));
        }


        [Test]
        public void PascalTest_2() {
            Assert.That(Tools.Text.ToCasing(TextCasing.PascalCase, "alpha123"), Is.EqualTo("Alpha123"));
        }

        [Test]
        public void PascalTest_3() {
            Assert.That(Tools.Text.ToCasing(TextCasing.PascalCase, "Alpha123"), Is.EqualTo("Alpha123"));
        }

        [Test]
        public void PascalTest_4() {
            Assert.That(Tools.Text.ToCasing(TextCasing.PascalCase, "ALpha123"), Is.EqualTo("Alpha123"));
        }

        [Test]
        public void PascalTest_5() {
            Assert.That(Tools.Text.ToCasing(TextCasing.PascalCase, "ALPha123BEta456"), Is.EqualTo("Alpha123Beta456"));
        }

        [Test]
        public void PascalTest_6() {
            Assert.That(Tools.Text.ToCasing(TextCasing.PascalCase, "alphaBeta"), Is.EqualTo("AlphaBeta"));
        }
        
        [Test]
        public void PascalTest_7() {
            Assert.That(Tools.Text.ToCasing(TextCasing.PascalCase, "AlphaBeta"), Is.EqualTo("AlphaBeta"));
        }

        
        [Test]
        public void PascalTest_Punctuators_1([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.PascalCase, $"alpha12{punctuator}3beta456"), Is.EqualTo("Alpha123Beta456"));
        }
                
        [Test]
        public void PascalTest_Punctuators_2([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.PascalCase, $"alpha123{punctuator}beta456"), Is.EqualTo("Alpha123Beta456"));
        }

        [Test]
        public void PascalTest_Punctuators_4([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.PascalCase, $"alpha12{punctuator}3BETA456"), Is.EqualTo("Alpha123Beta456"));
        }

        [Test]
        public void PascalTest_Punctuators_5([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.PascalCase, $"alpha12{punctuator}3BetaGamma456"), Is.EqualTo("Alpha123BetaGamma456"));
        }

        #endregion

        #region Camel Case

        [Test]
        public void CamelTest_0() {
            Assert.That(Tools.Text.ToCasing(TextCasing.CamelCase, "123alpha"), Is.EqualTo("123Alpha"));
        }

        [Test]
        public void CamelTest_1() {
            Assert.That(Tools.Text.ToCasing(TextCasing.CamelCase, "123Alpha"), Is.EqualTo("123Alpha"));
        }


        [Test]
        public void CamelTest_2() {
            Assert.That(Tools.Text.ToCasing(TextCasing.CamelCase, "alpha123"), Is.EqualTo("alpha123"));
        }

        [Test]
        public void CamelTest_3() {
            Assert.That(Tools.Text.ToCasing(TextCasing.CamelCase, "Alpha123"), Is.EqualTo("alpha123"));
        }

        [Test]
        public void CamelTest_4() {
            Assert.That(Tools.Text.ToCasing(TextCasing.CamelCase, "ALpha123"), Is.EqualTo("aLpha123"));
        }

        [Test]
        public void CamelTest_5() {
            Assert.That(Tools.Text.ToCasing(TextCasing.CamelCase, "ALPha123BEta456"), Is.EqualTo("aLpha123Beta456"));
        }

        [Test]
        public void CamelTest_6() {
            Assert.That(Tools.Text.ToCasing(TextCasing.CamelCase, "AlphaBeta"), Is.EqualTo("alphaBeta"));
        }


        [Test]
        public void CamelTest_7() {
            Assert.That(Tools.Text.ToCasing(TextCasing.CamelCase, "alphaBeta"), Is.EqualTo("alphaBeta"));
        }
        
        [Test]
        public void CamelTest_Punctuators_1([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.CamelCase, $"alpha12{punctuator}3beta456"), Is.EqualTo("alpha123Beta456"));
        }
                
        [Test]
        public void CamelTest_Punctuators_2([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.CamelCase, $"alpha123{punctuator}beta456"), Is.EqualTo("alpha123Beta456"));
        }

        [Test]
        public void CamelTest_Punctuators_4([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.CamelCase, $"alpha12{punctuator}3BETA456"), Is.EqualTo("alpha123Beta456"));
        }

        [Test]
        public void CamelTest_Punctuators_5([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.CamelCase, $"alpha12{punctuator}3BetaGamma456"), Is.EqualTo("alpha123BetaGamma456"));
        }

        #endregion

        #region Snake Case

        [Test]
        public void SnakeTest_0() {
            Assert.That(Tools.Text.ToCasing(TextCasing.SnakeCase, "123alpha"), Is.EqualTo("123ALPHA"));
        }

        [Test]
        public void SnakeTest_1() {
            Assert.That(Tools.Text.ToCasing(TextCasing.SnakeCase, "123Alpha"), Is.EqualTo("123ALPHA"));
        }


        [Test]
        public void SnakeTest_2() {
            Assert.That(Tools.Text.ToCasing(TextCasing.SnakeCase, "alpha123"), Is.EqualTo("ALPHA123"));
        }

        [Test]
        public void SnakeTest_3() {
            Assert.That(Tools.Text.ToCasing(TextCasing.SnakeCase, "Alpha123"), Is.EqualTo("ALPHA123"));
        }

        [Test]
        public void SnakeTest_4() {
            Assert.That(Tools.Text.ToCasing(TextCasing.SnakeCase, "ALpha123"), Is.EqualTo("ALPHA123"));
        }

        [Test]
        public void SnakeTest_5() {
            Assert.That(Tools.Text.ToCasing(TextCasing.SnakeCase, "ALPha123BEta456"), Is.EqualTo("ALPHA123BETA456"));
        }

        [Test]
        public void SnakeTest_6() {
            Assert.That(Tools.Text.ToCasing(TextCasing.SnakeCase, "ALPHA_BETA"), Is.EqualTo("ALPHA_BETA"));
        }
        
        [Test]
        public void SnakeTest_Punctuators_1([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.SnakeCase, $"alpha12{punctuator}3beta456"), Is.EqualTo("ALPHA12_3BETA456"));
        }
                
        [Test]
        public void SnakeTest_Punctuators_2([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.SnakeCase, $"alpha123{punctuator}beta456"), Is.EqualTo("ALPHA123_BETA456"));
        }

        [Test]
        public void SnakeTest_Punctuators_4([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.SnakeCase, $"alpha12{punctuator}3BETA456"), Is.EqualTo("ALPHA12_3BETA456"));
        }

        [Test]
        public void SnakeTest_Punctuators_5([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.SnakeCase, $"alpha12{punctuator}3BetaGamma456"), Is.EqualTo("ALPHA12_3BETAGAMMA456"));
        }

        #endregion

        #region Kebab Case

        [Test]
        public void KebabTest_0() {
            Assert.That(Tools.Text.ToCasing(TextCasing.KebabCase, "123alpha"), Is.EqualTo("123alpha"));
        }

        [Test]
        public void KebabTest_1() {
            Assert.That(Tools.Text.ToCasing(TextCasing.KebabCase, "123Alpha"), Is.EqualTo("123alpha"));
        }


        [Test]
        public void KebabTest_2() {
            Assert.That(Tools.Text.ToCasing(TextCasing.KebabCase, "alpha123"), Is.EqualTo("alpha123"));
        }

        [Test]
        public void KebabTest_3() {
            Assert.That(Tools.Text.ToCasing(TextCasing.KebabCase, "Alpha123"), Is.EqualTo("alpha123"));
        }

        [Test]
        public void KebabTest_4() {
            Assert.That(Tools.Text.ToCasing(TextCasing.KebabCase, "ALpha123"), Is.EqualTo("alpha123"));
        }

        [Test]
        public void KebabTest_5() {
            Assert.That(Tools.Text.ToCasing(TextCasing.KebabCase, "ALPha123BEta456"), Is.EqualTo("alpha123beta456"));
        }

        [Test]
        public void KebabTest_6() {
            Assert.That(Tools.Text.ToCasing(TextCasing.KebabCase, "alpha-beta"), Is.EqualTo("alpha-beta"));
        }
        
        [Test]
        public void KebabTest_Punctuators_1([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.KebabCase, $"alpha12{punctuator}3beta456"), Is.EqualTo("alpha12-3beta456"));
        }
                
        [Test]
        public void KebabTest_Punctuators_2([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.KebabCase, $"alpha123{punctuator}beta456"), Is.EqualTo("alpha123-beta456"));
        }

        [Test]
        public void KebabTest_Punctuators_4([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.KebabCase, $"alpha12{punctuator}3BETA456"), Is.EqualTo("alpha12-3beta456"));
        }

        [Test]
        public void KebabTest_Punctuators_5([PunctuatorValues] string punctuator) {
            Assert.That(Tools.Text.ToCasing(TextCasing.KebabCase, $"alpha12{punctuator}3BetaGamma456"), Is.EqualTo("alpha12-3betagamma456"));
        }

        #endregion

        private class PunctuatorValues : ValuesAttribute {
            public PunctuatorValues() : base("\t", " ", "\t  ", "-", "_", ":", "--", ".", ",") {
            }
        }
    }

}
