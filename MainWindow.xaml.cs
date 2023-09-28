using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
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
        

        bool IsWorking = false;
        DateTime Today = DateTime.Now;
        TimeOnly WorkTimeElapsed = new TimeOnly(0,0,0);
        TimeOnly BreakTimeElapsed = new TimeOnly(0,0,0);

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

            this.Topmost = true;
            DateT.Text = $"Today: {Today.Date.ToString("MM/dd/yyyy")}";
            _ = TimmerLoop();
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

        private void SaveDataToFile()
        {
        }

        private void UpdateUiText()
        {
            ElapsedWorkT.Text = WorkTimeElapsed.ToString("HH:mm:ss");
            ElapsedBreakT.Text = BreakTimeElapsed.ToString("HH:mm:ss");
        }

        private async Task TimmerLoop()
        {
            TimeSpan OneSec = new TimeSpan(0, 0, 1);
            while (true)
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

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ToggleRecordingBtnMouseEnter(object sender, MouseEventArgs e)
        {
            if(IsWorking)
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
    }
}
