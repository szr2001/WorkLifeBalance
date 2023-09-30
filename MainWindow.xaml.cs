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

        bool IsClosingApp = false;
        bool IsTimmerActive = true;
        bool IsSaveTimmerActive = false;
        bool IsAppReady = false;

        TimmerState AppTimmerState = TimmerState.Resting;

        public static Dispatcher MainDispatcher = Dispatcher.CurrentDispatcher;

        DayData? TodayData = null;
        WLBSettings? AppSettings = null;

        ImageSource? RestImg;
        ImageSource? WorkImg;

        SolidColorBrush ? WorkingBtnColor;
        SolidColorBrush? WorkingBtnColorHighlight;
        SolidColorBrush? BreakBtnColor;
        SolidColorBrush? BreakBtnColorHighlight;
        int ToggleBtnSize = 50;
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
            ToggleBtnSize = (int)ToggleBtn.Width;
            RestImg = new BitmapImage(new Uri($"{Directory.GetCurrentDirectory()}/Assets/Rest.png"));
            WorkImg = new BitmapImage(new Uri($"{Directory.GetCurrentDirectory()}/Assets/Work.png"));
            WorkingBtnColor = FindResource("WLFBLigherBlue") as SolidColorBrush;
            WorkingBtnColorHighlight = FindResource("WLFBLightBlue") as SolidColorBrush;
            BreakBtnColor = FindResource("WLFBLighterPurple") as SolidColorBrush;
            BreakBtnColorHighlight = FindResource("WLFBLightPurple") as SolidColorBrush;
        }

        private async Task LoadData()
        {
            TodayData = await DataBaseHandler.ReadDay(DateOnly.FromDateTime(DateTime.Now).ToString("MMddyyyy"));
            //AppSettings = await DataBaseHandler.ReadSettings();

            try
            {
                TodayData.ConvertSaveDataToUsableData();
                //AppSettings.ConvertSaveDataToUsableData();
            }
            catch (Exception ex)
            {
                ShowErrorBox("Failed to convert data", $"This can be caused by a a missing or corupted database file. {ex.Message}");
            }

            _ = TimmerLoop();
            _ = SetWindowLocation();

            IsAppReady = true;


            DateT.Text = $"Today: {TodayData.DateC.ToString("MM/dd/yyyy")}";
        }

        private async Task SetWindowLocation()
        {
            await Task.Delay(300);
            Vector2 UserScreen = new Vector2((float)SystemParameters.PrimaryScreenWidth, (float)SystemParameters.PrimaryScreenHeight);
            IntPtr TargetWindow = WindowPlacementHelper.GetWindow(null, "WorkLifeBalance");
            
            //Todo: switch statement to set the window start corner based on user settings
            WindowPlacementHelper.SetWindowLocation(TargetWindow, 0, (int)UserScreen.Y - 180);
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

        private void UpdateUiText()
        {
            ElapsedWorkT.Text = TodayData.WorkedAmmountC.ToString("HH:mm:ss");
            ElapsedBreakT.Text = TodayData.RestedAmmountC.ToString("HH:mm:ss");
        }

        private async Task TimmerLoop()
        {
            TimeSpan OneSec = new TimeSpan(0, 0, 1);
            
            while (IsTimmerActive)
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

        private async Task WriteData()
        {
            DateT.Text = $"Saving data...";

            TodayData.ConvertUsableDataToSaveData();
            await DataBaseHandler.WriteDay(TodayData);

            DateT.Text = $"Today: {TodayData.DateC.ToString("MM/dd/yyyy")}";
        }

        private void ViewData(object sender, RoutedEventArgs e)
        {

        }

        private void Options(object sender, RoutedEventArgs e)
        {

        }

        private async void CloseApp(object sender, RoutedEventArgs e)
        {
            if (IsClosingApp) return;

            IsTimmerActive = false;
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

        private void ToggleRecordingBtnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsAppReady) return;
            if (IsClosingApp) return;

            switch (AppTimmerState)
            {
                case TimmerState.Working:
                    ToggleBtn.Background = BreakBtnColorHighlight;
                    break;

                case TimmerState.Resting:
                    ToggleBtn.Background = WorkingBtnColorHighlight;
                    break;

                case TimmerState.Studying:
                    break;

                default:
                    break;
            }
            ToggleBtn.Width = ToggleBtnSize + 5;
            ToggleBtn.Height = ToggleBtnSize + 5;
        }

        private void ToggleRecordingBtnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsAppReady) return;
            if (IsClosingApp) return;

            switch (AppTimmerState)
            {
                case TimmerState.Working:
                    ToggleBtn.Background = BreakBtnColor;
                    break;

                case TimmerState.Resting:
                    ToggleBtn.Background = WorkingBtnColor;
                    break;

                case TimmerState.Studying:
                    break;

                default:
                    break;
            }

            ToggleBtn.Width = ToggleBtnSize;
            ToggleBtn.Height = ToggleBtnSize;
        }

        private void OptionMenuMouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsAppReady) return;
            if (IsClosingApp) return;

            OptionMenuVisibility.Width = new GridLength(35,GridUnitType.Pixel);
            OptionsPannel.Visibility = Visibility.Visible;
        }

        private void OptionMenuMouseLeave(object sender, MouseEventArgs e)
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
