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
    using System.Xml;
    using System.Xml.Linq;
    using FluentAssertions;
    using fmdev.MuiDB;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileTests
    {
        [TestMethod]
        public void LoadFileTest()
        {
            var filename = Guid.NewGuid().ToString();
            var f = new MuiDB.File(filename, File.OpenMode.CreateIfMissing);

            f.Filename.Should().BeSameAs(filename);
            f.OutputFiles.Should().BeEmpty();
            f.Strings.Should().BeEmpty();
            f.GetDocumentCopy().ToString(System.Xml.Linq.SaveOptions.DisableFormatting)
                .Should().BeEquivalentTo(MuiDB.File.EmptyDocument, "file should be created if it does not exist and therefore match match default empty file");
        }

        [TestMethod]
        public void SaveNewTest()
        {
            var filename = Guid.NewGuid().ToString();
            Assert.IsFalse(System.IO.File.Exists(filename), "new file must not exist before save");
            var f = new MuiDB.File(filename, File.OpenMode.CreateIfMissing);
            try
            {
                f.Save();
                Assert.IsTrue(System.IO.File.Exists(filename), "new file must exist after save");
                var doc = XDocument.Load(filename);
                doc.Should().BeEquivalentTo(f.GetDocumentCopy(), "saved file should have same content as document in memory");
            }
            finally
            {
                System.IO.File.Delete(filename);
            }
        }

        [TestMethod]
        [TestCategory("IO")]
        public void SaveExistingTest()
        {
            var f = new MuiDB.File("..\\..\\TestData\\Sample.xml");
            var tempFile = System.IO.Path.GetTempFileName();
            try
            {
                f.Save(tempFile);
                var doc = XDocument.Load(tempFile);
                doc.Should().BeEquivalentTo(f.GetDocumentCopy(), "saved file should have same content as document in memory");
            }
            finally
            {
                System.IO.File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void VerifyMissingTranslationsTest()
        {
            var f = new File("..\\..\\TestData\\MissingTranslations.xml");

            bool exceptionWasThrown = false;
            try
            {
                f.Validate();
            }
            catch (MissingTranslationsException e)
            {
                exceptionWasThrown = true;
                var expected = new List<string>() { "item1:de", "item2:en" };
                e.Items.ShouldBeEquivalentTo(expected, "missing items should match expected");
            }
            catch (Exception)
            {
            }
            finally
            {
                exceptionWasThrown.Should().BeTrue("Verify must throw MissingTranslationsException on missing translations");
            }
        }

        [TestMethod]
        public void ExportInvalidLanguageTest()
        {
            var f = new File("..\\..\\TestData\\Sample.xml");
            f.GetLanguages().Should().NotContain("fr");

            try
            {
                f.ExportResX("sdfs", "fr");
                Assert.Fail("Export of unknown language must lead to ArgumentException");
            }
            catch (ArgumentException e)
            {
                e.Message.ShouldBeEquivalentTo("fr is not a configured language.");
            }
        }

        [TestMethod]
        public void ImportNewLanguageTest()
        {
            var f = new File("new", File.OpenMode.CreateIfMissing);
            f.GetLanguages().Should().NotContain("de");
            f.ImportResX("..\\..\\TestData\\Sample.de.resx", "de");
            f.GetLanguages().Should().Contain("de");
        }

        [TestMethod]
        public void ImportExistingLanguageTest()
        {
            var f = new File("..\\..\\TestData\\Sample.xml");
            f.GetLanguages().Should().Contain("de");
            f.ImportResX("..\\..\\TestData\\Sample.de.resx", "de");
            f.GetLanguages().Should().Contain("de");
        }

        [TestMethod]
        public void ExportResXTest()
        {
            var f = new MuiDB.File("..\\..\\TestData\\Sample.xml");
            var tempFile_en = System.IO.Path.GetTempFileName();
            var tempFile_de = System.IO.Path.GetTempFileName();
            try
            {
                f.ExportResX(tempFile_en, "en", File.SaveOptions.IncludeComments | File.SaveOptions.SortEntries);
                f.ExportResX(tempFile_de, "de", File.SaveOptions.IncludeComments);

                var content_en = System.IO.File.ReadAllText(tempFile_en);
                var content_de = System.IO.File.ReadAllText(tempFile_de);
                var expected_en = System.IO.File.ReadAllText("..\\..\\TestData\\Sample.en.resx");
                var expected_de = System.IO.File.ReadAllText("..\\..\\TestData\\Sample.de.resx");

                content_en.ShouldBeEquivalentTo(expected_en, "Result must be sorted and contain the right values");
                content_de.ShouldBeEquivalentTo(expected_de, "Result must be sorted and contain the right values");
            }
            finally
            {
                System.IO.File.Delete(tempFile_en);
                System.IO.File.Delete(tempFile_de);
            }
        }
    }
}