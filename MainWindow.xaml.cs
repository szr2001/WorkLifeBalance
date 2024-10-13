using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public static MainWindow? instance;

        public Dispatcher MainDispatcher = Dispatcher.CurrentDispatcher;
        //use binding ifs to bind to colors and sprites directly in the xaml
        private ImageSource AutomaticImg = new BitmapImage();
        private ImageSource RestImg = new BitmapImage();
        private ImageSource WorkImg = new BitmapImage();

        private SolidColorBrush LightBlueColor = new();
        private SolidColorBrush LightPurpleColor = new();
        private SolidColorBrush OceanBlue = new();

        private readonly bool DebugMode = true;
        public MainWindow()
        {
            Topmost = true;
            //LoadStyleInfo();

            InitializeComponent();
            //SetWindowLocation();
        }

        private void LoadStyleInfo()
        {
            //load images and colors
            RestImg = new BitmapImage(new Uri($"{Directory.GetCurrentDirectory()}/Assets/Rest.png"));
            WorkImg = new BitmapImage(new Uri($"{Directory.GetCurrentDirectory()}/Assets/Work.png"));
            AutomaticImg = new BitmapImage(new Uri($"{Directory.GetCurrentDirectory()}/Assets/Automat.png"));
            LightBlueColor = (SolidColorBrush)FindResource("WLFBLigherBlue");
            LightPurpleColor = (SolidColorBrush)FindResource("WLFBLightPurple");
            OceanBlue = (SolidColorBrush)FindResource("WLFBOceanBlue");
        }

        private void SetWindowLocation()
        {
            //bind to the VM
            //use the DataStorageFeature.Instance.Settings.StartUpCornerC 
        }

        public void ApplyAutoDetectWorking()
        {
            //bool value = DataStorageFeature.Instance.Settings.AutoDetectWorkingC;
            //ToggleBtn.IsEnabled = !value;

            //if (value == true)
            //{
            //    AppTimer.Subscribe(StateCheckerFeature.Instance.AddFeature());
            //    ToggleBtn.Background = OceanBlue;
            //    ToggleRecordingImage.Source = AutomaticImg;
            //}
            //else
            //{
            //    AppTimer.UnSubscribe(StateCheckerFeature.Instance.AddFeature());
            //    ToggleBtn.Background = LightBlueColor;
            //    ToggleRecordingImage.Source = RestImg;
            //    SetAppState(AppState.Resting);
            //}
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void HideWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OpenSideBar(object sender, MouseEventArgs e)
        {
            OptionMenuVisibility.Width = new GridLength(35, GridUnitType.Pixel);
            OptionsPannel.Visibility = Visibility.Visible;
        }

        private void CloseSideBar(object sender, MouseEventArgs e)
        {
            OptionMenuVisibility.Width = new GridLength(15, GridUnitType.Pixel);
            OptionsPannel.Visibility = Visibility.Collapsed;
        }
    }
}
