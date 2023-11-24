using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WorkLifeBalance.Data;
using WorkLifeBalance.Handlers;
using WorkLifeBalance.Handlers.Feature;
using WorkLifeBalance.Windows;

namespace WorkLifeBalance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        public static MainWindow? instance;

        public Dispatcher MainDispatcher = Dispatcher.CurrentDispatcher;

        private ImageSource AutomaticImg = new BitmapImage();
        private ImageSource RestImg = new BitmapImage();
        private ImageSource WorkImg = new BitmapImage();

        private SolidColorBrush LightBlueColor = new();
        private SolidColorBrush LightPurpleColor = new();
        private SolidColorBrush OceanBlue = new();

        public MainWindow()
        {
            //initialize singleton
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                instance.Close();
                instance = this;
            }
            CheckAdministratorPerms();
            InitializeComponent();

            AllocConsole();//can be removed in release
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()//Can be removed in release
            .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

            Topmost = true;
            LoadStyleInfo();

            //subscribe events for triggering app when data loaded and updaing Ui on save/load
            DataStorageFeature.Instance.OnLoaded += InitializeApp;
            DataStorageFeature.Instance.OnSaving += () => { DateT.Text = $"Saving data..."; };
            DataStorageFeature.Instance.OnSaved += () => { DateT.Text = $"Today: {DataStorageFeature.Instance.TodayData.DateC.ToString("MM/dd/yyyy")}"; };
            //load data
            _ = DataStorageFeature.Instance.LoadData();
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

        //initialize app called when data was loaded
        private void InitializeApp()
        {
            //call set window location and continue
            _ = SetWindowLocation();

            //set app ready so timers can start
            DataStorageFeature.Instance.IsAppReady = true;

            //subscribe features to the main timer
            TimeHandler.Subscribe(TimeTrackerFeature.Instance.AddFeature());
            TimeHandler.Subscribe(DataStorageFeature.Instance.AddFeature());
            TimeHandler.Subscribe(ActivityTrackerFeature.Instance.AddFeature());

            //check settings to see if you need to add some features
            if (DataStorageFeature.Instance.Settings.AutoDetectWorkingC)
            {
                TimeHandler.Subscribe(StateCheckerFeature.Instance.AddFeature());
            }

            //asign update ui 
            TimeTrackerFeature.Instance.OnSpentTimeChange += UpdateUI;

            //starts the main timer
            TimeHandler.StartTick();

            //check if auto detect is enabled so you update ui
            CheckAutoDetectWorking();

            //asign the todays date
            DateT.Text = $"Today: {DataStorageFeature.Instance.TodayData.DateC.ToString("MM/dd/yyyy")}";
            Log.Information("------------------App Initialized------------------");
        }

        private async Task SetWindowLocation()
        {
            //Moves app in the 4 posible corners based on settings
            await Task.Delay(300);
            Vector2 UserScreen = new Vector2((float)SystemParameters.PrimaryScreenWidth, (float)SystemParameters.PrimaryScreenHeight);
            IntPtr TargetWindow = LowLevelHandler.GetWindow(null, "WorkLifeBalance");

            switch (DataStorageFeature.Instance.Settings.StartUpCornerC)
            {
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
            if (TimeHandler.AppTimmerState == state) return;

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
                        TimeHandler.Subscribe(IdleCheckerFeature.Instance.AddFeature());
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
                            TimeHandler.UnSubscribe(IdleCheckerFeature.Instance.RemoveFeature());
                        }
                    }
                    break;
            }
            TimeHandler.AppTimmerState = state;
            Log.Information($"App state changed to {state}");
        }

        private void ToggleState(object sender, RoutedEventArgs e)
        {
            if (!DataStorageFeature.Instance.IsAppReady || DataStorageFeature.Instance.IsClosingApp) return;

            switch (TimeHandler.AppTimmerState)
            {
                case AppState.Working:
                    SetAppState(AppState.Resting);
                    break;

                case AppState.Resting:
                    SetAppState(AppState.Working);
                    break;
            }
        }

        private void UpdateUI()
        {
            ElapsedWorkT.Text = DataStorageFeature.Instance.TodayData.WorkedAmmountC.ToString("HH:mm:ss");
            ElapsedRestT.Text = DataStorageFeature.Instance.TodayData.RestedAmmountC.ToString("HH:mm:ss");
        }

        public void CheckAutoDetectWorking()
        {
            bool value = DataStorageFeature.Instance.Settings.AutoDetectWorkingC;
            ToggleBtn.IsEnabled = !value;

            ToggleBtn.Background = OceanBlue;
            ToggleRecordingImage.Source = AutomaticImg;
            SetAppState(AppState.Resting);
        }

        private void OpenViewDataWindow(object sender, RoutedEventArgs e)
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.ViewData);
        }

        private void OpenOptionsWindow(object sender, RoutedEventArgs e)
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.Options);
        }

        private void CheckAdministratorPerms()
        {
            if (!LowLevelHandler.IsRunningAsAdmin())
            {
                RestartApplicationWithAdmin();
            }
        }

        //restart app if it dosent have admin rights
        private void RestartApplicationWithAdmin()
        {
            var psi = new ProcessStartInfo
            {
                FileName = DataStorageFeature.Instance.AppExePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(psi);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                ShowErrorBox("The application must be run as Administrator", $"Restart as Administrator Failed: {ex.Message}");
            }
        }

        private async void CloseApp(object sender, RoutedEventArgs e)
        {
            if (DataStorageFeature.Instance.IsClosingApp) return;

            DataStorageFeature.Instance.IsClosingApp = true;

            CloseSideBar(null, null);

            await DataStorageFeature.Instance.SaveData();

            Log.Information("------------------App Shuting Down------------------");
            
            await Log.CloseAndFlushAsync();

            Application.Current.Shutdown();
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


        #region ShowErrorBox
        public static void ShowErrorBox(string action, string messageBoxText, Exception ex, bool ForceShutdown)
        {
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, action, button, icon, MessageBoxResult.Yes);

            if (ForceShutdown)
            {
                Log.Error(ex,$"Show Error Box Triggered with message {action}:{messageBoxText}");
                Application.Current.Shutdown();
            }
            else
            {
                Log.Warning(ex,$"Show Error Box Triggered with message {action}:{messageBoxText}");
            }
        }
        public static void ShowErrorBox(string action, string messageBoxText, Exception ex)
        {
            ShowErrorBox(action, messageBoxText, ex, true);
        }
        public static void ShowErrorBox(string action, string messageBoxText, bool ForceShutdown)
        {
            ShowErrorBox(action, messageBoxText, null, ForceShutdown);
        }
        public static void ShowErrorBox(string action, string messageBoxText)
        {
            ShowErrorBox(action, messageBoxText, null, true);
        }
        #endregion
    }
}   
