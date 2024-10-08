using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IWshRuntimeLibrary;
using Serilog;
using System;
using System.Numerics;
using System.Threading.Tasks;
using WorkLifeBalance.Models;
using WorkLifeBalance.Services.Feature;
using File = System.IO.File;
using Path = System.IO.Path;

namespace WorkLifeBalance.ViewModels
{
    public partial class SettingsPageVM : SecondWindowPageVMBase
    {
        [ObservableProperty]
        private string version = "";

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
        private bool startupAncorTopLeft = false;

        [ObservableProperty]
        private bool startupAncorTopRight = false;

        [ObservableProperty]
        private bool startupAncorBottomLeft = false;

        [ObservableProperty]
        private bool startupAncorBottomRight = false;

        private DataStorageFeature dataStorageFeature;
        public SettingsPageVM(DataStorageFeature dataStorageFeature)
        {
            this.dataStorageFeature = dataStorageFeature;
            RequiredWindowSize = new Vector2(250, 320);
            WindowPageName = "Settings";

            ApplySettings();
        }

        private void ApplySettings()
        {
            switch (dataStorageFeature.Settings.StartUpCornerC)
            {
                case AnchorCorner.TopLeft:
                    StartupAncorTopLeft = true;
                    break;
                case AnchorCorner.TopRight:
                    StartupAncorTopRight = true;
                    break;
                case AnchorCorner.BootomLeft:
                    StartupAncorBottomLeft = true;
                    break;
                case AnchorCorner.BottomRight:
                    StartupAncorBottomRight = true;
                    break;
            }

            Version = $"Version: {dataStorageFeature.AppVersion}";

            AutoSaveInterval = dataStorageFeature.Settings.SaveInterval;

            AutoDetectInterval = dataStorageFeature.Settings.AutoDetectInterval;

            AutoDetectIdleInterval = dataStorageFeature.Settings.AutoDetectIdleInterval;

            StartWithWin = dataStorageFeature.Settings.StartWithWindowsC;

            AutoDetectWork = dataStorageFeature.Settings.AutoDetectWorkingC;

            AutoDetectIdle = dataStorageFeature.Settings.AutoDetectIdleC;

        }

        public override async Task OnPageClosingAsync()
        {
            dataStorageFeature.Settings.SaveInterval = AutoSaveInterval;

            dataStorageFeature.Settings.AutoDetectInterval = AutoDetectInterval;

            dataStorageFeature.Settings.AutoDetectIdleInterval = AutoDetectIdleInterval;

            dataStorageFeature.Settings.AutoDetectIdleC = (bool)AutoDetectIdle!;

            dataStorageFeature.Settings.AutoDetectWorkingC = AutoDetectWork;

            dataStorageFeature.Settings.StartWithWindowsC = StartWithWin;

            await dataStorageFeature.SaveData();

            ApplyStartToWindows();

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

        [RelayCommand]
        private void ConfigureAutoDetect()//needs more work, make a new class or pass the second window reff 
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.BackgroundProcesses);
        }
    }
}
