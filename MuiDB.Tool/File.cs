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

    internal class File
    {
        private const string TranslationsName = "translations";
        private const string ItemName = "item";
        private const string IdName = "id";
        private const string CommentName = "comment";
        private const string TextName = "text";
        private const string LangName = "lang";
        private const string StateName = "state";
        private XDocument doc;
        private XNamespace ns;

        public File(string filename)
        {
            Filename = filename;
            doc = XDocument.Load(Filename);
            ns = doc.Root.Name.Namespace;
        }

        public string Filename { get; private set; }

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
                        var state = t.Attribute(ns + StateName)?.Value;
                        item.Texts[lang] = new Text() { State = state, Value = t.Value };
                    }

                    foreach (var c in i.Elements(ns + CommentName))
                    {
                        var lang = c.Attribute(ns + LangName)?.Value;
                        item.Comments[lang] = c.Value;
                    }

                    return item;
                });
            }
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