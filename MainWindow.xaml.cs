using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WorkLifeBalance.Data;
using WorkLifeBalance.HandlerClasses;
using WorkLifeBalance.Handlers;
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

            InitializeComponent();
            Topmost = true;
            AllocConsole();
            LoadStyleInfo();

            DataHandler.Instance.OnLoaded += InitializeApp;
            DataHandler.Instance.OnSaving += ()=> { DateT.Text = $"Saving data..."; };
            DataHandler.Instance.OnSaved += ()=> { DateT.Text = $"Today: {DataHandler.Instance.TodayData.DateC.ToString("MM/dd/yyyy")}"; };

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


            TimeHandler.Instance.OnTimerTick += TimeTrackerHandler.Instance.UpdateSpentTime;
            TimeHandler.Instance.OnTimerTick += DataHandler.Instance.TriggerSaveData;

            TimeTrackerHandler.Instance.OnSpentTimeChange += UpdateUI;

            TimeHandler.Instance.StartTick();

            SetAutoDetect(DataHandler.Instance.AppSettings.AutoDetectWorkingC);

            DateT.Text = $"Today: {DataHandler.Instance.TodayData.DateC.ToString("MM/dd/yyyy")}";
        }
        
        private async Task SetWindowLocation()
        {
            await Task.Delay(300);
            Vector2 UserScreen = new Vector2((float)SystemParameters.PrimaryScreenWidth, (float)SystemParameters.PrimaryScreenHeight);
            IntPtr TargetWindow = WindowOptionsHelper.GetWindow(null, "WorkLifeBalance");

            switch (DataHandler.Instance.AppSettings.StartUpCornerC)
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
            DataHandler.Instance.AppTimmerState = state;
        }

        private void ToggleState(object sender, RoutedEventArgs e)
        {
            if (!DataHandler.Instance.IsAppReady || DataHandler.Instance.IsClosingApp) return;

            switch (DataHandler.Instance.AppTimmerState)
            {
                case TimmerState.Working:
                    SetAppState(LightBlueColor, RestImg,TimmerState.Resting);
                    break;

                case TimmerState.Resting:
                    SetAppState(LightPurpleColor, WorkImg, TimmerState.Working);
                    break;
            }
        }

        private void UpdateUI()
        {
            ElapsedWorkT.Text = DataHandler.Instance.TodayData.WorkedAmmountC.ToString("HH:mm:ss");
            ElapsedRestT.Text = DataHandler.Instance.TodayData.RestedAmmountC.ToString("HH:mm:ss");
        }

        //private async Task TimmerLoop()
        //{
        //    TimeSpan OneSec = new TimeSpan(0, 0, 1);

        //    while (DataHandler.Instance.IsAppReady && !DataHandler.Instance.IsClosingApp)
        //    {
        //        switch (AppTimmerState)
        //        {
        //            case TimmerState.Working:
        //                DataHandler.Instance.TodayData.WorkedAmmountC = DataHandler.Instance.TodayData.WorkedAmmountC.Add(OneSec);
        //                break;

        //            case TimmerState.Resting:
        //                DataHandler.Instance.TodayData.RestedAmmountC = DataHandler.Instance.TodayData.RestedAmmountC.Add(OneSec);
        //                break;
        //        }

        //        //temp
        //        try
        //        {
        //            IntPtr foregroundWindowHandle = WindowOptionsHelper.GetForegroundWindow();
        //            string applicationName = WindowOptionsHelper.GetApplicationName(foregroundWindowHandle);

        //            Console.WriteLine($"Application: {applicationName}");
        //        }
        //        catch(Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //        //temp

        //        UpdateUI();

        //        await Task.Delay(1000);
        //    }
        //}

        //private async Task SaveLoop()
        //{
        //    while (DataHandler.Instance.IsAppReady && !DataHandler.Instance.IsClosingApp)
        //    {
        //        await Task.Delay(DataHandler.Instance.AppSettings.SaveInterval * 60000);
        //        await DataHandler.Instance.SaveData();
        //    }
        //}

        //private async Task AutoDetectWorkLoop()
        //{
        //    while (DataHandler.Instance.IsAppReady && !DataHandler.Instance.IsClosingApp)
        //    {
        //        await Task.Delay(5000);

        //    }
        //}

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

        private void OpenViewDataWindow(object sender, RoutedEventArgs e)
        {
            SecondWindow.OpenSecondWindow(SecondWindowType.ViewData);
        }

        private void OpenOptionsWindow(object sender, RoutedEventArgs e)
        {
            SecondWindow.OpenSecondWindow(SecondWindowType.Settings);
        }

        private async void CloseApp(object sender, RoutedEventArgs e)
        {
            if (DataHandler.Instance.IsClosingApp) return;

            CloseSideBar(null,null);

            DataHandler.Instance.IsClosingApp = true;

            DateT.Text = "Closing, waiting for database";

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

        private void OpenSideBar(object sender, MouseEventArgs e)
        {
            if (!DataHandler.Instance.IsAppReady) return;
            if (DataHandler.Instance.IsClosingApp) return;

            OptionMenuVisibility.Width = new GridLength(35,GridUnitType.Pixel);
            OptionsPannel.Visibility = Visibility.Visible;
        }

        private void CloseSideBar(object sender, MouseEventArgs e)
        {
            if (!DataHandler.Instance.IsAppReady) return;
            if (DataHandler.Instance.IsClosingApp) return;

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
