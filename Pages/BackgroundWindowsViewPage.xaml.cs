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
    /// Interaction logic for BackgroundWindowsViewPage.xaml
    /// </summary>
    public partial class BackgroundWindowsViewPage : SecondWindowPageBase
    {
        public BackgroundWindowsViewPage(object? args) : base(args)
        {
            RequiredWindowSize = new Vector2(710, 570);
            pageNme = "Automatic Customize";
            InitializeComponent();
        }

        private void ReturnToPreviousPage(object sender, RoutedEventArgs e)
        {
           SecondWindow.OpenSecondWindow(SecondWindowType.Settings);

        }
    }
}
