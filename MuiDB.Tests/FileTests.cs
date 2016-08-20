// <copyright file="FileTests.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using FluentAssertions;
    using fmdev.MuiDB;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileTests
    {
        [TestMethod]
        public void FileTest()
        {
            var filename = Guid.NewGuid().ToString();
            var f = new MuiDB.File(filename, File.OpenMode.CreateIfMissing);

            f.Filename.Should().BeSameAs(filename);
            f.OutputFiles.Should().BeEmpty();
            f.Translations.Should().BeEmpty();
            f.GetDocumentCopy().ToString(System.Xml.Linq.SaveOptions.DisableFormatting)
                .Should().BeEquivalentTo(MuiDB.File.EmptyDocument, "file should be created if it does not exist and therefore match match default empty file");
        }

        [TestMethod]
        public void AddOrUpdateTranslationTest()
        {
            var filename = Guid.NewGuid().ToString();
            Assert.IsFalse(System.IO.File.Exists(filename), "new file must not exist before save");
            var f = new MuiDB.File(filename, File.OpenMode.CreateIfMissing);
            f.Save();
            Assert.IsTrue(System.IO.File.Exists(filename), "new file must exist after save");
            XDocument.Load(filename).Should().BeEquivalentTo(f.GetDocumentCopy(), "saved file should have same content as document in memory");
        }

        [TestMethod]
        [TestCategory("IO")]
        public void SaveTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void VerifyMissingTranslationsTest()
        {
            var f = new File("..\\..\\TestData\\MissingTranslations.xml");

            bool exceptionWasThrown = false;
            try
            {
                f.Verify();
            }
            catch (MissingTranslationsException e)
            {
                exceptionWasThrown = true;
                var expected = new List<string>() { "item1:de", "item2:en" };
                e.Items.ShouldBeEquivalentTo(expected, "missing items should match expected");
            }
            finally
            {
                exceptionWasThrown.Should().BeTrue("Verify must throw MissingTranslationsException on missing translations");
            }
        }

        [TestMethod]
        public void SaveAsResXTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void SaveAsResXTest1()
        {
            Assert.Fail();
        }
    }
}