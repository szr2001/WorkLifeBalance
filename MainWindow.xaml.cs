using Serilog;
using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WorkLifeBalance.Models;
using WorkLifeBalance.Services;
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
            _ = SetWindowLocation();
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

        private async Task SetWindowLocation()
        {
            //Moves app in the 4 posible corners based on settings
            //delay because why not ? (waiting for window to appear)
            await Task.Delay(300);
            Vector2 UserScreen = Vector2.Zero;
            IntPtr TargetWindow = IntPtr.Zero;
            try
            {
                UserScreen = new Vector2((float)SystemParameters.PrimaryScreenWidth, (float)SystemParameters.PrimaryScreenHeight);
                TargetWindow = LowLevelHandler.GetWindow(null, "WorkLifeBalance");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to find Window for seting StartupLocation");
            }

            switch (DataStorageFeature.Instance.Settings.StartUpCornerC)
            {
                //calculate corners based on user resolution
                case AnchorCorner.TopLeft:
                    LowLevelHandler.SetWindowLocation(TargetWindow, 0, 0);
                    break;
                case AnchorCorner.TopRight:
                    LowLevelHandler.SetWindowLocation(TargetWindow, (int)UserScreen.X - 220, 0);
                    break;
                case AnchorCorner.BootomLeft:
                    LowLevelHandler.SetWindowLocation(TargetWindow, 0, (int)UserScreen.Y - 180);
                    break;
                case AnchorCorner.BottomRight:
                    LowLevelHandler.SetWindowLocation(TargetWindow, (int)UserScreen.X - 220, (int)UserScreen.Y - 180);
                    break;
            }
        }

        //sets the app state and also enables/disables the mouse idle check feature based on settings so it will only run of 
        //the app is in working stage, no need to check idle when the user is not working
        public void SetAppState(AppState state)
        {
            if (AppTimer.AppTimerState == state) return;

            switch (state)
            {
                case AppState.Working:
                    if (!DataStorageFeature.Instance.Settings.AutoDetectWorkingC)
                    {
                        ToggleBtn.Background = LightPurpleColor;
                        ToggleRecordingImage.Source = WorkImg;
                    }

                    if (DataStorageFeature.Instance.Settings.AutoDetectIdleC)
                    {
                        AppTimer.Subscribe(IdleCheckerFeature.Instance.AddFeature());
                    }
                    break;

                case AppState.Resting:
                    if (!DataStorageFeature.Instance.Settings.AutoDetectWorkingC)
                    {
                        ToggleBtn.Background = LightBlueColor;
                        ToggleRecordingImage.Source = RestImg;
                    }

                    if (DataStorageFeature.Instance.Settings.AutoDetectIdleC)
                    {
                        if (!StateCheckerFeature.Instance.IsFocusingOnWorkingWindow)
                        {
                            AppTimer.UnSubscribe(IdleCheckerFeature.Instance.RemoveFeature());
                        }
                    }
                    break;
            }
            AppTimer.AppTimerState = state;
            Log.Information($"App state changed to {state}");
        }

        private void ToggleState(object sender, RoutedEventArgs e)
        {
            if (!DataStorageFeature.Instance.IsAppReady || DataStorageFeature.Instance.IsClosingApp) return;

            switch (AppTimer.AppTimerState)
            {
                case AppState.Working:
                    SetAppState(AppState.Resting);
                    break;

                case AppState.Resting:
                    SetAppState(AppState.Working);
                    break;
            }
        }

        public void ApplyAutoDetectWorking()
        {
            bool value = DataStorageFeature.Instance.Settings.AutoDetectWorkingC;
            ToggleBtn.IsEnabled = !value;

            if (value == true)
            {
                AppTimer.Subscribe(StateCheckerFeature.Instance.AddFeature());
                ToggleBtn.Background = OceanBlue;
                ToggleRecordingImage.Source = AutomaticImg;
            }
            else
            {
                AppTimer.UnSubscribe(StateCheckerFeature.Instance.AddFeature());
                ToggleBtn.Background = LightBlueColor;
                ToggleRecordingImage.Source = RestImg;
                SetAppState(AppState.Resting);
            }
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
