// <copyright file="ImportResult.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB
{
    using System.Collections.Generic;

    public class ImportResult
    {
        public List<string> AddedItems { get; internal set; } = new List<string>();

        public List<string> UpdatedItems { get; internal set; } = new List<string>();
    }
}