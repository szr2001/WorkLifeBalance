using System;
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

        public DayData? TodayData = null;
        public WLBSettings AppSettings = new();

        private ImageSource? RestImg;
        private ImageSource? WorkImg;

        private SolidColorBrush ? WorkingBtnColor;
        private SolidColorBrush? BreakBtnColorHighlight;

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
            WorkingBtnColor = FindResource("WLFBLigherBlue") as SolidColorBrush;
            BreakBtnColorHighlight = FindResource("WLFBLightPurple") as SolidColorBrush;
        }

        private async Task LoadData()
        {
            TodayData = await DataBaseHandler.ReadDay(DateOnly.FromDateTime(DateTime.Now).ToString("MMddyyyy"));
            AppSettings = await DataBaseHandler.ReadSettings();

            _ = SetWindowLocation();

            IsAppReady = true;

            _ = TimmerLoop();
            _ = SaveLoop();

            DateT.Text = $"Today: {TodayData.DateC.ToString("MM/dd/yyyy")}";
        }

        private async Task SetWindowLocation()
        {
            await Task.Delay(300);
            Vector2 UserScreen = new Vector2((float)SystemParameters.PrimaryScreenWidth, (float)SystemParameters.PrimaryScreenHeight);
            IntPtr TargetWindow = WindowPlacementHelper.GetWindow(null, "WorkLifeBalance");

            switch (AppSettings.StartUpCornerC)
            {
                case AnchorCorner.TopLeft:
                    WindowPlacementHelper.SetWindowLocation(TargetWindow,0, 0);
                    break;
                case AnchorCorner.TopRight:
                    WindowPlacementHelper.SetWindowLocation(TargetWindow, (int)UserScreen.X - 220, 0);
                    break;
                case AnchorCorner.BootomLeft:
                    WindowPlacementHelper.SetWindowLocation(TargetWindow, 0, (int)UserScreen.Y - 180);
                    break;
                case AnchorCorner.BottomRight:
                    WindowPlacementHelper.SetWindowLocation(TargetWindow, (int)UserScreen.X - 220, (int)UserScreen.Y - 180);
                    break;
            }
        }

        private void ToggleRecording(object sender, RoutedEventArgs e)
        {
            if (!IsAppReady) return;
            if (IsClosingApp) return;

            switch (AppTimmerState)
            {
                case TimmerState.Working:
                    ToggleBtn.Background = WorkingBtnColor;
                    ToggleRecordingImage.Source = RestImg;
                    AppTimmerState = TimmerState.Resting;
                    break;

                case TimmerState.Resting:
                    ToggleBtn.Background = BreakBtnColorHighlight;
                    ToggleRecordingImage.Source = WorkImg;
                    AppTimmerState = TimmerState.Working;
                    break;

                case TimmerState.Studying:
                    break;

                default:
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

        private void UpdateUiText()
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

                    case TimmerState.Studying:
                        break;

                    default:
                        break;
                }

                UpdateUiText();

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
        Resting,
        Studying
    }
}
