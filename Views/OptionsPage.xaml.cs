using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Pages
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
            RequiredWindowSize = new Vector2(250, 320);
            pageNme = "Options";
            this.optionsPageVM = optionsPageVM;
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.Settings);
        }
    }
}
