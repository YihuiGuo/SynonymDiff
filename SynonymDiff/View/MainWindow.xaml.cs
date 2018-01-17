using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using SynonymDiff.ViewModel;

namespace SynonymDiff.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MergeWindow mergeWindow;
        private AddWindow addWindow;
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);

        }

        private void NotificationMessageReceived(NotificationMessage msg)
        {
            if (msg.Notification == "ShowConflicts")
            {
                mergeWindow = new MergeWindow();
                mergeWindow.Show();
                this.Hide();
            }
            else if (msg.Notification == "ShowAdd")
            {
                addWindow = new AddWindow();
                if (mergeWindow.IsActive) mergeWindow.Close();
                addWindow.Show();
            }
        }

    }
}