using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Views
{
    /// <summary>
    /// Interaction logic for OptionsPage.xaml
    /// </summary>
    public partial class OptionsPage : Page
    {
        private OptionsPageVM optionsPageVM;
        public OptionsPage(OptionsPageVM optionsPageVM)
        {
            InitializeComponent();
            this.optionsPageVM = optionsPageVM;
            DataContext = optionsPageVM;
        }

        [RelayCommand]
        private void OpenSettings()
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.Settings);
        }
    }
}
