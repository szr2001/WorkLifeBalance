using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        DateOnly TodayDate;
        TimeOnly WorkTimeElapsed = new TimeOnly(0,0,0);
        TimeOnly BreakTimeElapsed = new TimeOnly(0,0,0);
        public MainWindow()
        {
            InitializeComponent();
            AllocConsole();
            TodayDate = DateOnly.FromDateTime(DateTime.Now);
            DateT.Text = TodayDate.ToString();
            _ = TimmerLoop();
        }

        private void ToggleRecording(object sender, RoutedEventArgs e)
        {
            if (IsWorking)
            {
                IsWorking = false;
            }
            else
            {
                IsWorking = true;
            }
        }
        private void SaveDataToFile()
        {
        }
        private void UpdateUiText()
        {
            ElapsedWorkT.Text = $"{WorkTimeElapsed.Hour}:{WorkTimeElapsed.Minute}:{WorkTimeElapsed.Second}";
            ElapsedBreakT.Text = $"{BreakTimeElapsed.Hour}:{BreakTimeElapsed.Minute}:{BreakTimeElapsed.Second}";
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
                Console.WriteLine("r");
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
    }
}
