// <copyright file="MuiDBFile.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using ResX;

    public class MuiDBFile
    {
        public const string NeutralLanguage = "*";
        public static readonly string EmptyDocument = $"<muidb xmlns=\"http://github.com/fmuecke/MuiDB\"><{SettingsElementName} {BaseNameAttributeName}=\"\" {LanguagesAttributeName}=\"\" /><{ItemsElementName} /></muidb>";
        private const string SettingsElementName = "settings";
        private const string BaseNameAttributeName = "base-name";
        private const string CodeNamespaceAttributeName = "code-namespace";
        private const string LanguagesAttributeName = "languages";
        private const string ProjectTitleAttributeName = "project-title";
        private const string ItemsElementName = "items";
        private const string ItemElementName = "item";
        private const string IdAttributeName = "id";
        private const string TitleAttributeName = "title";
        private const string CommentElementName = "comment";
        private const string TextElementName = "text";
        private const string LangAttributeName = "lang";
        private const string StateAttributeName = "state";
        private const string TargetFileElementName = "target-file";
        private const string DefaultNewState = "new";
        private const string DesignerAttributeName = "designer";
        private const string DesignerValueInternal = "internal";
        private const string DesignerValuePublic = "public";

        private XDocument doc;
        private XNamespace ns;

        public MuiDBFile(string filename, OpenMode mode)
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

        public MuiDBFile(string filename)
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
            SkipComments = 2
        }

        public string Filename { get; private set; }

        public IEnumerable<TargetFile> TargetFiles
        {
            get
            {
                var settingsNode = doc.Root.Element(ns + SettingsElementName);
                if (settingsNode == null)
                {
                    return new List<TargetFile>();
                }

                var fileNodes = settingsNode.Elements(ns + TargetFileElementName);
                if (!fileNodes.Any())
                {
                    return new List<TargetFile>();
                }

                return fileNodes.Select(f =>
                {
                    var targetFile = new TargetFile()
                    {
                        Name = f.Value,
                        Lang = f.Attribute(LangAttributeName)?.Value
                    };

                    var designerValue = f.Attribute(DesignerAttributeName)?.Value;
                    if (designerValue != null && (designerValue == DesignerValueInternal || designerValue == DesignerValuePublic))
                    {
                        var className = settingsNode.Attribute(BaseNameAttributeName)?.Value;
                        if (string.IsNullOrWhiteSpace(className))
                        {
                            className = System.IO.Path.GetFileNameWithoutExtension(f.Value);
                        }

                        targetFile.Designer = new DesignerFile()
                        {
                            ClassName = className,
                            Namespace = settingsNode.Attribute(CodeNamespaceAttributeName)?.Value,
                            IsInternal = designerValue == DesignerValueInternal
                        };
                    }

                    return targetFile;
                }).ToList();
            }
        }

        public IEnumerable<Item> Items
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

                        item.Texts[lang] = new TextItem() { State = state, Value = t.Value };
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

        public string ProjectTitle
        {
            get
            {
                return doc.Root.Element(ns + SettingsElementName).Attribute(ProjectTitleAttributeName)?.Value;
            }

            set
            {
                doc.Root.Element(ns + SettingsElementName).SetAttributeValue(ProjectTitleAttributeName, value);
            }
        }

        public string BaseName
        {
            get
            {
                return doc.Root.Element(ns + SettingsElementName)?.Attribute(BaseNameAttributeName)?.Value;
            }

            set
            {
                doc.Root.Element(ns + SettingsElementName)?.SetAttributeValue(BaseNameAttributeName, value);
            }
        }

        public string CodeNamespace
        {
            get
            {
                return doc.Root.Element(ns + SettingsElementName)?.Attribute(CodeNamespaceAttributeName)?.Value;
            }

            set
            {
                doc.Root.Element(ns + SettingsElementName)?.SetAttributeValue(CodeNamespaceAttributeName, value);
            }
        }

        public XDocument GetDocumentCopy()
        {
            return new XDocument(doc);
        }

        public List<string> GetLanguages()
        {
            var langs = doc.Root.Element(ns + SettingsElementName)?.Attribute(LanguagesAttributeName)?.Value;
            if (string.IsNullOrWhiteSpace(langs))
            {
                return new List<string>();
            }

            return langs.Split(';').Select(l => l.Trim()).ToList();
        }

        public void SetLanguages(IEnumerable<string> languages)
        {
            var settingsNode = doc.Root.Element(ns + SettingsElementName);
            if (settingsNode == null)
            {
                settingsNode = new XElement(ns + SettingsElementName);
                doc.Root.Add(settingsNode);
            }

            settingsNode.SetAttributeValue(LanguagesAttributeName, string.Join(";", languages));
        }

        public AddOrUpdateResult AddOrUpdateString(string id, string lang, string text, string state, string comment)
        {
            var validState = StateConverter.ToMuiDB(state);  // may throw ArgumentOutOfRangeException

            var stringsNode = doc.Root.Element(ns + ItemsElementName);
            if (stringsNode == null)
            {
                stringsNode = new XElement(ns + ItemsElementName);
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
                item.SetAttributeValue(IdAttributeName, id);
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
                textNode.SetAttributeValue(LangAttributeName, lang);
                item.Add(textNode);

                // add new language to global language list
                var langs = GetLanguages();
                if (!langs.Contains(lang))
                {
                    langs.Add(lang);
                    SetLanguages(langs);
                }
            }

            textNode.SetValue(text);
            textNode.SetAttributeValue(StateAttributeName, validState);

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
                    ////commentNode.SetAttributeValue(LangName, lang);
                    item.AddFirst(commentNode);
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
            var itemsNode = doc.Root.Element(ns + ItemsElementName);
            var items = itemsNode.Elements(ns + ItemElementName).OrderBy(i => i.Attribute(IdAttributeName)?.Value);
            itemsNode.ReplaceAll(items);
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
            foreach (var item in Items)
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
                throw new ArgumentException($"'{language}' is not a configured language.");
            }

            var entries = new List<ResXEntry>();
            foreach (var item in Items)
            {
                var entry = new ResXEntry();
                TextItem text;
                if (!item.Texts.TryGetValue(language, out text) && !item.Texts.TryGetValue(NeutralLanguage, out text))
                {
                    throw new MissingTranslationsException(new List<string>() { $"{item.Id};{language}" });
                }

                entry.Id = item.Id;
                entry.Value = text.Value;

                if (!options.HasFlag(SaveOptions.SkipComments))
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

            ResXFile.Write(filename, entries);
        }

        public ImportResult ImportResX(string filename, string language)
        {
            var result = new ImportResult();
            var entries = ResXFile.Read(filename);

            foreach (var e in entries)
            {
                var addOrUpdate = AddOrUpdateString(e.Id, language, e.Value, DefaultNewState, e.Comment);

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