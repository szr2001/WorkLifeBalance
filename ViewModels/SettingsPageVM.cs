using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IWshRuntimeLibrary;
using Serilog;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using WorkLifeBalance.Models;
using WorkLifeBalance.Services.Feature;
using File = System.IO.File;
using Path = System.IO.Path;

namespace WorkLifeBalance.ViewModels
{
    public partial class SettingsPageVM : SecondWindowPageVMBase
    {
        [ObservableProperty]
        private string version = "2.0.0";

        [ObservableProperty]
        private int autoSaveInterval = 5;

        [ObservableProperty]
        private int autoDetectInterval = 1;

        [ObservableProperty]
        private int autoDetectIdleInterval = 1;

        [ObservableProperty]
        private bool startWithWin = false;

        [ObservableProperty]
        private bool autoDetectWork = false;

        [ObservableProperty]
        private bool autoDetectIdle = false;

        [ObservableProperty]
        public AnchorCorner startUpCorner;

        private DataStorageFeature dataStorageFeature;
        public SettingsPageVM(DataStorageFeature dataStorageFeature)
        {
            this.dataStorageFeature = dataStorageFeature;
            RequiredWindowSize = new Vector2(250, 320);
            WindowPageName = "Settings";

            ApplySettings();
        }

        //updates ui based on loaded settings
        private void ApplySettings()
        {
            StartUpCorner = dataStorageFeature.Settings.StartUpCornerC;

            Version = $"Version: {dataStorageFeature.AppVersion}";

            AutoSaveInterval = dataStorageFeature.Settings.SaveInterval;

            AutoDetectInterval = dataStorageFeature.Settings.AutoDetectInterval;

            AutoDetectIdleInterval = dataStorageFeature.Settings.AutoDetectIdleInterval;

            StartWithWin = dataStorageFeature.Settings.StartWithWindowsC;

            AutoDetectWork = dataStorageFeature.Settings.AutoDetectWorkingC;

            AutoDetectIdle = dataStorageFeature.Settings.AutoDetectIdleC;

            if (dataStorageFeature.Settings.AutoDetectWorkingC)
            {
                //ExpandAutoDetectArea();
            }
            if (dataStorageFeature.Settings.AutoDetectIdleC)
            {
                //ExpandDetectMouseIdleArea();
            }
        }

        [RelayCommand]
        private void SetBotLeftStartup()
        {
            StartUpCorner = AnchorCorner.BootomLeft;
            dataStorageFeature.Settings.StartUpCornerC = StartUpCorner;
            Log.Information($"StartupPosition set to {StartUpCorner}");
        }

        [RelayCommand]
        private void SetBotRightStartup()
        {
            StartUpCorner = AnchorCorner.BottomRight;
            dataStorageFeature.Settings.StartUpCornerC = StartUpCorner;
            Log.Information($"StartupPosition set to {StartUpCorner}");
        }

        [RelayCommand]
        private void SetUpRightStartup()
        {
            StartUpCorner = AnchorCorner.TopRight;
            dataStorageFeature.Settings.StartUpCornerC = StartUpCorner;
            Log.Information($"StartupPosition set to {StartUpCorner}");
        }

        [RelayCommand]
        private void SetUpLeftStartup()
        {
            StartUpCorner = AnchorCorner.TopLeft;
            dataStorageFeature.Settings.StartUpCornerC = StartUpCorner;
            Log.Information($"StartupPosition set to {StartUpCorner}");
        }

        //Each page has a close page so the second window can await the page specific cleanups stuff
        //here we wait set the changed values,apply changes and wait for the save to db
        public override async Task ClosePageAsync()
        {
            dataStorageFeature.Settings.SaveInterval = AutoSaveInterval;

            dataStorageFeature.Settings.AutoDetectInterval = AutoDetectInterval;

            dataStorageFeature.Settings.AutoDetectIdleInterval = AutoDetectIdleInterval;

            dataStorageFeature.Settings.AutoDetectIdleC = (bool)AutoDetectIdle!;

            dataStorageFeature.Settings.AutoDetectWorkingC = AutoDetectWork;

            dataStorageFeature.Settings.StartWithWindowsC = StartWithWin;

            await dataStorageFeature.SaveData();

            ApplyStartToWindows();

            //MainWindow.instance.ApplyAutoDetectWorking();
            Log.Information("Applied Settings");
        }

        private void ApplyStartToWindows()
        {
            string startupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), $"{dataStorageFeature.AppName}.lnk");

            if (dataStorageFeature.Settings.StartWithWindowsC)
            {
                CreateShortcut(startupFolderPath);
            }
            else
            {
                DeleteShortcut(startupFolderPath);
            }
        }

        private void CreateShortcut(string startupfolder)
        {
            if (!File.Exists(startupfolder))
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(startupfolder);
                shortcut.TargetPath = dataStorageFeature.AppExePath;
                shortcut.WorkingDirectory = dataStorageFeature.AppDirectory;
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
            SecondWindow.RequestSecondWindow(SecondWindowType.BackgroundProcesses);
        }

        //private void ToggleAutoDetectWorking(object sender, RoutedEventArgs e)
        //{
        //    if (AutoDetectWorkingBtn.IsChecked == true)
        //    {
        //        ExpandAutoDetectArea();
        //    }
        //    else
        //    {
        //        ContractAutoDetectArea();
        //    }
        //    Log.Information($"AutoDetectWorking changed to {AutoDetectWorkingBtn.IsChecked}");
        //}
        //private void ExpandAutoDetectArea()
        //{
        //    AutoToggleWorkingPanel.Height = 80;
        //    AutoDetectWork = true;
        //}
        //private void ContractAutoDetectArea()
        //{
        //    AutoToggleWorkingPanel.Height = 0;
        //    AutoDetectWork = false;
        //}

        //private void ToggleDetectMouseIdle(object sender, RoutedEventArgs e)
        //{
        //    if (AutoDetectIdleBtn.IsChecked == true)
        //    {
        //        ExpandDetectMouseIdleArea();
        //    }
        //    else
        //    {
        //        ContractDetectMouseIdleArea();
        //        AppTimer.UnSubscribe(IdleCheckerFeature.Instance.RemoveFeature());
        //    }
        //    Log.Information($"AutoDetectIdle changed to {AutoDetectIdleBtn.IsChecked}");
        //}

        //private void ChangeDetectMouseIdleDelay(object sender, TextChangedEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(AutoDetectIdleT.Text) || !int.TryParse(AutoDetectIdleT.Text, out _))
        //    {
        //        AutoDetectIdleT.Text = 1.ToString();
        //        AutoDetectIdleInterval = 1;
        //        return;
        //    }

        //    AutoDetectIdleInterval = int.Parse(AutoDetectIdleT.Text);
        //    Log.Information($"AutoDetectIdleInterval set to {AutoDetectIdleInterval}");
        //}

        //private void ExpandDetectMouseIdleArea()
        //{
        //    AutoDetectIdlePanel.Height = 55;
        //    AutoDetectIdle = true;
        //}
        //private void ContractDetectMouseIdleArea()
        //{
        //    AutoDetectIdlePanel.Height = 0;
        //    AutoDetectIdle = false;
        //}
    }
}
