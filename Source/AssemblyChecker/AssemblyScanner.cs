using Kraken.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyChecker
{
    public class AssemblyEntry
    {
        public FileInfo FileInfo { get; set; }

        public Assembly Assembly { get; set; }
        
        public AssemblyEntry()
        {

        }

        public AssemblyEntry(string file)
        {
            FileInfo = new FileInfo(file);
        }

        public override int GetHashCode()
        {
            return FileInfo.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as AssemblyEntry;
            return other != null && other.FileInfo.Equals(FileInfo);
        }
    }
  
    public class AssemblyReferenceEntry
    {
        public AssemblyName AssemblyName { get; set; }

        public List<FileInfo> ReferencedBy { get; set; }

        public override int GetHashCode()
        {
            return AssemblyName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var objAsThis = obj as AssemblyReferenceEntry;
            return objAsThis != null && objAsThis.AssemblyName.Equals(AssemblyName);
        }
    }
    

    class AssemblyScanner
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public AssemblyScanner()
        {
        }

        public List<AssemblyEntry> Scan(ConsoleOptions options)
        {
            string referencedAssemblyPath = options.Folder;

            var assemblyExtensions = new List<string> { ".exe", ".dll" };
            var searchOption = options.Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var assemblyEntries = Directory
                                .GetFiles(referencedAssemblyPath, options.AssemblyPattern, searchOption)
                                .Select(f => new AssemblyEntry(f))
                                .Where(ae => assemblyExtensions.Contains(ae.FileInfo.Extension.ToLower()))
                                .Where(ae => string.IsNullOrEmpty(options.IncludeFilter) || ae.FileInfo.FullName.ToLower().Contains(options.IncludeFilter))
                                .ToList();

            foreach (var assemblyEntry in assemblyEntries)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(assemblyEntry.FileInfo.FullName);
                    assemblyEntry.Assembly = assembly;

                    assembly.GetReferencedAssemblies();
                }
                catch (Exception ex)
                {
                    Log.Info(ex, "Failed to load " + assemblyEntry.FileInfo.FullName);
                }
            }

            assemblyEntries.RemoveAll(ae => ae.Assembly == null);
            assemblyEntries.Sort((x, y) => x.Assembly.FullName.CompareTo(y.Assembly.FullName));
            
            return assemblyEntries;
        }

        public List<AssemblyReferenceEntry> GetReferences(List<AssemblyEntry> entries)
        {
            var references = new Dictionary<string, List<FileInfo>>();
            var assemblyNames = new Dictionary<string, AssemblyName>();
            foreach (var entry in entries)
            {
                var names = entry.Assembly.GetReferencedAssemblies();
                foreach(var refAssembly in names)
                {
                    var assemblyName = refAssembly.FullName;
                    if (!references.Keys.Contains(assemblyName))
                    {
                        references.Add(assemblyName, new List<FileInfo>());
                        assemblyNames.Add(assemblyName, refAssembly);
                    }
                    references[assemblyName].Add(entry.FileInfo);
                }
            }

            var refList = new List<AssemblyReferenceEntry>();
            foreach (var key in references.Keys)
            {
                var refEntry = new AssemblyReferenceEntry();
                refEntry.AssemblyName = assemblyNames[key];
                refEntry.ReferencedBy = references[key];
                refList.Add(refEntry);
            }
            refList.Sort((x, y) => x.AssemblyName.FullName.CompareTo(y.AssemblyName.FullName));
            return refList;
        }

        public void Dump(List<AssemblyEntry> entries)
        {
            var objectDumper = new ObjectDumper<AssemblyEntry>(GetAssemblyDump);
            Log.Info(objectDumper.Dump(entries));
        }
        
        private static ObjectDump GetAssemblyDump(AssemblyEntry assemblyEntry)
        {
            var dump = new ObjectDump();
            var assembly = assemblyEntry.Assembly;

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
                try
                {
                    productAttribute = (AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Failed to enumerate AssemblyProductAttribute for {assemblyEntry.FileInfo.FullName}");
                }

                var configAttribute = (AssemblyConfigurationAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyConfigurationAttribute));

                dump.Data.Add(assembly.GetName().Name);
                dump.Data.Add(productAttribute == null ? string.Empty : productAttribute.Product);
                dump.Data.Add(assembly.GetName().Version.ToString());
                dump.Data.Add(FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion);
                dump.Data.Add(FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion);
                dump.Data.Add(configAttribute == null ? string.Empty : configAttribute.Configuration);
                dump.Data.Add(assembly.ImageRuntimeVersion);
                dump.Data.Add(assemblyEntry.FileInfo.Directory.FullName);
            }
            catch(Exception eOuter)
            {

                Log.Error(eOuter, $"Failed to dump for {assemblyEntry.FileInfo.FullName}");
            }


            return dump;
        }
    }
}
