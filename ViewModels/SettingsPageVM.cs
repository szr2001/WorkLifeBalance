using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IWshRuntimeLibrary;
using Serilog;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Documents;
using WorkLifeBalance.Interfaces;
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
        private AnchorCorner[] anchorCorners = new AnchorCorner[4]
        {
            AnchorCorner.TopLeft,
            AnchorCorner.TopRight,
            AnchorCorner.BootomLeft,
            AnchorCorner.BottomRight
        };

        [ObservableProperty]
        private int[]? numbers;

        [ObservableProperty]
        private AnchorCorner selectedStartupCorner = AnchorCorner.BootomLeft;

        private DataStorageFeature dataStorageFeature;
        private ISecondWindowService secondWindowService;

        public SettingsPageVM(DataStorageFeature dataStorageFeature, ISecondWindowService secondWindowService)
        {
            this.secondWindowService = secondWindowService;
            this.dataStorageFeature = dataStorageFeature;
            RequiredWindowSize = new Vector2(250, 320);
            WindowPageName = "Settings";

            InitializeData();
        }

        private void InitializeData()
        {
            SelectedStartupCorner = dataStorageFeature.Settings.StartUpCornerC;

            Version = $"Version: {dataStorageFeature.AppVersion}";

            AutoSaveInterval = dataStorageFeature.Settings.SaveInterval;

            AutoDetectInterval = dataStorageFeature.Settings.AutoDetectInterval;

            AutoDetectIdleInterval = dataStorageFeature.Settings.AutoDetectIdleInterval;

            StartWithWin = dataStorageFeature.Settings.StartWithWindowsC;

            AutoDetectWork = dataStorageFeature.Settings.AutoDetectWorkingC;

            AutoDetectIdle = dataStorageFeature.Settings.AutoDetectIdleC;

            List<int> numbersTemp = new();
            for(int x = 1; x <= 300; x++)
            {
                numbersTemp.Add(x);
            }
            Numbers = numbersTemp.ToArray();
        }

        public override async Task OnPageClosingAsync()
        {
            dataStorageFeature.Settings.SaveInterval = AutoSaveInterval;

            dataStorageFeature.Settings.AutoDetectInterval = AutoDetectInterval;

            dataStorageFeature.Settings.AutoDetectIdleInterval = AutoDetectIdleInterval;

            dataStorageFeature.Settings.AutoDetectIdleC = (bool)AutoDetectIdle!;

            dataStorageFeature.Settings.AutoDetectWorkingC = AutoDetectWork;

            dataStorageFeature.Settings.StartWithWindowsC = StartWithWin;

            dataStorageFeature.Settings.StartUpCornerC = SelectedStartupCorner;

            await dataStorageFeature.SaveData();

            ApplyStartToWindows();
            dataStorageFeature.Settings.OnSettingsChanged.Invoke();
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
        private void ConfigureAutoDetect()
        {
            secondWindowService.OpenWindowWith<BackgroundProcessesViewPageVM>();
        }
    }
}
