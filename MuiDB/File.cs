// <copyright file="File.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Resources;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    public class File
    {
        public const string EmptyDocument = "<muidb><settings /><strings /></muidb>";
        public const string NeutralLanguage = "*";
        private const string LanguagesElementName = "languages";
        private const string StringsElementName = "strings";
        private const string ItemElementName = "item";
        private const string IdAttributeName = "id";
        private const string CommentElementName = "comment";
        private const string TextElementName = "text";
        private const string LangAttributeName = "lang";
        private const string StateAttributeName = "state";
        private const string SettingsElementName = "settings";
        private const string OutFileElementName = "out-file";

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
                var filesNode = doc.Root.Element(ns + SettingsElementName);
                if (filesNode == null)
                {
                    return new List<OutputFile>();
                }

                var fileNodes = filesNode.Elements(ns + OutFileElementName);
                if (!fileNodes.Any())
                {
                    return new List<OutputFile>();
                }

                return fileNodes.Select(f =>
                {
                    return new OutputFile()
                    {
                        Name = f.Value,
                        Lang = f.Attribute(LangAttributeName)?.Value
                    };
                }).ToList();
            }
        }

        public IEnumerable<Item> Strings
        {
            get
            {
                return doc.Descendants(ns + ItemElementName).Select(i =>
                {
                    var item = new Item();
                    item.Id = i.Attribute(IdAttributeName)?.Value;

                    foreach (var t in i.Elements(ns + TextElementName))
                    {
                        var state = t.Attribute(StateAttributeName)?.Value;

                        var lang = t.Attribute(LangAttributeName)?.Value;
                        if (string.IsNullOrWhiteSpace(lang))
                        {
                            lang = NeutralLanguage;
                        }

                        if (item.Texts.ContainsKey(lang))
                        {
                            throw new Exception($"Item '{item.Id}' has multiple entries for language '{lang}'.");
                        }

                        item.Texts[lang] = new Text() { State = state, Value = t.Value };
                    }

                    foreach (var c in i.Elements(ns + CommentElementName))
                    {
                        ////var lang = c.Attribute(LangName)?.Value;
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
            var langs = doc.Root.Element(ns + SettingsElementName).Element(ns + LanguagesElementName)?.Value;
            if (langs == null || string.IsNullOrWhiteSpace(langs))
            {
                return new List<string>();
            }

            return langs.Split(';').Select(l => l.Trim()).ToList();
        }

        public void SetLanguages(IEnumerable<string> languages)
        {
            var settingsNode = doc.Root.Element(ns + SettingsElementName);
            XElement langsNode;
            if (settingsNode == null)
            {
                settingsNode = new XElement(ns + SettingsElementName);
                langsNode = new XElement(ns + LanguagesElementName);
                settingsNode.Add(langsNode);
                doc.Root.Add(settingsNode);
            }
            else
            {
                langsNode = settingsNode.Element(ns + LanguagesElementName);
                if (langsNode == null)
                {
                    langsNode = new XElement(ns + LanguagesElementName);
                    settingsNode.AddFirst(langsNode);
                }
            }

            langsNode.SetValue(string.Join(";", languages));
        }

        public AddOrUpdateResult AddOrUpdateString(string id, string lang, string text, string state, string comment)
        {
            var stringsNode = doc.Root.Element(ns + StringsElementName);
            if (stringsNode == null)
            {
                stringsNode = new XElement(ns + StringsElementName);
                doc.Root.Add(stringsNode);
            }

            var items = stringsNode.Elements(ns + ItemElementName).Where(i => i.Attribute(IdAttributeName)?.Value == id);
            XElement item;
            AddOrUpdateResult result;
            if (items.Any())
            {
                item = items.First();
                result = AddOrUpdateResult.Updated;
            }
            else
            {
                item = new XElement(ns + ItemElementName);
                item.SetAttributeValue(ns + IdAttributeName, id);
                stringsNode.Add(item);
                result = AddOrUpdateResult.Added;
            }

            var textNodes = item.Elements(ns + TextElementName).Where(t => t.Attribute(LangAttributeName)?.Value == lang);
            XElement textNode;
            if (textNodes.Any())
            {
                textNode = textNodes.First();
            }
            else
            {
                textNode = new XElement(ns + TextElementName);
                textNode.SetAttributeValue(ns + LangAttributeName, lang);
                item.Add(textNode);
            }

            textNode.SetValue(text);
            textNode.SetAttributeValue(ns + StateAttributeName, state);

            if (!string.IsNullOrWhiteSpace(comment))
            {
                var commentNodes = item.Elements(ns + CommentElementName);
                ////var commentNodes = item.Elements(ns + CommentName).Where(c => c.Attribute(LangName)?.Value == lang);
                XElement commentNode;
                if (commentNodes.Any())
                {
                    commentNode = commentNodes.First();
                }
                else
                {
                    commentNode = new XElement(ns + CommentElementName);
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
            var stringsNode = doc.Root.Element(ns + StringsElementName);
            var items = stringsNode.Elements(ns + ItemElementName).OrderBy(i => i.Attribute(IdAttributeName).Value);
            stringsNode.ReplaceAll(items);
            doc.Save(filename);
        }

        // will throw exception if muidb is not valid
        public void Validate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var schemaStream = assembly.GetManifestResourceStream("MuiDBSchema.xsd"))
            {
                var schemas = new XmlSchemaSet();
                var schema = XmlSchema.Read(schemaStream, null);
                schemas.Add(schema);
                doc.Validate(schemas, null);
            }

            var missingTranslations = new List<string>();
            var langs = GetLanguages();
            foreach (var item in Strings)
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
                throw new ArgumentException($"{language} is not a configured language.");
            }

            var entries = new List<ResXEntry>();
            foreach (var item in Strings)
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
                var addOrUpdate = AddOrUpdateString(e.Id, language, e.Value, "new", e.Comment);

                if (addOrUpdate == AddOrUpdateResult.Added)
                {
                    result.AddedItems.Add(e.Id);
                }

                if (addOrUpdate == AddOrUpdateResult.Updated)
                {
                    result.UpdatedItems.Add(e.Id);
                }
            }

            if (!GetLanguages().Contains(language))
            {
                var langs = GetLanguages();
                langs.Add(language);
                SetLanguages(langs);
            }

            return result;
        }
    }
}