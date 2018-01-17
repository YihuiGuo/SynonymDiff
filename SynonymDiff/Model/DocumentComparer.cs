using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonymDiff.Model
{
    public interface IDocumentComparer
    {
        List<KeyValuePair<string, string>> OriginalKVPairs { get; set; }
        List<KeyValuePair<string, string>> NewKVPairs { get; set; }
        Dictionary<string, string> ResultDict { get; set; }

        List<DocumentDiff> ConflictList { get; set; }
        List<string> AddList { get; set; }

        void RemoveDuplicate();
        void CalculateConflict();
        List<string> FindExistingRelatedKeys(string key);
        string SaveResultDict();
    }
    public class DocumentComparer : IDocumentComparer
    {
        List<KeyValuePair<string, string>> originalKVPairs, newKVPairs;
        Dictionary<string, string> OriginalDict { get; set; }
        Dictionary<string, string> NewDict { get; set; }
        public Dictionary<string, string> ResultDict { get; set; }


        public List<DocumentDiff> ConflictList { get; set; }
        public List<string> AddList { get; set; }

        public List<KeyValuePair<string, string>> OriginalKVPairs
        {
            get { return originalKVPairs; }
            set
            {
                originalKVPairs = value;
            }
        }

        public List<KeyValuePair<string, string>> NewKVPairs
        {
            get { return newKVPairs; }
            set
            {
                newKVPairs = value;
            }
        }

        public void RemoveDuplicate()
        {
            this.newKVPairs = this.NewKVPairs.Distinct().ToList();
            this.originalKVPairs = this.OriginalKVPairs.Distinct().ToList();
            this.OriginalDict = this.originalKVPairs.ToDictionary(p => p.Key, p => p.Value);
            this.NewDict = this.newKVPairs.ToDictionary(p => p.Key, p => p.Value);

        }

        public List<string> FindExistingRelatedKeys(string key)
        {
            return OriginalDict.Where(p => p.Value.Split(',').Contains(key))
                    .Select(p => p.Key).ToList();
        }

        public void CalculateConflict()
        {
            ConflictList = new List<DocumentDiff>();
            AddList = new List<string>();
            ResultDict = OriginalDict.ToDictionary(k=>k.Key,k=>k.Value);
            foreach (var key in NewDict.Keys)
            {
                List<string> existingKeys = FindExistingRelatedKeys(key);

                if (OriginalDict.ContainsKey(key))
                {
                    //skip , since the original dict has been manually edited, we don't want new records to mess up existing synonyms.
                    continue;
                }
                else if (existingKeys.Count!=0)
                {
                    //add this to "TO BE DECIDED" list, let user decide what to do
                    ConflictList.Add(new DocumentDiff
                    {
                        OldValue = existingKeys.Select(p => $"{OriginalDict[p]}=>{p}").ToList(),
                        NewValue = $"{NewDict[key]}=>{key}"
                    });
                }
                else
                {
                    //add to "TO BE ADDED" list, let user decide whether add it or skip it
                    AddList.Add($"{NewDict[key]}=>{key}");
                }
            }
        }
        public string SaveResultDict()
        {
            var filename = DateTime.Now.ToString("yyyyMMddHHmmss") + "synonym.txt";
            var lines = ResultDict.Select(p => $"{p.Value}=>{p.Key}").ToList();
            System.IO.File.WriteAllLines($@"{filename}", lines);

            return filename;
        }

    }
}
