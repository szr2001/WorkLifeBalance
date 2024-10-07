using System.Windows.Controls;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Views
{
    /// <summary>
    /// Interaction logic for BackgroundWindowsViewPage.xaml
    /// </summary>
    public partial class BackgroundProcessesViewPage : Page
    {
        private BackgroundProcessesViewPageVM backgroundWindowsViewPageVM;
        public BackgroundProcessesViewPage(BackgroundProcessesViewPageVM backgroundWindowsViewPageVM)
        {
            InitializeComponent();
            this.backgroundWindowsViewPageVM = backgroundWindowsViewPageVM;
            DataContext = backgroundWindowsViewPageVM;
        }
    }
}
