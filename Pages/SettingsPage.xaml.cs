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

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : SecondWindowPageBase
    {
        int interval = 5;
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
            StartWithWInBtn.IsChecked = secondwindow.MainWindowParent.AppSettings.StartWithWindowsC;
        }

        private void ChangeAutosaveDelay(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(AutosaveT.Text) || int.Parse(AutosaveT.Text) == 0 )
            {
                AutosaveT.Text = 5.ToString();
                interval = 5;
            }
            try
            {
                interval = int.Parse(AutosaveT.Text);
            }
            catch
            {
                AutosaveT.Text = 5.ToString();
                interval = 5;
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
            ParentWindow.MainWindowParent.AppSettings.SaveInterval = interval;
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

    }
}
