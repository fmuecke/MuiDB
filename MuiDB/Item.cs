// <copyright file="Item.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB
{
    using System.Collections.Generic;

    public class Item
    {
        public Dictionary<string, TextItem> Texts { get; private set; } = new Dictionary<string, TextItem>();

        // <lang, comment>
        public Dictionary<string, string> Comments { get; private set; } = new Dictionary<string, string>();

        public string Id { get; set; } = string.Empty;
    }
}