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
        public static extern bool AllocConsole();

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
            Topmost = true;
            AllocConsole();
            LoadStyleInfo();

            DataHandler.Instance.OnLoaded += InitializeApp;
            DataHandler.Instance.OnSaving += ()=> { DateT.Text = $"Saving data...";};
            DataHandler.Instance.OnSaved += ()=> { DateT.Text = $"Today: {DataHandler.Instance.TodayData.DateC.ToString("MM/dd/yyyy")}";};

            _ = DataHandler.Instance.LoadData();
        }

        private void LoadStyleInfo()
        {
            RestImg = new BitmapImage(new Uri($"{Directory.GetCurrentDirectory()}/Assets/Rest.png"));
            WorkImg = new BitmapImage(new Uri($"{Directory.GetCurrentDirectory()}/Assets/Work.png"));
            AutomaticImg = new BitmapImage(new Uri($"{Directory.GetCurrentDirectory()}/Assets/Automat.png"));
            LightBlueColor = (SolidColorBrush)FindResource("WLFBLigherBlue");
            LightPurpleColor = (SolidColorBrush)FindResource("WLFBLightPurple");
            OceanBlue = (SolidColorBrush)FindResource("WLFBOceanBlue");
        }

        private void InitializeApp()
        {
            _ = SetWindowLocation();

            DataHandler.Instance.IsAppReady = true;

            //make a method in every handler that returns a delegate like Get(methodname) and call that one so you can run code and also return the method
            TimeHandler.Instance.Subscribe(TimeTrackerHandler.Instance.AddFeature());
            TimeHandler.Instance.Subscribe(DataHandler.Instance.AddFeature());
            TimeHandler.Instance.Subscribe(ActivityTrackerHandler.Instance.AddFeature());

            if (DataHandler.Instance.Settings.AutoDetectWorkingC)
            {
                TimeHandler.Instance.Subscribe(StateChangerHandler.Instance.AddFeature());
            }

            TimeTrackerHandler.Instance.OnSpentTimeChange += UpdateUI;

            TimeHandler.Instance.StartTick();

            CheckAutoDetectWorking();

            DateT.Text = $"Today: {DataHandler.Instance.TodayData.DateC.ToString("MM/dd/yyyy")}";
        }

        private async Task SetWindowLocation()
        {
            await Task.Delay(300);
            Vector2 UserScreen = new Vector2((float)SystemParameters.PrimaryScreenWidth, (float)SystemParameters.PrimaryScreenHeight);
            IntPtr TargetWindow = LowLevelHandler.GetWindow(null, "WorkLifeBalance");

            switch (DataHandler.Instance.Settings.StartUpCornerC)
            {
                case AnchorCorner.TopLeft:
                    LowLevelHandler.SetWindowLocation(TargetWindow,0, 0);
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

        public void SetAppState(AppState state)
        {
            switch (state)
            {
                case AppState.Working:
                    if (!DataHandler.Instance.Settings.AutoDetectWorkingC)
                    {
                        ToggleBtn.Background = LightPurpleColor;
                        ToggleRecordingImage.Source = WorkImg;
                    }

                    if (DataHandler.Instance.Settings.AutoDetectIdleC)
                    {
                        TimeHandler.Instance.Subscribe(MouseIdleHandler.Instance.AddFeature());
                    }
                    break;

                case AppState.Resting:
                    if (!DataHandler.Instance.Settings.AutoDetectWorkingC)
                    {
                        ToggleBtn.Background = LightBlueColor;
                        ToggleRecordingImage.Source = RestImg;
                    }

                    if (DataHandler.Instance.Settings.AutoDetectIdleC)
                    {
                        if(!StateChangerHandler.Instance.IsFocusingOnWorkingWindow)
                        {
                            TimeHandler.Instance.UnSubscribe(MouseIdleHandler.Instance.RemoveFeature());
                        }
                    }
                    break;
            }
            TimeHandler.Instance.AppTimmerState = state;
        }
        private void ToggleState(object sender, RoutedEventArgs e)
        {
            if (!DataHandler.Instance.IsAppReady || DataHandler.Instance.IsClosingApp) return;

            switch (TimeHandler.Instance.AppTimmerState)
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
            ElapsedWorkT.Text = DataHandler.Instance.TodayData.WorkedAmmountC.ToString("HH:mm:ss");
            ElapsedRestT.Text = DataHandler.Instance.TodayData.RestedAmmountC.ToString("HH:mm:ss");
        }

        public void CheckAutoDetectWorking()
        {
            bool value = DataHandler.Instance.Settings.AutoDetectWorkingC;
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
            if(!LowLevelHandler.IsRunningAsAdmin())
            {
                RestartApplicationWithAdmin();
            }
        }
        private void RestartApplicationWithAdmin()
        {
            var psi = new ProcessStartInfo
            {
                FileName = DataHandler.Instance.AppExePath,
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
            if (DataHandler.Instance.IsClosingApp) return;

            DataHandler.Instance.IsClosingApp = true;

            CloseSideBar(null,null);

            await DataHandler.Instance.SaveData();

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
            OptionMenuVisibility.Width = new GridLength(35,GridUnitType.Pixel);
            OptionsPannel.Visibility = Visibility.Visible;
        }

        private void CloseSideBar(object sender, MouseEventArgs e)
        {
            OptionMenuVisibility.Width = new GridLength(15, GridUnitType.Pixel);
            OptionsPannel.Visibility = Visibility.Collapsed;
        }

        public static void ShowErrorBox(string action, string messageBoxText, bool ForceShutdown = true)
        {
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, action, button, icon, MessageBoxResult.Yes);

            if (ForceShutdown)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
