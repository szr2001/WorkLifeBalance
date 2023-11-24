using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WorkLifeBalance.Windows;

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
