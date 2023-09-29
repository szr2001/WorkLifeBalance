using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
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
        bool IsWorking = false;
        DateTime Today = DateTime.Now;
        TimeOnly WorkTimeElapsed = new TimeOnly(0,0,0);
        TimeOnly BreakTimeElapsed = new TimeOnly(0,0,0);

        DayData TodayData = new();
        WLBSettings AppSettings = new();

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
            AllocConsole();
            LoadStyleInfo();
            LoadTodayData();
            this.Topmost = true;
            DateT.Text = $"Today: {Today.Date.ToString("MM/dd/yyyy")}";
            _ = TimmerLoop();
            _ = SetWindowLocation();
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

        private void LoadTodayData()
        {

        }

        private async Task SetWindowLocation()
        {
            await Task.Delay(300);
            Vector2 UserScreen = new Vector2((float)SystemParameters.PrimaryScreenWidth, (float)SystemParameters.PrimaryScreenHeight);
            IntPtr TargetWindow = WindowPlacementHelper.GetWindow(null, "WorkLifeBalance");
            Console.WriteLine(TargetWindow);
            
            //Todo: switch statement to set the window start corner based on user settings
            WindowPlacementHelper.SetWindowLocation(TargetWindow, 0, (int)UserScreen.Y - 180);
        }

        private void ToggleRecording(object sender, RoutedEventArgs e)
        {
            if (IsWorking)
            {
                ToggleBtn.Background = WorkingBtnColor;
                ToggleRecordingImage.Source = RestImg;
                IsWorking = false;
            }
            else
            {
                ToggleBtn.Background = BreakBtnColorHighlight;
                ToggleRecordingImage.Source = WorkImg;
                IsWorking = true;
            }
        }

        private void UpdateUiText()
        {
            ElapsedWorkT.Text = WorkTimeElapsed.ToString("HH:mm:ss");
            ElapsedBreakT.Text = BreakTimeElapsed.ToString("HH:mm:ss");
        }

        private async Task TimmerLoop()
        {
            TimeSpan OneSec = new TimeSpan(0, 0, 1);
            
            while (IsTimmerActive)
            {
                if (IsWorking)
                {
                    WorkTimeElapsed = WorkTimeElapsed.Add(OneSec);
                }
                else
                {
                    BreakTimeElapsed = BreakTimeElapsed.Add(OneSec);
                }

                UpdateUiText();

                await Task.Delay(1000);
            }
        }

        private async Task WriteData()
        {
            IsTimmerActive = false;

            //simulate writing data
            await Task.Delay(3000);
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
            if (IsWorking)
            {
                ToggleBtn.Background = BreakBtnColorHighlight;
            }
            else
            {
                ToggleBtn.Background = WorkingBtnColorHighlight;
            }
            ToggleBtn.Width = ToggleBtnSize + 5;
            ToggleBtn.Height = ToggleBtnSize + 5;
        }

        private void ToggleRecordingBtnMouseLeave(object sender, MouseEventArgs e)
        {
            if (IsWorking)
            {
                ToggleBtn.Background = BreakBtnColor;
            }
            else
            {
                ToggleBtn.Background = WorkingBtnColor;
            }
            ToggleBtn.Width = ToggleBtnSize;
            ToggleBtn.Height = ToggleBtnSize;
        }

        private void OptionMenuMouseEnter(object sender, MouseEventArgs e)
        {
            OptionMenuVisibility.Width = new GridLength(35,GridUnitType.Pixel);
            OptionsPannel.Visibility = Visibility.Visible;
        }

        private void OptionMenuMouseLeave(object sender, MouseEventArgs e)
        {
            OptionMenuVisibility.Width = new GridLength(15, GridUnitType.Pixel);
            OptionsPannel.Visibility = Visibility.Collapsed;
        }
    }
}
