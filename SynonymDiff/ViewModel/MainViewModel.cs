using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SynonymDiff.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonymDiff.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        string originalFileName, newFileName;
        IFileParser fileParser;

        public MainViewModel(IFileParser fileParser)
        {
            this.fileParser = fileParser;
        }

        public string OriginalFileName
        {
            get
            {
                return originalFileName;
            }
            set
            {
                originalFileName = value;
                RaisePropertyChanged();
            }
        }

        public string NewFileName
        {
            get
            {
                return newFileName;
            }
            set
            {
                newFileName = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand SelectOriginalFile => new RelayCommand(() =>
        {
            OriginalFileName = BrowseFile();
        });

        public RelayCommand SelectNewFile => new RelayCommand(() =>
        {
            NewFileName = BrowseFile();
        });

        public RelayCommand StartDiff => new RelayCommand(
            () =>
            {
                var originalKVPairs = fileParser.ParseFileToKVPair(OriginalFileName);
                var newKVPairs = fileParser.ParseFileToKVPair(NewFileName);
                new ViewModelLocator().Comparer.NewKVPairs = newKVPairs;
                new ViewModelLocator().Comparer.OriginalKVPairs = originalKVPairs;

                new ViewModelLocator().Comparer.RemoveDuplicate();
                new ViewModelLocator().Comparer.CalculateConflict();
                Messenger.Default.Send(new NotificationMessage("ShowConflicts"));
            });

        string BrowseFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                return dlg.FileName;
            }
            else
                return null;
        }
    }
}
