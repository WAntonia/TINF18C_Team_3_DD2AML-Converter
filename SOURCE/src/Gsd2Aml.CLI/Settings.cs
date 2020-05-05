/*
 *  Copyright (C) 2019 GSD2AML Team (Nico Dietz, Steffen Gerdes, Constantin Ruhdorfer,
 *  Jonas Komarek, Phuc Quang Vu, Michael Weidmann)
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation version 3 of the License.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Gsd2Aml.Lib.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gsd2Aml.Cli
{
    /// <summary>
    /// Stores the settings specified by the user.
    /// </summary>
    internal class Settings
    {
        internal const string CHelp = "--help";
        internal const string CHelpShort = "-h";

        private const string CInputFile = "--input";
        private const string CInputFileShort = "-i";

        private const string COutputFile = "--output";
        private const string COutputFileShort = "-o";

        private const string CStringOutput = "--string";
        private const string CStringOutputShort = "-s";

        private const string CValidation = "--novalidate";
        private const string CValidationShort = "-n";

        private const string CCAEXVersion = "--version";
        private const string CCAEXVersionShort = "-v";

        private static string[] Arguments { get; } = { CHelp, CHelpShort, CInputFile, CInputFileShort, COutputFile, COutputFileShort, CStringOutput, CStringOutputShort, CValidation, CValidationShort, CCAEXVersion, CCAEXVersionShort };

        internal string InputFile { get; set; }

        internal string OutputFile { get; set; }

        internal bool StringOutput { get; set; }

        internal bool Validation { get; set; } = true;

        internal string CAEXVersion { get; set; }

        internal List<string> Args { get; set; }

        /// <summary>
        /// Empty constructor for testing purposes.
        /// </summary>
        internal Settings() { }

        /// <summary>
        /// Constructor for Settings.
        /// </summary>
        /// <param name="args">Arguments which were passed to the program.</param>
        internal Settings(List<string> args)
        {
            Args = args;
            CheckCliArguments();
            ParseCliArguments();
            CheckGsdExistence();
        }

        /// <summary>
        /// This method checks three things:
        /// 1) If the user passed the corresponding long/short argument to an argument multiple times. E.g. gsd2aml -i --input
        /// 2) If the user passed the same argument multiple times . E.g. gsd2aml -i -i
        /// 3) If the user passed --output and --string at the same time.
        /// If one of the above happens, an exception will be thrown.
        /// </summary>
        /// <exception cref="ArgumentException">The argument list is invalid.</exception>
        internal void CheckCliArguments()
        {
            // Check for 1)
            for (var i = 0; i < Arguments.Length - 1; i += 2)
            {
                if (Args.IndexOf(Arguments[i]) < 0 || Args.IndexOf(Arguments[i + 1]) < 0) continue;

                Util.Logger.Log(LogLevel.Error, $"User passed {Arguments[i]} and {Arguments[i + 1]} but only of them is allowed.");
                throw new ArgumentException($"{Environment.NewLine}Error: You used {Arguments[i]} and {Arguments[i + 1]} while only one of them is allowed." +
                                            $"{Environment.NewLine}For more information run 'dd2aml --help'.");
            }

            // Check for 2)
            if (Args.Count != Args.Distinct().Count())
            {
                PrintMultipleParameterError();
            }

            // Check for 3)
            if (Args.IndexOf(COutputFile) < 0 && Args.IndexOf(COutputFileShort) < 0 ||
                Args.IndexOf(CStringOutput) < 0 && Args.IndexOf(CStringOutputShort) < 0) return;

            Util.Logger.Log(LogLevel.Error, "User passed -o/--output and -s/--string at the same time.");
            throw new ArgumentException($"{Environment.NewLine}Error: You used {COutputFile} and {CStringOutput} while only one of them is allowed." +
                                        $"{Environment.NewLine}For more information run 'dd2aml --help'.");
        }

        /// <summary>
        /// This method parses the CLI Arguments and maps them to the settings properties.
        /// </summary>
        internal void ParseCliArguments()
        {
            var parameter = new Dictionary<string, string>
            {
                { CInputFile, null },
                { CInputFileShort, null },
                { COutputFile, null },
                { COutputFileShort, null}
            };

            for (var i = 0; i < Args.Count; i++)
            {
                if (!parameter.ContainsKey(Args[i])) continue;

                if (i + 1 < Args.Count)
                {
                    parameter[Args[i]] = Args[i + 1];
                }
            }

            InputFile = parameter[CInputFileShort] ?? parameter[CInputFile];

            if (Regex.IsMatch(InputFile, $"(.+(GSDML|gsdml)-.+{Regex.Escape(".xml")})"))
            {
                Lib.Util.filetype = 1;
            }
            if (Regex.IsMatch(InputFile, $"(.+.-(IODD|iodd).+{Regex.Escape(".xml")})"))
            {
                Lib.Util.filetype = 2;
            }
            if (Regex.IsMatch(InputFile, $"(.+{Regex.Escape(".cspp")})"))
            {
                Lib.Util.filetype = 3;
            }

            OutputFile = parameter[COutputFileShort] ?? parameter[COutputFile];

            StringOutput = Args.FindIndex(arg => arg.Equals(CStringOutputShort)) >= 0 ||
                           Args.FindIndex(arg => arg.Equals(CStringOutput)) >= 0;
            if (!Args.Contains(CValidationShort) && !Args.Contains(CValidation)) return;
            Console.WriteLine("Warning: The DD file validation was turned off.");
            Util.Logger.Log(LogLevel.Warning, "DD file validation was turned off.");
            Validation = false;
        }

        /// <summary>
        /// Checks for the existence of the DD file.
        /// </summary>
        /// <exception cref="FileNotFoundException">The DD file could not be found.</exception>
        private void CheckGsdExistence()
        {
            if (File.Exists(InputFile))
            {
                Util.Logger.Log(LogLevel.Info, $"Input file exists: {InputFile}");
            }
            else if (File.Exists(Args[0]))
            {
                Util.Logger.Log(LogLevel.Info, $"Input file exists: {Args[0]}");
                InputFile = Args[0];
            }
            else
            {
                Util.Logger.Log(LogLevel.Error, "Input file does not exist.");
                throw new FileNotFoundException($"{Environment.NewLine}Error: Input file not found. Please enter a valid path to a DD-formatted file." +
                                                $"{Environment.NewLine}For more information run 'dd2aml --help'.");
            }
        }

        /*
      if (Regex.IsMatch(InputFile, $"(.+(GSDML|gsdml)-.+{Regex.Escape(".xml")})"))
    {
        var diretoryName = System.IO.Path.GetDirectoryName(InputFile) ?? "";
        var fileName = System.IO.Path.GetFileNameWithoutExtension(InputFile).Remove(0, "GSDML-".Length) + ".amlx";

        TxtAmlFile.Text = System.IO.Path.Combine(diretoryName, fileName);
        Lib.Util.filetype =  1;
    }

    //IODD
    if (Regex.IsMatch(InputFile, $"(.+.-(IODD|iodd).+{Regex.Escape(".xml")})"))
    {
        var diretoryName = System.IO.Path.GetDirectoryName(InputFile) ?? "";
        var len = senderText.Length - 13 - diretoryName.Length;
        var fileName = System.IO.Path.GetFileNameWithoutExtension(InputFile).Remove(len, 8) + ".amlx";

        TxtAmlFile.Text = System.IO.Path.Combine(diretoryName, fileName);
        Lib.Util.filetype = 2;
    }

    //CSP+
    if (Regex.IsMatch(InputFile, $"(.+{Regex.Escape(".cspp")})"))
    {
        var diretoryName = System.IO.Path.GetDirectoryName(InputFile) ?? "";
        var fileName = System.IO.Path.GetFileNameWithoutExtension(InputFile) + ".amlx";

        TxtAmlFile.Text = System.IO.Path.Combine(diretoryName, fileName);
        Lib.Util.filetype = 3;
    }*/

        /// <summary>
        /// Prints an error message if the same argument was used multiple times.
        /// </summary>
        /// <exception cref="ArgumentException">An argument was used multiple times.</exception>
        internal void PrintMultipleParameterError()
        {
            var iteratedArguments = new HashSet<string>();

            foreach (var argument in Args)
            {
                if (!Arguments.Contains(argument)) continue;
                if (!iteratedArguments.Contains(argument))
                {
                    iteratedArguments.Add(argument);
                }
                else
                {
                    Util.Logger.Log(LogLevel.Error, $"User passed {argument} multiple times.");
                    throw new ArgumentException($"{Environment.NewLine}Error: You used {argument} multiple times." +
                                                $"{Environment.NewLine}For more information run 'dd2aml --help'.");
                }
            }
        }
    }
}
