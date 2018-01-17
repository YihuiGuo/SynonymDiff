using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
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
    public class MergeViewModel : ViewModelBase
    {
        private IDocumentComparer documentComparer;
        private int resolvedConflicts, sumOfConflicts;
        private string topHintContent, originalText, newText, mergedText, currentKey;
        private bool isOriginalChecked, isNewChecked;
        private List<string> unsolvedConflicts;

        public MergeViewModel(IDocumentComparer comparer)
        {
            this.documentComparer = comparer;
            InitMerge();
        }

        public void InitMerge()
        {
            SumOfConflicts = documentComparer.ConflictList.Count();
            ResolvedConflicts = 0;
            UnsolvedConflicts = documentComparer.ConflictList
                .Select(p => p.NewValue.Split(new string[] { "=>" }, StringSplitOptions.None).Last()).ToList();
            CurrentKey = UnsolvedConflicts.First();
            RefreshContent(documentComparer.ConflictList.First());
        }

        public void RefreshContent(DocumentDiff diff)
        {
            var a = UnsolvedConflicts.ToList();
            UnsolvedConflicts = a;
            OriginalText = string.Join("\n", diff.OldValue.ToArray());
            NewText = diff.NewValue;
            MergedText = string.Empty;
            IsOriginalChecked = false;
            IsNewChecked = false;
        }



        #region MVVM Light Observable Properties
        public bool IsOriginalChecked
        {
            get { return isOriginalChecked; }
            set
            {
                isOriginalChecked = value;
                GenerateMergeText();
                RaisePropertyChanged();
            }
        }

        public bool IsNewChecked
        {
            get { return isNewChecked; }
            set
            {
                isNewChecked = value;
                GenerateMergeText();
                RaisePropertyChanged();
            }
        }
        public string OriginalText
        {
            get { return originalText; }
            set
            {
                originalText = value;
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

        public string MergedText
        {
            get { return mergedText; }
            set
            {
                mergedText = value;
                RaisePropertyChanged();
            }
        }

        public int ResolvedConflicts
        {
            get { return resolvedConflicts; }
            set
            {
                resolvedConflicts = value;
                GenerateTopHintContent();
                RaisePropertyChanged();
            }
        }

        public int SumOfConflicts
        {
            get { return sumOfConflicts; }
            set
            {
                sumOfConflicts = value;
                RaisePropertyChanged();
            }
        }

        public string CurrentKey
        {
            get { return currentKey; }
            set
            {
                currentKey = value;
                if (currentKey != null) {
                    LoadContentByCurrentKey(currentKey);
                    RaisePropertyChanged();
                }
                
            }
        }

        public RelayCommand FinishThisConflict => new RelayCommand(() => { ResolveCurrentConflict(); ContinueProcessing(); });

        public string HintContent
        {
            get { return topHintContent; }
            set
            {
                topHintContent = value;
                RaisePropertyChanged();
            }
        }

        public List<string> UnsolvedConflicts
        {
            get { return unsolvedConflicts; }
            set
            {
                unsolvedConflicts = value;
                if (unsolvedConflicts != value)
                    RaisePropertyChanged();
            }
        }

        #endregion

        private void GenerateTopHintContent()
        {
            HintContent = $"{ResolvedConflicts}/{SumOfConflicts}";
        }

        private void GenerateMergeText()
        {
            MergedText = string.Empty;
            if (IsOriginalChecked) MergedText += OriginalText + "\n";
            if (IsNewChecked) MergedText += NewText;
        }

        private void LoadContentByCurrentKey(string currentKey)
        {
            var diff = FindDiffByNewKey(currentKey);
            RefreshContent(diff);
        }

        private DocumentDiff FindDiffByNewKey(string newKey)
        {
            return documentComparer.ConflictList
                .First(p => p.NewValue.Split(new string[] { "=>" }, StringSplitOptions.None).Last() == currentKey);
        }
        private void ResolveCurrentConflict()
        {
            //if the user make any modification to original text, or does not select original text,
            //this means we need to remove related items in original file.
            //partial unchanged items will be recovered later
            var removeKeys = documentComparer.FindExistingRelatedKeys(CurrentKey);
            foreach (var rmkey in removeKeys)
            {
                documentComparer.ResultDict.Remove(rmkey);
            }
            //if any text exists in the input pane,
            //we would ignore whether the new item was selected, just commit changes made in edit pane.
            var parser = ServiceLocator.Current.GetInstance<IFileParser>();
            var kvPairs = parser.ParseLinesToKVPair(MergedText);
            foreach (var kv in kvPairs)
            {
                if (kv.Key == null) continue;
                documentComparer.ResultDict.Add(kv.Key, kv.Value);
            }
            UnsolvedConflicts.Remove(CurrentKey);
        }

        private void ContinueProcessing()
        {
            var nextkey = UnsolvedConflicts.FirstOrDefault();
            if (nextkey != null)
            {
                CurrentKey = nextkey;
                LoadContentByCurrentKey(nextkey);
                ResolvedConflicts++;
            }
            else if(documentComparer.AddList.Count()!=0)
            {
                ProceedToNextPage();
            }
            else
            {
                MessageBox.Show($"File saved to project folder, file name is{documentComparer.SaveResultDict()}");
                Environment.Exit(-1);
            }
        }

        private void ProceedToNextPage()
        {
            Messenger.Default.Send(new NotificationMessage("ShowAdd"));
        }

    }
}
