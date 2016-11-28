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
        private const string ReFormatHelpText = "Apply default format to MuiDB file (fixes item sort order).";
        private const string CodeNamespaceHelpText = "The code namespace for the generated designer files.";

        [Description("Display information for a MuiDB file.")]
        public class InfoCommand : VerboseCommand
        {
            [CommandArg(HelpText = "The MuiDB file to read.", IsRequired = true)]
            public string MuiDB { get; set; }
        }

        [Description("Configure settings for a MuiDB file.")]
        public class ConfigureCommand : VerboseCommand
        {
            [CommandArg(HelpText = "The MuiDB file to apply changes to.", IsRequired = true)]
            public string MuiDB { get; set; }

            [CommandArg(HelpText = "The base name for the generated files.")]
            public string BaseName { get; set; }

            [CommandArg(HelpText = CodeNamespaceHelpText)]
            public string CodeNamespace { get; set; }

            [CommandArg(HelpText = "The project title.")]
            public string ProjectTitle { get; set; }
        }

        [Description("Import translations from specific ResX/XLIFF file.")]
        public class ImportFileCommand : VerboseCommand
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
        }

        [Description("Export translations for configured resx files. Performs validation prior to export.")]
        public class ExportCommand : VerboseCommand
        {
            [CommandArg(HelpText = "The MuiDB file to process. (supports wildcards)", IsRequired = true)]
            public string MuiDB { get; set; }

            [CommandArg(HelpText = ReFormatHelpText)]
            public bool ReFormat { get; set; }

            [CommandArg(HelpText = CodeNamespaceHelpText)]
            public string CodeNamespace { get; set; }
        }

        [Description("Export translations to a specific ResX/XLIFF file.")]
        public class ExportFileCommand : VerboseCommand
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
        }

        [Description("Validate MuiDB schema and data (optionally apply format).")]
        public class ValidateCommand : VerboseCommand
        {
            [CommandArg(HelpText = "The MuiDB file to validate", IsRequired = true)]
            public string MuiDB { get; set; }

            [CommandArg(HelpText = ReFormatHelpText)]
            public bool ReFormat { get; set; }
        }

        [Description("Display version and license information.")]
        public class AboutCommand : Command
        {
        }
    }
}