/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:SynonymDiff.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using SynonymDiff.Model;

namespace SynonymDiff.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<MergeViewModel>();
            SimpleIoc.Default.Register<AddViewModel>();

            SimpleIoc.Default.Register<IFileParser, SynonymFileParser>();
            SimpleIoc.Default.Register<IDocumentComparer, DocumentComparer>();

        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public MergeViewModel Merge
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MergeViewModel>();
            }
        }

        public IDocumentComparer Comparer
        {
            get
            {
                return SimpleIoc.Default.GetInstance<IDocumentComparer>();
            }
        }

        public AddViewModel Add
        {
            get
            {
                return SimpleIoc.Default.GetInstance<AddViewModel>();
            }
        }
        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
        }
    }
}