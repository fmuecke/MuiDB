// <copyright file="Args.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using fmdev.ArgsParser;

    internal static class Args
    {

        [Description("display information on MuiDB file")]
        public class InfoCommand : Command
        {
            [CommandArg(HelpText = "the MuiDB file", IsRequired = true)]
            public string MuiDB { get; set; }
        }

        [Description("import translations from specific ResX/XLIFF file")]
        public class ImportFileCommand : Command
        {
            [CommandArg(HelpText = "the file to import from", IsRequired = true)]
            public string In { get; set; }

            [CommandArg(HelpText = "the input file format - can be resx or xliff", IsRequired = true)]
            public string Format { get; set; }

            [CommandArg(HelpText = "the target language that should be written", IsRequired = true)]
            public string Lang { get; set; }

            [CommandArg(HelpText = "the MuiDB file", IsRequired = true)]
            public string Muidb { get; set; }

            ////[CommandArg(HelpText = "update existing entries during import")]
            ////public bool Update { get; set; }

            [CommandArg(HelpText = "print verbose output")]
            public bool Verbose { get; set; }
        }

        [Description("export translations for configured resx files")]
        public class ExportCommand : Command
        {
            [CommandArg(HelpText = "the MuiDB file to process", IsRequired = true)]
            public string MuiDB { get; set; }

            [CommandArg(HelpText = "print verbose output")]
            public bool Verbose { get; set; }
        }

        [Description("export translations to specific ResX/XLIFF file")]
        public class ExportFileCommand : Command
        {
            [CommandArg(HelpText = "the MuiDB file", IsRequired = true)]
            public string MuiDB { get; set; }

            [CommandArg(HelpText = "the language that should be exported", IsRequired = true)]
            public string Lang { get; set; }

            [CommandArg(HelpText = "the resx output file", IsRequired = true)]
            public string Out { get; set; }

            [CommandArg(HelpText = "the input file format - can be resx or xliff", IsRequired = true)]
            public string Format { get; set; }

            [CommandArg(HelpText = "do not include comments in output file")]
            public bool NoComments { get; set; }

            [CommandArg(HelpText = "print verbose output")]
            public bool Verbose { get; set; }
        }

        [Description("apply default format to MuiDB file (sorts items)")]
        public class FormatCommand : Command
        {
            [CommandArg(HelpText = "the MuiDB file", IsRequired = true)]
            public string MuiDB { get; set; }
        }

        [Description("display version and license information")]
        public class AboutCommand : Command
        {
        }
    }
}