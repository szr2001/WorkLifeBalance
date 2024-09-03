using System.Windows.Controls;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Views
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private SettingsPageVM settingsPageVM;
        public SettingsPage(SettingsPageVM settingsPageVM)
        {
            InitializeComponent();
            this.settingsPageVM = settingsPageVM;
            DataContext = settingsPageVM;
        }
    }
}
