// <copyright file="VerboseCommand.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using fmdev.ArgsParser;

    internal abstract class VerboseCommand : Command
    {
        [CommandArg(HelpText = "Print verbose output.")]
        public bool Verbose { get; set; }
    }
}