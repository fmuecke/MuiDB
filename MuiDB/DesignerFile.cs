// <copyright file="DesignerFile.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB
{
    public class DesignerFile
    {
        public string ClassName { get; set; }

        public string Namespace { get; set; }

        public bool IsInternal { get; set; }
    }
}