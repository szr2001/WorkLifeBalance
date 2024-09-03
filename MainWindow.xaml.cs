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
        private MainMenuVM mainMenuVM;
        public MainWindow(MainMenuVM mainMenuVM)
        {
            this.mainMenuVM = mainMenuVM;
            DataContext = this.mainMenuVM;

            Topmost = true;
            LoadStyleInfo();

            InitializeComponent();
            SetWindowLocation();
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

        //sets the app state and also enables/disables the mouse idle check feature based on settings so it will only run of 
        //the app is in working stage, no need to check idle when the user is not working
        public void SetAppState(AppState state)
        {
            //if (AppTimer.AppTimerState == state) return;

            //switch (state)
            //{
            //    case AppState.Working:
            //        if (!DataStorageFeature.Instance.Settings.AutoDetectWorkingC)
            //        {
            //            ToggleBtn.Background = LightPurpleColor;
            //            ToggleRecordingImage.Source = WorkImg;
            //        }

            //        if (DataStorageFeature.Instance.Settings.AutoDetectIdleC)
            //        {
            //            AppTimer.Subscribe(IdleCheckerFeature.Instance.AddFeature());
            //        }
            //        break;

            //    case AppState.Resting:
            //        if (!DataStorageFeature.Instance.Settings.AutoDetectWorkingC)
            //        {
            //            ToggleBtn.Background = LightBlueColor;
            //            ToggleRecordingImage.Source = RestImg;
            //        }

            //        if (DataStorageFeature.Instance.Settings.AutoDetectIdleC)
            //        {
            //            if (!StateCheckerFeature.Instance.IsFocusingOnWorkingWindow)
            //            {
            //                AppTimer.UnSubscribe(IdleCheckerFeature.Instance.RemoveFeature());
            //            }
            //        }
            //        break;
            //}
            //AppTimer.AppTimerState = state;
            //Log.Information($"App state changed to {state}");
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
