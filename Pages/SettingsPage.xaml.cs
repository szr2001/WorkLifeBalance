using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
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
using WorkLifeBalance.Data;
using WorkLifeBalance.Windows;

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : SecondWindowPageBase
    {
        int interval = 5;
        public SettingsPage(SecondWindow secondwindow) : base(secondwindow)
        {
            InitializeComponent();
            RequiredWindowSize = new Vector2(250,340);
            pageNme = "Settings";

            switch (secondwindow.MainWindowParent.AppSettings.StartUpCornerC)
            {
                case AnchorCorner.TopLeft:
                    TopLeftBtn.IsChecked = true;
                    break;
                case AnchorCorner.TopRight:
                    TopRightBtn.IsChecked = true;
                    break;
                case AnchorCorner.BootomLeft:
                    BottomLeftBtn.IsChecked = true;
                    break;
                case AnchorCorner.BottomRight:
                    BottomRightBtn.IsChecked = true;
                    break;
            }
            AutosaveT.Text = secondwindow.MainWindowParent.AppSettings.SaveInterval.ToString();
            StartWithWInBtn.IsChecked = secondwindow.MainWindowParent.AppSettings.StartWithWindowsC;
        }

        private void ChangeAutosaveDelay(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(AutosaveT.Text) || int.Parse(AutosaveT.Text) == 0 )
            {
                AutosaveT.Text = 5.ToString();
                interval = 5;
            }
            try
            {
                interval = int.Parse(AutosaveT.Text);
            }
            catch
            {
                AutosaveT.Text = 5.ToString();
                interval = 5;
            }
        }

        private void SetBotLeftStartup(object sender, RoutedEventArgs e)
        {
            BottomRightBtn.IsChecked = false;
            TopRightBtn.IsChecked = false;
            TopLeftBtn.IsChecked = false;
            ParentWindow.MainWindowParent.AppSettings.StartUpCornerC = AnchorCorner.BootomLeft;
        }

        private void SetBotRightStartup(object sender, RoutedEventArgs e)
        {
            BottomLeftBtn.IsChecked = false;
            TopRightBtn.IsChecked = false;
            TopLeftBtn.IsChecked = false;
            ParentWindow.MainWindowParent.AppSettings.StartUpCornerC = AnchorCorner.BottomRight;
        }

        private void SetUpRightStartup(object sender, RoutedEventArgs e)
        {
            BottomLeftBtn.IsChecked = false;
            BottomRightBtn.IsChecked = false;
            TopLeftBtn.IsChecked = false;
            ParentWindow.MainWindowParent.AppSettings.StartUpCornerC = AnchorCorner.TopRight;
        }

        private void SetUpLeftStartup(object sender, RoutedEventArgs e)
        {
            BottomLeftBtn.IsChecked = false;
            BottomRightBtn.IsChecked = false;
            TopRightBtn.IsChecked = false;
            ParentWindow.MainWindowParent.AppSettings.StartUpCornerC = AnchorCorner.TopLeft;
        }

        private void SetStartWithWin(object sender, RoutedEventArgs e)
        {
            ParentWindow.MainWindowParent.AppSettings.StartWithWindowsC = (bool)StartWithWInBtn.IsChecked;
        }

        private async void SaveSettings(object sender, RoutedEventArgs e)
        {
            SaveBtn.IsEnabled = false;
            ParentWindow.MainWindowParent.AppSettings.SaveInterval = interval;
            await ParentWindow.MainWindowParent.WriteData();
            SaveBtn.IsEnabled = true;
        }
    }
}
