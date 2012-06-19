using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using Kraken.Framework.Core;
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
            string referencedAssemblyPath = args[0];

            string[] files = Directory.GetFiles(referencedAssemblyPath);

            Log.Info("Found {0} files to enumerate in {1}", files.Length, referencedAssemblyPath);

            List<string> assemblyExtensions = new List<string> { ".exe", ".dll" };
            List<Assembly> assemblies = new List<Assembly>();
            
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                if (assemblyExtensions.Contains(fileInfo.Extension))
                {
                    try
                    {
                        assemblies.Add(Assembly.LoadFrom(file));
                    }
                    catch (Exception ex)
                    {
                        Log.InfoException("Failed to load " + fileInfo.Name, ex);
                        continue;
                    }
                }
                else
                {
                    Log.Debug("Ignoring {0}", fileInfo.Name);
                }
            }

            Log.Info("Found {0} assemblies. Proceeding to dump:", assemblies.Count);

            assemblies.Sort((x, y) => x.FullName.CompareTo(y.FullName));

            ObjectDumper<Assembly> objectDumper = new ObjectDumper<Assembly>(GetAssemblyDump);
            Log.Info(objectDumper.Dump(assemblies));

            Log.Info("Finished");
        }

        public static ObjectDump GetAssemblyDump(Assembly assembly)
        {
            ObjectDump dump = new ObjectDump();

            dump.Headers.Add("Name");
            dump.Headers.Add("Product");
            dump.Headers.Add("Version");
            dump.Headers.Add("FileVersion");
            dump.Headers.Add("CLR");

            AssemblyProductAttribute product = (AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));

            dump.Data.Add(assembly.GetName().Name);
            dump.Data.Add(product == null ? string.Empty : product.Product);
            dump.Data.Add(assembly.GetName().Version.ToString());
            dump.Data.Add(FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion);
            dump.Data.Add(assembly.ImageRuntimeVersion);

            return dump;
        }
    }
}
