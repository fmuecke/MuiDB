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

        public enum OpenMode
        {
            OpenExisting,
            CreateIfMissing
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
                        if (lang == null)
                        {
                            throw new Exception($"Item '{item.Id}' does not have a 'lang' attribute in the text element.");
                        }
                        var state = t.Attribute(ns + StateName)?.Value;
                        item.Texts[lang] = new Text() { State = state, Value = t.Value };
                    }

                    foreach (var c in i.Elements(ns + CommentName))
                    {
                        var lang = c.Attribute(ns + LangName)?.Value;
                        lang = lang ?? NeutralLanguage;
                        item.Comments[lang] = c.Value;
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

        public void AddOrUpdateTranslation(string id, string lang, string text, string state, string comment)
        {
            var translationsNode = doc.Root.Element(ns + TranslationsName);
            if (translationsNode == null)
            {
                translationsNode = new XElement(ns + TranslationsName);
                doc.Root.Add(translationsNode);
            }

            var items = translationsNode.Elements(ns + ItemName).Where(i => i.Attribute(ns + IdName)?.Value == id);
            XElement item;
            if (items.Any())
            {
                item = items.First();
            }
            else
            {
                item = new XElement(ns + ItemName);
                item.SetAttributeValue(ns + IdName, id);
                translationsNode.Add(item);
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
                var commentNodes = item.Elements(ns + CommentName).Where(c => c.Attribute(ns + LangName)?.Value == lang);
                XElement commentNode;
                if (commentNodes.Any())
                {
                    commentNode = commentNodes.First();
                }
                else
                {
                    commentNode = new XElement(ns + CommentName);
                    commentNode.SetAttributeValue(ns + LangName, lang);
                    item.Add(commentNode);
                }

                commentNode.SetValue(comment);
            }
        }

        public void Save()
        {
            var translationsNode = doc.Root.Element(ns + TranslationsName);
            var items = translationsNode.Elements(ns + ItemName).OrderBy(i => i.Attribute(ns + IdName).Value);
            translationsNode.ReplaceAll(items);
            doc.Save(Filename);
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

        public void SaveAsResX(string fileName, string language)
        {
            SaveAsResX(fileName, language, new ResXSaveMode());
        }

        public void SaveAsResX(string fileName, string language, ResXSaveMode mode)
        {
            using (var resx = new ResXResourceWriter(fileName))
            {
                var nodes = new List<ResXDataNode>();
                foreach (var i in Translations)
                {
                    var text = i.Texts[language].Value.Replace("\n", Environment.NewLine);
                    var node = new ResXDataNode(i.Id, text);

                    if (mode.DoIncludeComments)
                    {
                        string comment;
                        if (i.Comments.TryGetValue(language, out comment))
                        {
                            node.Comment = comment.Replace("\n", Environment.NewLine);
                        }
                    }

                    nodes.Add(node);
                }

                if (mode.DoSort)
                {
                    nodes.Sort((x, y) =>
                    {
                        if (x.Name == null && y.Name == null)
                        {
                            return 0;
                        }
                        else if (x.Name == null)
                        {
                            return -1;
                        }
                        else if (y.Name == null)
                        {
                            return 1;
                        }
                        else
                        {
                            return x.Name.CompareTo(y.Name);
                        }
                    });
                }

                foreach (var node in nodes)
                {
                    resx.AddResource(node);
                }
            }
        }

        public class ResXSaveMode
        {
            public ResXSaveMode()
            {
            }

            public bool DoSort { get; set; } = false;

            public bool DoIncludeComments { get; set; } = false;
        }
    }
}