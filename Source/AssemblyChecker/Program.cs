using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using Kraken.Core;
using NLog;

namespace AssemblyChecker
{
    /// <summary>
    /// Given a folder, enumerates the list of assemblies and their versions and CLR compilation target
    /// </summary>
    class Program
    {
        #region Fields
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        #endregion

        static void Main(string[] args)
        {
            var options = new ConsoleOptions(args);

            if (options.ShowHelp)
            {
                Console.WriteLine("Options:");
                options.OptionSet.WriteOptionDescriptions(Console.Out);
                return;
            }

            Log.Info(options.ToString());

            var scanner = new AssemblyScanner();
            var assemblyEntries = scanner.Scan(options);
            Log.Info($"Found {assemblyEntries.Count} assembly files in {options.Folder}");

            scanner.Dump(assemblyEntries);

            if (!string.IsNullOrEmpty(options.ReferenceFilter))
            {
                var references = scanner
                                    .GetReferences(assemblyEntries)
                                    .Where(r => r.AssemblyName.FullName.Contains(options.ReferenceFilter))
                                    .ToList();

                var referenceDump = new StringBuilder();
                foreach (var reference in references)
                {
                    referenceDump.AppendLine($"# {reference.AssemblyName}");
                    var files = reference.ReferencedBy.Select(f => f.FullName).ToCsv("\r\n", f => f);
                    referenceDump.AppendLine($"{files}\r\n");
                }

                Log.Info($"Reference dump\r\n{referenceDump}");
            }

            Log.Info("Finished");
        }

    }
}
