// <copyright file="Args.cs" company="Florian Mücke">
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

    internal static class Args
    {
        public static List<Command> Commands()
        {
            return typeof(Args).GetNestedTypes().Select(t => (Command)Activator.CreateInstance(t)).ToList();
        }

        [System.ComponentModel.Description("import translations from ResX/XLIFF file")]
        public class ImportCommand : Command
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
        }

        [System.ComponentModel.Description("export translations to ResX/XLIFF file")]
        public class ExportCommand : Command
        {
            [CommandArg(HelpText = "the MuiDB file", IsRequired = true)]
            public string Muidb { get; set; }

            [CommandArg(HelpText = "the language that should be exported", IsRequired = true)]
            public string Lang { get; set; }

            [CommandArg(HelpText = "the resx output file", IsRequired = true)]
            public string Out { get; set; }

            [CommandArg(HelpText = "the input file format - can be resx or xliff", IsRequired = true)]
            public string Format { get; set; }

            [CommandArg(HelpText = "do not include comments in output file")]
            public bool NoComments { get; set; }
        }

        [System.ComponentModel.Description("apply default format to MuiDB file (will sort items as well)")]
        public class FormatCommand : Command
        {
            [CommandArg(HelpText = "the MuiDB file", IsRequired = true)]
            public string Muidb { get; set; }
        }

        [System.ComponentModel.Description("display statistical information")]
        public class InfoCommand : Command
        {
            [CommandArg(HelpText = "the MuiDB file", IsRequired = true)]
            public string Muidb { get; set; }
        }

        [System.ComponentModel.Description("version and license information")]
        public class AboutCommand : Command
        {
        }
    }
}