using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonymDiff.Model
{
    public interface IFileParser
    {
        List<KeyValuePair<string, string>> ParseLinesToKVPair(string content);
        List<KeyValuePair<string, string>> ParseFileToKVPair(string fileName);
        KeyValuePair<string, string> ParseSynonymLineToKVPair(string synonymFileLine);
    }

    public class SynonymFileParser:IFileParser
    {
        public List<KeyValuePair<string, string>> ParseLinesToKVPair(string content)
        {
            var allLines = content.Split('\n');
            return allLines.Select(line => ParseSynonymLineToKVPair(line)).ToList();
        }
        public List<KeyValuePair<string, string>> ParseFileToKVPair(string fileName)
        {
            var allLines = File.ReadLines(fileName);
            return allLines.Select(line => ParseSynonymLineToKVPair(line)).ToList();
        }

        public KeyValuePair<string, string> ParseSynonymLineToKVPair(string synonymFileLine)
        {
            var segments = synonymFileLine.Split(new string[] { "=>" }, StringSplitOptions.None);
            if (segments.Count() != 2||
                segments.Any(p=>string.IsNullOrEmpty(p)))
                return new KeyValuePair<string, string>();
            return new KeyValuePair<string, string>(segments[1], segments[0]);
        }
    }
}

