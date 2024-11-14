using CommunityToolkit.Mvvm.ComponentModel;
using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services;
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
        private readonly LowLevelHandler lowLevelHandler;
        public SettingsPageVM(DataStorageFeature dataStorageFeature, IFeaturesServices featuresServices, LowLevelHandler lowLevelHandler)
        {
            this.featuresServices = featuresServices;
            this.dataStorageFeature = dataStorageFeature;
            this.lowLevelHandler = lowLevelHandler;
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
            if (dataStorageFeature.Settings.StartWithWindowsC)
            {
                lowLevelHandler.CreateStartupShortcut();
            }
            else
            {
                lowLevelHandler.DeleteStartupShortcut();
            }
        }
    }
}
