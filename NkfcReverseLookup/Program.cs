
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NkfcReverseLookup
{
    [HelpOption("-?|-h|--help")]
    class Program
    {
        [Argument(0, Description = "UnicodeDataFile.txt")]
        public string UnicodeDataFile { get; }
        
        static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private void OnExecute(CommandLineApplication app)
        {
            if(string.IsNullOrEmpty(UnicodeDataFile))
            {
                app.ShowHelp();
                return;
            }

            Console.WriteLine("{");
            var (cplist,data) = Load(UnicodeDataFile);
            Stack<uint> st = new Stack<uint>();
            HashSet<uint> hash = new HashSet<uint>();
            foreach (var target in cplist)
            {
                hash.Clear();
                st.Push(target);
                while(st.TryPop(out uint val))
                {
                    foreach (var d in data)
                    {
                        if ( Array.IndexOf(d.Value,val) >= 0)
                        {
                            if (hash.Add(d.Key)) st.Push(d.Key);
                        }
                    }
                }
                if(hash.Count != 0)
                    Console.WriteLine($"\"{target}\":[{string.Join(",",hash)}],");
            }
            Console.WriteLine("\"0\":[]}");
        }


        private static uint ParseHex(string s)
        {
            return uint.Parse(s, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        public static (IList<uint>,Dictionary<uint, uint[]>) Load(string fileName)
        {
            List<uint> cplist = new List<uint>();
            Dictionary<uint, uint[]> dict = new Dictionary<uint, uint[]>();
            using (var sr = new StreamReader(fileName))
            {
                string line;
                while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                {
                    var s = line.Split(';');
                    var cp = ParseHex(s[0]);
                    var de = DecompositionMappingParse(s[5]);
                    cplist.Add(cp);
                    if(de != null )dict.Add(cp, de);
                }
            }
            return (cplist,dict);
        }

        public static uint[] DecompositionMappingParse(string s)
        {
            var mapping = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // 空なら null
            if (mapping.Length == 0) return null;

            if (mapping[0][0] == '<')
            {
                return mapping.Skip(1).Select(ParseHex).ToArray();
            }

            return Array.ConvertAll(mapping, ParseHex);
        }
    }

}