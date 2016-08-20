// <copyright file="File.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Resources;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    public class File
    {
        public const string EmptyDocument = "<muidb><files /><translations /></muidb>";
        public const string NeutralLanguage = "*";
        private const string LanguagesName = "languages";
        private const string TranslationsName = "translations";
        private const string ItemName = "item";
        private const string IdName = "id";
        private const string CommentName = "comment";
        private const string TextName = "text";
        private const string LangName = "lang";
        private const string StateName = "state";
        private const string OutputFilesName = "files";
        private const string ResxName = "resx";

        ////private const string XlfName = "files";

        private XDocument doc;
        private XNamespace ns;

        public File(string filename, OpenMode mode)
        {
            Filename = filename;
            if (mode == OpenMode.CreateIfMissing && !System.IO.File.Exists(Filename))
            {
                doc = XDocument.Parse(EmptyDocument);
            }
            else
            {
                doc = XDocument.Load(Filename);
            }

            ns = doc.Root.Name.Namespace;
        }

        public File(string filename)
            : this(filename, OpenMode.OpenExisting)
        {
        }

        public enum AddOrUpdateResult
        {
            Added,
            Updated
        }

        public enum OpenMode
        {
            OpenExisting,
            CreateIfMissing
        }

        [Flags]
        public enum SaveOptions
        {
            None = 0,
            SortEntries = 1,
            IncludeComments = 2
        }

        public string Filename { get; private set; }

        public IEnumerable<OutputFile> OutputFiles
        {
            get
            {
                var filesNode = doc.Root.Element(ns + OutputFilesName);
                if (filesNode == null)
                {
                    return new List<OutputFile>();
                }

                var fileNodes = filesNode.Elements(ns + ResxName);
                if (!fileNodes.Any())
                {
                    return new List<OutputFile>();
                }

                return fileNodes.Select(f =>
                {
                    return new OutputFile()
                    {
                        Name = f.Value,
                        Lang = f.Attribute(ns + LangName)?.Value
                    };
                }).ToList();
            }
        }

        public IEnumerable<Item> Translations
        {
            get
            {
                return doc.Descendants(ns + ItemName).Select(i =>
                {
                    var item = new Item();
                    item.Id = i.Attribute(IdName)?.Value;

                    foreach (var t in i.Elements(ns + TextName))
                    {
                        var lang = t.Attribute(ns + LangName)?.Value;
                        if (string.IsNullOrWhiteSpace(lang))
                        {
                            throw new Exception($"Item '{item.Id}' does not have a valid 'lang' attribute in the text element.");
                        }
                        var state = t.Attribute(ns + StateName)?.Value;
                        item.Texts[lang] = new Text() { State = state, Value = t.Value };
                    }

                    foreach (var c in i.Elements(ns + CommentName))
                    {
                        ////var lang = c.Attribute(ns + LangName)?.Value;
                        ////lang = string.IsNullOrWhiteSpace(lang) ? NeutralLanguage : lang;
                        ////item.Comments[lang] = c.Value;
                        item.Comments[NeutralLanguage] = c.Value;
                    }

                    return item;
                });
            }
        }

        public XDocument GetDocumentCopy()
        {
            return new XDocument(doc);
        }

        public List<string> GetLanguages()
        {
            var langs = doc.Root.Attribute(ns + LanguagesName)?.Value;
            if (langs == null || string.IsNullOrWhiteSpace(langs))
            {
                return new List<string>();
            }

            return langs.Split(',').Select(l => l.Trim()).ToList();
        }

        public void SetLanguages(IEnumerable<string> languages)
        {
            doc.Root.SetAttributeValue(ns + LanguagesName, string.Join(",", languages));
        }

        public AddOrUpdateResult AddOrUpdateTranslation(string id, string lang, string text, string state, string comment)
        {
            var translationsNode = doc.Root.Element(ns + TranslationsName);
            if (translationsNode == null)
            {
                translationsNode = new XElement(ns + TranslationsName);
                doc.Root.Add(translationsNode);
            }

            var items = translationsNode.Elements(ns + ItemName).Where(i => i.Attribute(ns + IdName)?.Value == id);
            XElement item;
            AddOrUpdateResult result;
            if (items.Any())
            {
                item = items.First();
                result = AddOrUpdateResult.Updated;
            }
            else
            {
                item = new XElement(ns + ItemName);
                item.SetAttributeValue(ns + IdName, id);
                translationsNode.Add(item);
                result = AddOrUpdateResult.Added;
            }

            var textNodes = item.Elements(ns + TextName).Where(t => t.Attribute(ns + LangName)?.Value == lang);
            XElement textNode;
            if (textNodes.Any())
            {
                textNode = textNodes.First();
            }
            else
            {
                textNode = new XElement(ns + TextName);
                textNode.SetAttributeValue(ns + LangName, lang);
                item.Add(textNode);
            }

            textNode.SetValue(text);
            textNode.SetAttributeValue(ns + StateName, state);

            if (!string.IsNullOrWhiteSpace(comment))
            {
                var commentNodes = item.Elements(ns + CommentName);
                ////var commentNodes = item.Elements(ns + CommentName).Where(c => c.Attribute(ns + LangName)?.Value == lang);
                XElement commentNode;
                if (commentNodes.Any())
                {
                    commentNode = commentNodes.First();
                }
                else
                {
                    commentNode = new XElement(ns + CommentName);
                    ////commentNode.SetAttributeValue(ns + LangName, lang);
                    item.Add(commentNode);
                }

                commentNode.SetValue(comment);
            }

            return result;
        }

        public void Save()
        {
            Save(Filename);
        }

        public void Save(string filename)
        {
            var translationsNode = doc.Root.Element(ns + TranslationsName);
            var items = translationsNode.Elements(ns + ItemName).OrderBy(i => i.Attribute(ns + IdName).Value);
            translationsNode.ReplaceAll(items);
            doc.Save(filename);
        }

        // will throw exception if muidb is not valid
        public void Verify()
        {
            var missingTranslations = new List<string>();
            var langs = GetLanguages();
            foreach (var item in Translations)
            {
                foreach (var l in langs)
                {
                    if (!item.Texts.ContainsKey(l) && !item.Texts.ContainsKey(NeutralLanguage))
                    {
                        missingTranslations.Add($"{item.Id}:{l}");
                    }
                }
            }

            if (missingTranslations.Any())
            {
                throw new MissingTranslationsException(missingTranslations);
            }
        }

        public void ExportResX(string fileName, string language)
        {
            ExportResX(fileName, language, SaveOptions.None);
        }

        public void ExportResX(string filename, string language, SaveOptions options)
        {
            if (!GetLanguages().Contains(language))
            {
                throw new Exception($"{language} is not a configured language.");
            }

            var entries = new List<ResXEntry>();
            foreach (var item in Translations)
            {
                var entry = new ResXEntry();
                Text text;
                if (!item.Texts.TryGetValue(language, out text) && !item.Texts.TryGetValue(NeutralLanguage, out text))
                {
                    throw new MissingTranslationsException(new List<string>() { $"{item.Id};{language}" });
                }
                entry.Id = item.Id;
                entry.Value = text.Value;

                if (options.HasFlag(SaveOptions.IncludeComments))
                {
                    string comment;
                    if (item.Comments.TryGetValue(language, out comment) || item.Comments.TryGetValue(NeutralLanguage, out comment))
                    {
                        entry.Comment = comment;
                    }
                }

                entries.Add(entry);
            }

            if (options.HasFlag(SaveOptions.SortEntries))
            {
                entries.Sort();
            }

            ResXParser.Write(filename, entries);
        }

        public ImportResult ImportResX(string filename, string language)
        {
            var result = new ImportResult();
            var entries = ResXParser.Read(filename);

            foreach (var e in entries)
            {
                var addOrUpdate = AddOrUpdateTranslation(e.Id, language, e.Value, "new", e.Comment);

                if (addOrUpdate == AddOrUpdateResult.Added)
                {
                    result.AddedItems.Add(e.Id);
                }

                if (addOrUpdate == AddOrUpdateResult.Updated)
                {
                    result.UpdatedItems.Add(e.Id);
                }
            }

            return result;
        }
    }
}