using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using WorkLifeBalance.Data;
using File = System.IO.File;
using WorkLifeBalance.Windows;
using Path = System.IO.Path;
using System.Windows.Controls.Primitives;

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : SecondWindowPageBase
    {
        int Saveinterval = 5;
        int AutoDetectInterval = 5;
        public SettingsPage(SecondWindow secondwindow, object? args) : base(secondwindow,args)
        {
            InitializeComponent();
            RequiredWindowSize = new Vector2(250,340);
            pageNme = "Settings";

            switch (secondwindow.MainWindowParent.AppSettings.StartUpCornerC)
            {
                case AnchorCorner.TopLeft:
                    TopLeftBtn.IsChecked = true;
                    break;
                case AnchorCorner.TopRight:
                    TopRightBtn.IsChecked = true;
                    break;
                case AnchorCorner.BootomLeft:
                    BottomLeftBtn.IsChecked = true;
                    break;
                case AnchorCorner.BottomRight:
                    BottomRightBtn.IsChecked = true;
                    break;
            }

            AutosaveT.Text = secondwindow.MainWindowParent.AppSettings.SaveInterval.ToString();

            AutoDetectT.Text = secondwindow.MainWindowParent.AppSettings.AutoDetectInterval.ToString();

            StartWithWInBtn.IsChecked = secondwindow.MainWindowParent.AppSettings.StartWithWindowsC;

            AutoDetectWorkingBtn.IsChecked = secondwindow.MainWindowParent.AppSettings.AutoDetectWorkingC;

            if (secondwindow.MainWindowParent.AppSettings.AutoDetectWorkingC)
            {
                ExpandAutoDetectArea();
            }
        }

        private void ChangeAutosaveDelay(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(AutosaveT.Text) || int.Parse(AutosaveT.Text) == 0 )
            {
                AutosaveT.Text = 5.ToString();
                Saveinterval = 5;
            }
            try
            {
                Saveinterval = int.Parse(AutosaveT.Text);
            }
            catch
            {
                AutosaveT.Text = 5.ToString();
                Saveinterval = 5;
            }
        }
        private void ChangeAutoDetectDelay(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(AutosaveT.Text) || int.Parse(AutosaveT.Text) == 0)
            {
                AutoDetectT.Text = 5.ToString();
                AutoDetectInterval = 5;
            }
            try
            {
                AutoDetectInterval = int.Parse(AutoDetectT.Text);
            }
            catch
            {
                AutoDetectT.Text = 5.ToString();
                AutoDetectInterval = 5;
            }

        }
        private void SetBotLeftStartup(object sender, RoutedEventArgs e)
        {
            BottomRightBtn.IsChecked = false;
            TopRightBtn.IsChecked = false;
            TopLeftBtn.IsChecked = false;
            ParentWindow.MainWindowParent.AppSettings.StartUpCornerC = AnchorCorner.BootomLeft;
        }

        private void SetBotRightStartup(object sender, RoutedEventArgs e)
        {
            BottomLeftBtn.IsChecked = false;
            TopRightBtn.IsChecked = false;
            TopLeftBtn.IsChecked = false;
            ParentWindow.MainWindowParent.AppSettings.StartUpCornerC = AnchorCorner.BottomRight;
        }

        private void SetUpRightStartup(object sender, RoutedEventArgs e)
        {
            BottomLeftBtn.IsChecked = false;
            BottomRightBtn.IsChecked = false;
            TopLeftBtn.IsChecked = false;
            ParentWindow.MainWindowParent.AppSettings.StartUpCornerC = AnchorCorner.TopRight;
        }

        private void SetUpLeftStartup(object sender, RoutedEventArgs e)
        {
            BottomLeftBtn.IsChecked = false;
            BottomRightBtn.IsChecked = false;
            TopRightBtn.IsChecked = false;
            ParentWindow.MainWindowParent.AppSettings.StartUpCornerC = AnchorCorner.TopLeft;
        }

        private void SetStartWithWin(object sender, RoutedEventArgs e)
        {
            ParentWindow.MainWindowParent.AppSettings.StartWithWindowsC = (bool)StartWithWInBtn.IsChecked;
        }

        private async void SaveSettings(object sender, RoutedEventArgs e)
        {
            SaveBtn.IsEnabled = false;

            ApplyStartToWindows();

            ParentWindow.MainWindowParent.AppSettings.SaveInterval = Saveinterval;

            ParentWindow.MainWindowParent.AppSettings.AutoDetectInterval = AutoDetectInterval;

            await ParentWindow.MainWindowParent.WriteData();

            SaveBtn.IsEnabled = true;
        }

        private void ApplyStartToWindows()
        {
            string appName = "WorkLifeBalance";
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string appPath = Path.Combine(appDirectory, $"{appName}.exe");

            string startupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), $"{appName}.lnk");

            if (ParentWindow.MainWindowParent.AppSettings.StartWithWindowsC)
            {
                CreateShortcut(appPath,appDirectory, startupFolderPath);
            }
            else
            {
                DeleteShortcut(startupFolderPath);
            }
        }

        private void CreateShortcut(string appPath,string appdirectory, string startupfolder)
        {
            // Create a shortcut if it doesn't exist
            if (!File.Exists(startupfolder))
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(startupfolder);
                shortcut.TargetPath = appPath;
                shortcut.WorkingDirectory = appdirectory;
                shortcut.Save();
            }
        }

        private void DeleteShortcut(string startupfolder)
        {
            if (File.Exists(startupfolder))
            {
                File.Delete(startupfolder);
            }
        }

        private void ConfigureAutoDetectBtn(object sender, RoutedEventArgs e)
        {
            ParentWindow.MainWindowParent.OpenSecondWindow(SecondWindowType.BackgroundProcesses);
        }

        private void ToggleAutoDetectWorking(object sender, RoutedEventArgs e)
        {
            if (AutoDetectWorkingBtn.IsChecked == true)
            {
                ExpandAutoDetectArea();
            }
            else
            {
                ContractAutoDetectArea();
            }
        }
        private void ExpandAutoDetectArea()
        {
                AutoToggleWorkingPanel.Height = 80;
            ParentWindow.MainWindowParent.AppSettings.AutoDetectWorkingC = (bool)AutoDetectWorkingBtn.IsChecked;
            ParentWindow.MainWindowParent.SetAutoDetect((bool)AutoDetectWorkingBtn.IsChecked);
        }
        private void ContractAutoDetectArea()
        {
                AutoToggleWorkingPanel.Height = 0;
            ParentWindow.MainWindowParent.AppSettings.AutoDetectWorkingC = (bool)AutoDetectWorkingBtn.IsChecked;
            ParentWindow.MainWindowParent.SetAutoDetect((bool)AutoDetectWorkingBtn.IsChecked);
        }
    }
}
