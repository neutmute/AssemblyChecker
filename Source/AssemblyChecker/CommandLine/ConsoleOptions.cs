using System;
using System.Text;
using Common.Logging;
using NDesk.Options;

namespace AssemblyChecker
{

    class ConsoleOptions
    {
        public string AssemblyPattern { get; set; }

        public string Folder { get; set; }

        public bool Recurse { get; set; }

        public bool ShowHelp { get; set; }

        public OptionSet OptionSet { get; private set; }

        public ConsoleOptions(string[] args)
        {
           AssemblyPattern = "*.*";

            OptionSet = new OptionSet {
                { "f|folder=", "Folder to scan", v => { Folder = v; }},
                { "p|pattern=", "Filter by assembly name", v => { AssemblyPattern = v; }},
                { "r|recurse", "Recurse", v => { Recurse= true; }},
                { "h|?:", "Show help", v => ShowHelp = true }
            };

            var unprocessed = OptionSet.Parse(args);
            if (unprocessed.Count == 1)
            {
                Folder = unprocessed[0];
            }
        }

        //private string GetLegalEnumOptions<T>(string friendlyName)
        //{
        //    var allValues = (T[])Enum.GetValues(typeof(T));
        //    var contextHelpText = $"Legal values for {friendlyName} are:" +
        //                        string.Join<T>(",", allValues);

        //    return contextHelpText;
        //}

        //private T ParseEnum<T>(string value) where T : struct, IConvertible
        //{
        //    T output;
        //    if (Enum.TryParse(value, true, out output))
        //    {
        //        return output;
        //    }
        //    var legalValues = GetLegalEnumOptions<T>(typeof(T).Name);
        //    throw new NotSupportedException($"Failed to parse '{value}' as {typeof(T).Name}. {legalValues}");
        //}

        public override string ToString()
        {
            var s = new StringBuilder();

            s.AppendFormat($"Folder={Folder}");

            if (!string.IsNullOrEmpty(AssemblyPattern))
            {
                s.AppendFormat($", Assembly={AssemblyPattern}");
            }

            if (Recurse)
            {
                s.AppendFormat($", Recurse={Recurse}");
            }

            return s.ToString();
        }

    }
}
