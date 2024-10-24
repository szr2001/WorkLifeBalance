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
        private int[]? numbers;

        private readonly DataStorageFeature dataStorageFeature;
        private readonly IFeaturesServices featuresServices;
        public SettingsPageVM(DataStorageFeature dataStorageFeature, IFeaturesServices featuresServices)
        {
            this.featuresServices = featuresServices;
            this.dataStorageFeature = dataStorageFeature;
            PageHeight = 320;
            PageWidth = 250;
            PageName = "Settings";

            InitializeData();
        }

        private void InitializeData()
        {
            Version = $"Version: {dataStorageFeature.Settings.Version}";

            AutoSaveInterval = dataStorageFeature.Settings.SaveInterval;

            AutoDetectInterval = dataStorageFeature.Settings.AutoDetectInterval;

            AutoDetectIdleInterval = dataStorageFeature.Settings.AutoDetectIdleInterval;

            StartWithWin = dataStorageFeature.Settings.StartWithWindowsC;

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

            dataStorageFeature.Settings.StartWithWindowsC = StartWithWin;

            await dataStorageFeature.SaveData();

            ApplyStartToWindows();
            dataStorageFeature.Settings.OnSettingsChanged.Invoke();
        }

        private void ApplyStartToWindows()
        {
            string startupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), $"{dataStorageFeature.Settings.Version}.lnk");

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
                WshShell shell = new();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(startupfolder);
                shortcut.TargetPath = dataStorageFeature.Settings.AppDirectory;
                shortcut.WorkingDirectory = dataStorageFeature.Settings.AppDirectory;
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
