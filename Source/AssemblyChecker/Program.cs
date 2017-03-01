﻿using System;
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
            string referencedAssemblyPath = args[0];

            string[] files = Directory.GetFiles(referencedAssemblyPath);

            Log.Info("Found {0} files to enumerate in {1}", files.Length, referencedAssemblyPath);

            var assemblyExtensions = new List<string> { ".exe", ".dll" };
            var assemblies = new List<Assembly>();
            
            foreach (string file in files)
            {
                var fileInfo = new FileInfo(file);
                if (assemblyExtensions.Contains(fileInfo.Extension))
                {
                    try
                    {
                        assemblies.Add(Assembly.LoadFrom(file));
                    }
                    catch (Exception ex)
                    {
                        Log.Info(ex, "Failed to load " + fileInfo.Name);
                    }
                }
                else
                {
                    Log.Debug("Ignoring {0}", fileInfo.Name);
                }
            }

            Log.Info("Found {0} assemblies. Proceeding to dump:", assemblies.Count);

            assemblies.Sort((x, y) => x.FullName.CompareTo(y.FullName));

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
            
            return dump;
        }
    }
}
