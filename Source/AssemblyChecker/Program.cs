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

            Log.Info("Finished");
        }

    }
}
