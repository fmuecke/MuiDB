// <copyright file="MuiDBFileTests.cs" company="Florian Mücke">
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
    public class MuiDBFileTests
    {
        [TestMethod]
        public void LoadFileTest()
        {
            var filename = Guid.NewGuid().ToString();
            var f = new MuiDB.MuiDBFile(filename, MuiDBFile.OpenMode.CreateIfMissing);

            f.Filename.Should().BeSameAs(filename);
            f.TargetFiles.Should().BeEmpty();
            f.Items.Should().BeEmpty();
            f.GetDocumentCopy().ToString(System.Xml.Linq.SaveOptions.DisableFormatting)
                .Should().BeEquivalentTo(MuiDB.MuiDBFile.EmptyDocument, "file should be created if it does not exist and therefore match match default empty file");
        }

        [TestMethod]
        public void ItemsAreNotEmpty()
        {
            var f = new MuiDB.MuiDBFile("..\\..\\TestData\\Sample.xml");
            f.Items.Should().NotBeEmpty("the sample must have translation items!");
        }

        [TestMethod]
        public void TextsAreNotEmpty()
        {
            var f = new MuiDB.MuiDBFile("..\\..\\TestData\\Sample.xml");

            foreach (var item in f.Items)
            {
                item.Texts.Should().NotBeEmpty("the sample data has translations defined");
            }
        }

        [TestMethod]
        public void SaveNewTest()
        {
            var filename = Guid.NewGuid().ToString();
            Assert.IsFalse(System.IO.File.Exists(filename), "new file must not exist before save");
            var f = new MuiDB.MuiDBFile(filename, MuiDBFile.OpenMode.CreateIfMissing);
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
        public void SaveExistingTest()
        {
            var f = new MuiDB.MuiDBFile("..\\..\\TestData\\Sample.xml");
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
            var f = new MuiDBFile("..\\..\\TestData\\MissingTranslations.xml");

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
        public void ReadProjectTitle()
        {
            var f = new MuiDBFile("..\\..\\TestData\\Sample.xml");
            f.ProjectTitle.Should().Be("test sample");
        }

        [TestMethod]
        public void ExportInvalidLanguageTest()
        {
            var f = new MuiDBFile("..\\..\\TestData\\Sample.xml");
            f.GetLanguages().Should().NotContain("fr");

            try
            {
                f.ExportResX("sdfs", "fr");
                Assert.Fail("Export of unknown language must lead to ArgumentException");
            }
            catch (ArgumentException e)
            {
                e.Message.ShouldBeEquivalentTo("'fr' is not a configured language.");
            }
        }

        [TestMethod]
        public void ImportNewItemTest()
        {
            var f = new MuiDBFile("new", MuiDBFile.OpenMode.CreateIfMissing);

            var id = "someRandomNewId";
            var lang = "de";
            var state = "new";
            var value = "Ein Ring, sie alle zu kechten, sie alle zu finden.";

            f.Items.Should().NotContain((i) => i.Id == id);
            f.GetLanguages().Should().NotContain(lang);
            f.AddOrUpdateString(id, lang, value, state, null);
            f.GetLanguages().Should().Contain(lang);
            f.Items.Should().Contain((i) => i.Id == id);
            var item = f.Items.First((i) => i.Id == id);
            item.Texts.Should().ContainKey(lang);
            item.Texts[lang].State.Should().Be(state);
            item.Texts[lang].Value.Should().Be(value);
            item.Comments.Should().BeEmpty();

            // import second language
            var lang2 = "en";
            var state2 = "translated";
            var value2 = "One Ring to rule them all, One Ring to find them.";
            var comment = "This is a test";
            f.AddOrUpdateString(id, lang2, value2, state2, comment);
            f.GetLanguages().Should().Contain(lang2);
            f.Items.Should().Contain((i) => i.Id == id);
            item = f.Items.First((i) => i.Id == id);
            item.Texts.Should().ContainKey(lang2);
            item.Texts[lang2].State.Should().Be(state2);
            item.Texts[lang2].Value.Should().Be(value2);
            item.Comments.Should().Contain(MuiDBFile.NeutralLanguage, comment);

            // check if document is compliant with schema
            // (comment node must be first child element of the item etc.)
            f.Validate();
        }

        [TestMethod]
        public void ImportExistingLanguageFromResxTest()
        {
            var f = new MuiDBFile("..\\..\\TestData\\Sample.xml");
            f.GetLanguages().Should().Contain("de");
            f.ImportResX("..\\..\\TestData\\Sample.de.resx", "de");
            f.GetLanguages().Should().Contain("de");
        }

        [TestMethod]
        public void SampleSchemaTest()
        {
            var f = new MuiDB.MuiDBFile("..\\..\\TestData\\Sample.xml");
            f.Validate();
        }

        [TestMethod]
        public void ExportResXTest()
        {
            var f = new MuiDB.MuiDBFile("..\\..\\TestData\\Sample.xml");
            f.TargetFiles.Should().NotBeEmpty("the sample has target files defined for export");
            f.TargetFiles.Count().ShouldBeEquivalentTo(3, "there are three target files defined in the sample");

            var tempFile_en = System.IO.Path.GetTempFileName();
            var tempFile_de = System.IO.Path.GetTempFileName();

            try
            {
                f.ExportResX(tempFile_en, "en", MuiDBFile.SaveOptions.SortEntries);
                f.ExportResX(tempFile_de, "de");

                var content_en = System.IO.File.ReadAllLines(tempFile_en);
                var content_de = System.IO.File.ReadAllLines(tempFile_de);
                var expected_en = System.IO.File.ReadAllLines("..\\..\\TestData\\Sample.en.resx");
                var expected_de = System.IO.File.ReadAllLines("..\\..\\TestData\\Sample.de.resx");

                for (int i = 0; i < content_en.Length; ++i)
                {
                    content_en[i].Should().Be(expected_en[i], "the result must be sorted and contain the right values");
                }

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