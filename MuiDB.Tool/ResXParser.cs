// <copyright file="ResXParser.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Linq;
    using System.Resources;
    using System.Text;
    using System.Threading.Tasks;

    public static class ResXParser
    {
        public static List<ResXEntry> Read(string filename)
        {
            var result = new List<ResXEntry>();
            using (var resx = new ResXResourceReader(filename))
            {
                resx.UseResXDataNodes = true;
                var dict = resx.GetEnumerator();
                while (dict.MoveNext())
                {
                    var node = dict.Value as ResXDataNode;
                    result.Add(new ResXEntry()
                    {
                        Id = dict.Key as string,
                        Value = (node.GetValue((ITypeResolutionService)null) as string).Replace("\r", string.Empty),
                        Comment = node.Comment.Replace("\r", string.Empty)
                    });
                }
            }

            return result;
        }

        public static void Write(string filename, List<ResXEntry> entries)
        {
            using (var resx = new ResXResourceWriter(filename))
            {
                foreach (var entry in entries)
                {
                    var node = new ResXDataNode(entry.Id, entry.Value.Replace("\n", Environment.NewLine));

                    if (!string.IsNullOrWhiteSpace(entry.Comment))
                    {
                        node.Comment = entry.Comment.Replace("\n", Environment.NewLine);
                    }

                    resx.AddResource(node);
                }
            }
        }
    }
}