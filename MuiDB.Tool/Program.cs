﻿// <copyright file="Program.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Resources;
    using System.Text;
    using System.Threading.Tasks;

    internal class Program
    {
        public static void Info(Args.InfoCommand cmd)
        {
            var muidb = new MuiDBFile(cmd.MuiDB);
            var initialCount = 0;
            var translatedCount = 0;
            var reviewedCount = 0;
            var finalCount = 0;
            var commentCount = 0;
            var itemCount = 0;
            var langs = new HashSet<string>();

            foreach (var i in muidb.Items)
            {
                ++itemCount;
                commentCount += i.Comments.Count();

                foreach (var text in i.Texts)
                {
                    if (!langs.Contains(text.Key))
                    {
                        langs.Add(text.Key);
                    }

                    switch (text.Value.State)
                    {
                        case "translated": ++translatedCount; break;
                        case "initial": ++initialCount; break;
                        case "reviewed": ++reviewedCount; break;
                        case "final": ++finalCount; break;
                        default:
                            Console.Error.WriteLine($"unknown state '{text.Value.State}' for item id={i.Id} and lang={text.Key}");
                            break;
                    }
                }
            }

            Console.WriteLine($"  items total : {itemCount}");
            Console.WriteLine($"  languages   : {langs.Count}");
            Console.WriteLine($"  # initial   : {initialCount}");
            Console.WriteLine($"  # translated: {translatedCount}");
            Console.WriteLine($"  # reviewed  : {reviewedCount}");
            Console.WriteLine($"  # final     : {finalCount}");

            Console.WriteLine("Configured output files:");
            foreach (var file in muidb.TargetFiles)
            {
                Console.WriteLine($" - {file.Name} (lang={file.Lang})");
            }
        }

        public static void ImportFile(Args.ImportFileCommand cmd)
        {
            var muidb = new MuiDBFile(cmd.Muidb, MuiDBFile.OpenMode.CreateIfMissing);

            switch (cmd.Type.ToLowerInvariant())
            {
                case "resx":
                    var result = muidb.ImportResX(cmd.In, cmd.Lang);
                    if (cmd.Verbose)
                    {
                        foreach (var added in result.AddedItems)
                        {
                            Console.WriteLine($"added resource '{added}'");
                        }

                        foreach (var updated in result.UpdatedItems)
                        {
                            Console.WriteLine($"updated resource '{updated}'");
                        }
                    }

                    Console.WriteLine($"added items: {result.AddedItems.Count}\nupdated items: {result.UpdatedItems.Count}");
                    break;

                case "xliff":
                    var doc = new XliffParser.XlfDocument(cmd.In);
                    var file = doc.Files.First();

                    if (cmd.Verbose)
                    {
                        Console.WriteLine($"adding/updating resources for language '{cmd.Lang}...");
                    }

                    foreach (var unit in file.TransUnits)
                    {
                        var id = unit.Id;
                        if (string.Equals(id, "none"))
                        {
                            id = unit.Optional.Resname;
                        }

                        var comment = unit.Optional.Notes.Any() ? unit.Optional.Notes.First().Value : null;
                        if (cmd.Verbose)
                        {
                            Console.WriteLine($"adding/updating resource '{id}': text='{unit.Target}', state='{unit.Optional.TargetState}'");
                        }

                        muidb.AddOrUpdateString(id, cmd.Lang, unit.Target, unit.Optional.TargetState, comment);
                    }

                    break;

                default:
                    throw new Exception($"unknown format: {cmd.Type}");
            }

            muidb.Save();
        }

        public static void Export(Args.ExportCommand cmd)
        {
            string dir = GetFullNormalizedDirectory(cmd.MuiDB);

            foreach (var file in GetMatchingFiles(dir, Path.GetFileName(cmd.MuiDB)))
            {
                if (cmd.Verbose)
                {
                    Console.WriteLine($"exporting from file '{file}'");
                }

                var muidb = new MuiDBFile(file);
                if (!muidb.TargetFiles.Any())
                {
                    throw new InvalidOperationException($"{file} does not contain any files to export");
                }

                foreach (var target in muidb.TargetFiles)
                {
                    var targetFile = Path.Combine(dir, target.Name);
                    if (cmd.Verbose)
                    {
                        Console.WriteLine($"exporting language '{target.Lang}' into file '{targetFile}'");
                    }

                    muidb.ExportResX(targetFile, target.Lang, MuiDBFile.SaveOptions.None);
                }
            }
        }

        public static void ExportFile(Args.ExportFileCommand cmd)
        {
            var muidb = new MuiDBFile(cmd.MuiDB);

            switch (cmd.Type.ToLowerInvariant())
            {
                case "resx":
                    var options = cmd.NoComments ? MuiDBFile.SaveOptions.SkipComments : MuiDBFile.SaveOptions.None;
                    muidb.ExportResX(cmd.Out, cmd.Lang, options);
                    if (cmd.Verbose)
                    {
                        Console.WriteLine($"exporting language '{cmd.Lang}' into file '{cmd.Out}'");
                    }

                    break;

                case "xliff":
                    throw new Exception("xliff export is not implemented, yet");

                default:
                    throw new Exception($"unknown format: {cmd.Type}");
            }
        }

        public static void Verify(Args.ValidateCommand cmd)
        {
            var muidb = new MuiDBFile(cmd.MuiDB);
            muidb.Validate();
            if (cmd.ApplyFormat)
            {
                muidb.Save();
            }
        }

        private static string GetFullNormalizedDirectory(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (dir == null)
            {
                throw new ArgumentException($"filepath contains an invalid directory: {dir}");
            }

            if (string.IsNullOrWhiteSpace(dir) || !Path.IsPathRooted(dir))
            {
                dir = Path.Combine(System.IO.Directory.GetCurrentDirectory(), dir);
            }

            dir = Path.GetFullPath(dir);  // normalize path
            return dir;
        }

        private static List<string> GetMatchingFiles(string dir, string pattern)
        {
            List<string> files = new List<string>();

            if (pattern.Contains("*") || pattern.Contains('?'))
            {
                foreach (var fileInfo in new DirectoryInfo(dir).EnumerateFiles(pattern))
                {
                    files.Add(fileInfo.FullName);
                }

                if (files.Count == 0)
                {
                    throw new ArgumentException("no matching files found to export from");
                }
            }
            else
            {
                files.Add(Path.Combine(dir, pattern));
            }

            return files;
        }

        private static void Main(string[] args)
        {
            try
            {
                var argsParser = new fmdev.ArgsParser.ArgsParser(typeof(Args));
                if (argsParser.Parse(args))
                {
                    if (argsParser.Result is Args.InfoCommand)
                    {
                        Info(argsParser.Result as Args.InfoCommand);
                    }
                    else if (argsParser.Result is Args.ImportFileCommand)
                    {
                        ImportFile(argsParser.Result as Args.ImportFileCommand);
                    }
                    else if (argsParser.Result is Args.ExportCommand)
                    {
                        Export(argsParser.Result as Args.ExportCommand);
                    }
                    else if (argsParser.Result is Args.ExportFileCommand)
                    {
                        ExportFile(argsParser.Result as Args.ExportFileCommand);
                    }
                    else if (argsParser.Result is Args.ValidateCommand)
                    {
                        Verify(argsParser.Result as Args.ValidateCommand);
                    }
                    else if (argsParser.Result is Args.AboutCommand)
                    {
                        argsParser.PrintHeader();
                        About();
                    }
                }
                else
                {
                    System.Environment.ExitCode = 1;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("error: " + e.Message);
                if (e.InnerException != null)
                {
                    Console.Error.WriteLine("Additonal error information: " + e.InnerException.Message);
                }
#if DEBUG

                Console.Error.WriteLine($"+++ Stacktrace +++\n{e.StackTrace}");
#endif
                System.Environment.ExitCode = 1;
            }
        }

        private static void About()
        {
            var app = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Console.WriteLine(
                $"\n{app} uses the following open source 3rdParty libs:\n\n" +
                "- XliffParser: https://github.com/fmuecke/XliffParser (BSD)\n" +
                "- ArgsParser: https://github.com/fmuecke/ArgsParser (BSD)\n\n" +
                "For additional license information please consult the LICENSE file in the respective repository.\n\n" +
                "License:\n" +
                "Copyright (c) 2016, Florian Mücke\n" +
                "All rights reserved.\n" +
                "\n" +
                "Redistribution and use in source and binary forms, with or without\n" +
                "modification, are permitted provided that the following conditions are met:\n" +
                "\n" +
                "* Redistributions of source code must retain the above copyright notice, this\n" +
                "  list of conditions and the following disclaimer.\n" +
                "\n" +
                "* Redistributions in binary form must reproduce the above copyright notice,\n" +
                "  this list of conditions and the following disclaimer in the documentation\n" +
                "  and / or other materials provided with the distribution.\n" +
                "\n" +
                "THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS \"AS IS\"\n" +
                "AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE\n" +
                "IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE\n" +
                "DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE\n" +
                "FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL\n" +
                "DAMAGES(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR\n" +
                "SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER\n" +
                "CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,\n" +
                "OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE\n" +
                "OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.\n\n" +
                "Feel free to contribute! For more information, visit https://github.com/fmuecke/MuiDB.\n");
        }
    }
}