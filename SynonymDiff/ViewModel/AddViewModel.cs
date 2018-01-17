using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using SynonymDiff.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SynonymDiff.ViewModel
{
    public class AddViewModel : ViewModelBase
    {
        IFileParser fileParser;
        IDocumentComparer documentComparer;
        private int handledNewEntries, sumOfNewEntries;
        private string topHintContent, currentKey, newText;
        private List<string> unhandledEntries;

        public AddViewModel(IFileParser parser, IDocumentComparer comparer)
        {
            this.fileParser = parser;
            this.documentComparer = comparer;
            SumOfNewEntries = documentComparer.ConflictList.Count();
            HandledNewEntries = 0;
            UnhandledEntries = documentComparer.AddList
                .Select(p => p.Split(new string[] { "=>" }, StringSplitOptions.None).Last()).ToList();
            CurrentKey = UnhandledEntries.First();
            InitAdd();
        }
        public void InitAdd()
        {
            SumOfNewEntries = documentComparer.AddList.Count();
            HandledNewEntries = 0;
            UnhandledEntries = documentComparer.AddList
                .Select(p => p.Split(new string[] { "=>" }, StringSplitOptions.None).Last()).ToList();
            CurrentKey = UnhandledEntries.First();
            LoadContentByCurrentKey(documentComparer.AddList.First());
        }

        public int HandledNewEntries
        {
            get { return handledNewEntries; }
            set
            {
                handledNewEntries = value;
                GenerateTopHintContent();
                RaisePropertyChanged();
            }
        }
        public List<string> UnhandledEntries
        {
            get { return unhandledEntries; }
            set
            {
                unhandledEntries = value;
                if (unhandledEntries != value)
                    RaisePropertyChanged();
            }
        }

        public string NewText
        {
            get { return newText; }
            set
            {
                newText = value;
                RaisePropertyChanged();
            }
        }

        public string CurrentKey
        {
            get { return currentKey; }
            set
            {
                currentKey = value;
                if (currentKey != null)
                {
                    LoadContentByCurrentKey(currentKey);
                    RaisePropertyChanged();
                }

            }
        }

        private void LoadContentByCurrentKey(string currentKey)
        {
            var a = UnhandledEntries.ToList();
            UnhandledEntries = a;
            NewText = FindStringByKey(currentKey);
        }

        public int SumOfNewEntries
        {
            get { return sumOfNewEntries; }
            set
            {
                sumOfNewEntries = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand AddEntry => new RelayCommand(() => { HandleCurrentEntry(); ContinueProcessing(); });

        public RelayCommand DiscardEntry => new RelayCommand(() => { HandleCurrentEntry(); ContinueProcessing(); });


        public string HintContent
        {
            get { return topHintContent; }
            set
            {
                topHintContent = value;
                RaisePropertyChanged();
            }
        }

        private void DiscardCurrentEntry()
        {
            UnhandledEntries.Remove(CurrentKey);
        }

        private void HandleCurrentEntry()
        {
            //if any text exists in the input pane,
            //we would ignore whether the new item was selected, just commit changes made in edit pane.
            var parser = ServiceLocator.Current.GetInstance<IFileParser>();
            var kvPairs = parser.ParseLinesToKVPair(NewText);
            foreach (var kv in kvPairs)
            {
                if (kv.Key == null) continue;
                documentComparer.ResultDict.Add(kv.Key, kv.Value);
            }
            UnhandledEntries.Remove(CurrentKey);
        }

        private void ContinueProcessing()
        {
            var nextkey = UnhandledEntries.FirstOrDefault();
            if (nextkey != null)
            {
                CurrentKey = nextkey;
                LoadContentByCurrentKey(nextkey);
                HandledNewEntries++;
            }
            else
            {
                MessageBox.Show($"File saved to project folder, file name is{documentComparer.SaveResultDict()}");
                Environment.Exit(-1);
            }
        }

        private string FindStringByKey(string newKey)
        {
            return documentComparer.AddList
                .First(p => p.Split(new string[] { "=>" }, StringSplitOptions.None).Last() == currentKey);
        }

        private void GenerateTopHintContent()
        {
            HintContent = $"{HandledNewEntries}/{SumOfNewEntries}";
        }

    }
}
