using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WorkLifeBalance.Data;
using WorkLifeBalance.HandlerClasses;
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

        public SecondWindow? SecondWindow;

        public static Dispatcher MainDispatcher = Dispatcher.CurrentDispatcher;

        private bool IsClosingApp = false;
        private bool IsAppReady = false;

        private TimmerState AppTimmerState = TimmerState.Resting;

        public TimeOnly CurrentProcessSpentTime = new();
        public DayData? TodayData = null;
        public WLBSettings AppSettings = new();

        private ImageSource AutomaticImg = new BitmapImage();
        private ImageSource RestImg = new BitmapImage();
        private ImageSource WorkImg = new BitmapImage();

        private SolidColorBrush LightBlueColor = new();
        private SolidColorBrush LightPurpleColor = new();
        private SolidColorBrush OceanBlue = new();
        public MainWindow()
        {
            InitializeComponent();
            Topmost = true;
            AllocConsole();
            LoadStyleInfo();
            _ = LoadData();
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

        private async Task LoadData()
        {
            TodayData = await DataBaseHandler.ReadDay(DateOnly.FromDateTime(DateTime.Now).ToString("MMddyyyy"));
            AppSettings = await DataBaseHandler.ReadSettings();

            _ = SetWindowLocation();

            IsAppReady = true;

            _ = TimmerLoop();
            _ = SaveLoop();

            SetAutoDetect(AppSettings.AutoDetectWorkingC);

            DateT.Text = $"Today: {TodayData.DateC.ToString("MM/dd/yyyy")}";
        }
        
        private async Task SetWindowLocation()
        {
            await Task.Delay(300);
            Vector2 UserScreen = new Vector2((float)SystemParameters.PrimaryScreenWidth, (float)SystemParameters.PrimaryScreenHeight);
            IntPtr TargetWindow = WindowOptionsHelper.GetWindow(null, "WorkLifeBalance");

            switch (AppSettings.StartUpCornerC)
            {
                case AnchorCorner.TopLeft:
                    WindowOptionsHelper.SetWindowLocation(TargetWindow,0, 0);
                    break;
                case AnchorCorner.TopRight:
                    WindowOptionsHelper.SetWindowLocation(TargetWindow, (int)UserScreen.X - 220, 0);
                    break;
                case AnchorCorner.BootomLeft:
                    WindowOptionsHelper.SetWindowLocation(TargetWindow, 0, (int)UserScreen.Y - 180);
                    break;
                case AnchorCorner.BottomRight:
                    WindowOptionsHelper.SetWindowLocation(TargetWindow, (int)UserScreen.X - 220, (int)UserScreen.Y - 180);
                    break;
            }
        }

        public void SetAppState(SolidColorBrush backgroundcolor, ImageSource image,TimmerState state)
        {
            ToggleBtn.Background = backgroundcolor;
            ToggleRecordingImage.Source = image;
            AppTimmerState = state;
        }

        private void ToggleState(object sender, RoutedEventArgs e)
        {
            if (!IsAppReady || IsClosingApp) return;

            switch (AppTimmerState)
            {
                case TimmerState.Working:
                    SetAppState(LightBlueColor, RestImg,TimmerState.Resting);
                    break;

                case TimmerState.Resting:
                    SetAppState(LightPurpleColor, WorkImg, TimmerState.Working);
                    break;
            }
        }

        public void OpenSecondWindow(SecondWindowType Page,object? args = null)
        {
            if (SecondWindow != null && SecondWindow.WindowType == Page)
            {
                SecondWindow.Close();
                SecondWindow = null;
                return;
            }
            SecondWindow = new SecondWindow(this, Page, args);
            SecondWindow.Show();
        }

        private void UpdateUI()
        {
            ElapsedWorkT.Text = TodayData.WorkedAmmountC.ToString("HH:mm:ss");
            ElapsedBreakT.Text = TodayData.RestedAmmountC.ToString("HH:mm:ss");
        }

        private async Task TimmerLoop()
        {
            TimeSpan OneSec = new TimeSpan(0, 0, 1);

            while (IsAppReady && !IsClosingApp)
            {
                switch (AppTimmerState)
                {
                    case TimmerState.Working:
                        TodayData.WorkedAmmountC = TodayData.WorkedAmmountC.Add(OneSec);
                        break;

                    case TimmerState.Resting:
                        TodayData.RestedAmmountC = TodayData.RestedAmmountC.Add(OneSec);
                        break;
                }

                //temp
                try
                {
                    IntPtr foregroundWindowHandle = WindowOptionsHelper.GetForegroundWindow();
                    string applicationName = WindowOptionsHelper.GetApplicationName(foregroundWindowHandle);

                    Console.WriteLine($"Application: {applicationName}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                //temp

                UpdateUI();

                await Task.Delay(1000);
            }
        }

        private async Task SaveLoop()
        {
            while (IsAppReady && !IsClosingApp)
            {
                await Task.Delay(AppSettings.SaveInterval * 60000);
                await WriteData();
            }
        }

        private async Task AutoDetectWorkLoop()
        {
            while (IsAppReady && !IsClosingApp)
            {
                await Task.Delay(5000);

            }
        }

        public void SetAutoDetect(bool value)
        {
            ToggleBtn.IsEnabled = !value;

            if (value)
            {
                SetAppState(OceanBlue,AutomaticImg,TimmerState.Resting);
            }
            else
            {
                SetAppState(LightBlueColor, RestImg, TimmerState.Resting);
            }
        }

        public async Task WriteData()
        {
            DateT.Text = $"Saving data...";

            TodayData.ConvertUsableDataToSaveData();
            AppSettings.ConvertUsableDataToSaveData();

            await DataBaseHandler.WriteDay(TodayData);
            await DataBaseHandler.WriteSettings(AppSettings);

            DateT.Text = $"Today: {TodayData.DateC.ToString("MM/dd/yyyy")}";
        }

        private void OpenViewDataWindow(object sender, RoutedEventArgs e)
        {
            OpenSecondWindow(SecondWindowType.ViewData);
        }

        private void OpenOptionsWindow(object sender, RoutedEventArgs e)
        {
            OpenSecondWindow(SecondWindowType.Settings);
        }

        private async void CloseApp(object sender, RoutedEventArgs e)
        {
            if (IsClosingApp) return;

            CloseSideBar(null,null);

            IsClosingApp = true;

            DateT.Text = "Closing, waiting for database";

            await WriteData();

            Application.Current.Shutdown();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void OpenSideBar(object sender, MouseEventArgs e)
        {
            if (!IsAppReady) return;
            if (IsClosingApp) return;

            OptionMenuVisibility.Width = new GridLength(35,GridUnitType.Pixel);
            OptionsPannel.Visibility = Visibility.Visible;
        }

        private void CloseSideBar(object sender, MouseEventArgs e)
        {
            if (!IsAppReady) return;
            if (IsClosingApp) return;

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

    public enum TimmerState
    {
        Working,
        Resting
    }
}
