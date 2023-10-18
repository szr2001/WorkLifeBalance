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
using WorkLifeBalance.HandlerClasses;
using WorkLifeBalance.Handlers;
using WorkLifeBalance.Windows;

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for BackgroundWindowsViewPage.xaml
    /// </summary>
    public partial class BackgroundWindowsViewPage : SecondWindowPageBase
    {
        public List<string> DetectedWindows { get; set; } = new();
        public List<string> SelectedWindows { get; set; } = new();
        public BackgroundWindowsViewPage(object? args) : base(args)
        {
            RequiredWindowSize = new Vector2(710, 570);
            pageNme = "Automatic Customize";
            DetectBackgroundWindows();
            DataContext = this;
            InitializeComponent();
        }

        private void DetectBackgroundWindows()
        {
            DetectedWindows = WindowOptionsHelper.GetBackgroundApplicationsName();
        }

        private void ReturnToPreviousPage(object sender, RoutedEventArgs e)
        {
           SecondWindow.OpenSecondWindow(SecondWindowType.Settings);

        }

        public override async Task ClosePageAsync()
        {
            await DataHandler.Instance.SaveData();
        }

        private void SelectProcess(object sender, RoutedEventArgs e)
        {
            //add to list
        }

        private void DeselectProcess(object sender, RoutedEventArgs e)
        {
            //remove from list
        }
    }
}
