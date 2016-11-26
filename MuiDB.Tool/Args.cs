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
        private const string HelpTextVerbose = "Print verbose output.";

        [Description("Display information for a MuiDB file.")]
        public class InfoCommand : Command
        {
            [CommandArg(HelpText = "The MuiDB file to read.", IsRequired = true)]
            public string MuiDB { get; set; }
        }

        [Description("Import translations from specific ResX/XLIFF file.")]
        public class ImportFileCommand : Command
        {
            [CommandArg(HelpText = "The file to import from.", IsRequired = true)]
            public string In { get; set; }

            [CommandArg(HelpText = "The input file type - can be resx or xliff.", IsRequired = true)]
            public string Type { get; set; }

            [CommandArg(HelpText = "The target language that should be written.", IsRequired = true)]
            public string Lang { get; set; }

            [CommandArg(HelpText = "The MuiDB file that should be imported to.", IsRequired = true)]
            public string Muidb { get; set; }

            ////[CommandArg(HelpText = "update existing entries during import")]
            ////public bool Update { get; set; }

            [CommandArg(HelpText = HelpTextVerbose)]
            public bool Verbose { get; set; }
        }

        [Description("Export translations for configured resx files.")]
        public class ExportCommand : Command
        {
            [CommandArg(HelpText = "The MuiDB file to process. (supports wildcards)", IsRequired = true)]
            public string MuiDB { get; set; }

            [CommandArg(HelpText = "Generate *.Designer.cs file from configured resx files")]
            public bool GenerateDesignerFiles { get; set; }

            [CommandArg(HelpText = HelpTextVerbose)]
            public bool Verbose { get; set; }
        }

        [Description("Export translations to a specific ResX/XLIFF file.")]
        public class ExportFileCommand : Command
        {
            [CommandArg(HelpText = "The MuiDB file to export from.", IsRequired = true)]
            public string MuiDB { get; set; }

            [CommandArg(HelpText = "The language that should be exported.", IsRequired = true)]
            public string Lang { get; set; }

            [CommandArg(HelpText = "The resx output file.", IsRequired = true)]
            public string Out { get; set; }

            [CommandArg(HelpText = "The input file type - can be resx or xliff.", IsRequired = true)]
            public string Type { get; set; }

            [CommandArg(HelpText = "Do not include comments in output file.")]
            public bool NoComments { get; set; }

            [CommandArg(HelpText = HelpTextVerbose)]
            public bool Verbose { get; set; }
        }

        [Description("Validate MuiDB schema and data (optionally apply format).")]
        public class ValidateCommand : Command
        {
            [CommandArg(HelpText = "The MuiDB file to validate", IsRequired = true)]
            public string MuiDB { get; set; }

            [CommandArg(HelpText = "Apply default format to MuiDB file (will fix item sort order).")]
            public bool ApplyFormat { get; set; }
        }

        [Description("Display version and license information.")]
        public class AboutCommand : Command
        {
        }
    }
}
