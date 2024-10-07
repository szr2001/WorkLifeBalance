using System.Windows.Controls;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Views
{
    /// <summary>
    /// Interaction logic for ViewDataPage.xaml
    /// </summary>
    public partial class ViewDataPage : Page
    {
        private ViewDataPageVM viewDataPageVM;
        public ViewDataPage(ViewDataPageVM viewDataPageVM)
        {
            InitializeComponent();
            this.viewDataPageVM = viewDataPageVM;
            DataContext = viewDataPageVM;
        }
    }
}
