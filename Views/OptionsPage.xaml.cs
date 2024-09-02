using System.Numerics;
using System.Windows;
using WorkLifeBalance.Views;

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for OptionsPage.xaml
    /// </summary>
    public partial class OptionsPage : SecondWindowPageBase
    {
        public OptionsPage(object? args) : base(args)
        {
            InitializeComponent();
            RequiredWindowSize = new Vector2(250, 320);
            pageNme = "Options";
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.Settings);
        }
    }
}
