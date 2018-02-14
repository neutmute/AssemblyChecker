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


            string referencedAssemblyPath = args[0];

            var assemblyExtensions = new List<string> { ".exe", ".dll" };
            var searchOption = options.Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory
                                .GetFiles(referencedAssemblyPath, options.AssemblyPattern, searchOption)
                                .Select(f => new FileInfo(f))
                                .Where(f => assemblyExtensions.Contains(f.Extension.ToLower()))
                                .ToList();


            var assemblies = new List<Assembly>();
            
            Log.Info($"Found {files.Count} assembly files to enumerate in {referencedAssemblyPath}");

            foreach (var fileInfo in files)
            {
                try
                {
                    assemblies.Add(Assembly.LoadFrom(fileInfo.FullName));
                }
                catch (Exception ex)
                {
                    Log.Info(ex, "Failed to load " + fileInfo.Name);
                }
            }

            assemblies.Sort((x, y) => x.FullName.CompareTo(y.FullName));

            Log.Info("Found {0} assemblies. Proceeding to dump:", assemblies.Count);
            
            var objectDumper = new ObjectDumper<Assembly>(GetAssemblyDump);
            Log.Info(objectDumper.Dump(assemblies));

            Log.Info("Finished");
        }

        public static ObjectDump GetAssemblyDump(Assembly assembly)
        {
            var dump = new ObjectDump();

            dump.Headers.Add("Name");
            dump.Headers.Add("Product");
            dump.Headers.Add("Version");
            dump.Headers.Add("File Version");
            dump.Headers.Add("Informational Version");
            dump.Headers.Add("Configuration");
            dump.Headers.Add("CLR");
            dump.Headers.Add("Path");

            AssemblyProductAttribute productAttribute = null;
            try
            {
                productAttribute = (AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to enumerate AssemblyProductAttribute");
            }
            
            var configAttribute = (AssemblyConfigurationAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyConfigurationAttribute));

            dump.Data.Add(assembly.GetName().Name);
            dump.Data.Add(productAttribute == null ? string.Empty : productAttribute.Product);
            dump.Data.Add(assembly.GetName().Version.ToString());
            dump.Data.Add(FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion);
            dump.Data.Add(FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion);
            dump.Data.Add(configAttribute == null ? string.Empty : configAttribute.Configuration);
            dump.Data.Add(assembly.ImageRuntimeVersion);
            dump.Data.Add(Path.GetDirectoryName(assembly.Location));


            return dump;
        }
    }
}
