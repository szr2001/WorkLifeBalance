using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.IO.Pipes;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Models;
using WorkLifeBalance.Services;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.ViewModels
{
    public partial class MainMenuVM : ObservableObject
    {


        //bind to dateOnly and timeOnly and use converters
        [ObservableProperty]
        public string? dateText;

        [ObservableProperty]
        public TimeOnly elapsedWorkTime;

        [ObservableProperty]
        public TimeOnly elapsedRestTime;

        [ObservableProperty]
        public AppState appState = AppState.Resting;

        [ObservableProperty]
        public bool autoDetectWork;

        [ObservableProperty]
        public int startupTop = 0;

        [ObservableProperty]
        public int startupLeft = 0;

        private readonly AppStateHandler appStateHandler;
        private readonly LowLevelHandler lowLevelHandler;
        private readonly DataStorageFeature dataStorageFeature;
        private readonly TimeTrackerFeature timeTrackerFeature;
        private readonly ISecondWindowService secondWindowService;
        public MainMenuVM(AppTimer mainTimer, LowLevelHandler lowLevelHandler, DataStorageFeature dataStorageFeature, TimeTrackerFeature timeTrackerFeature, ISecondWindowService secondWindowService, AppStateHandler appStateHandler)
        {
            this.lowLevelHandler = lowLevelHandler;
            this.dataStorageFeature = dataStorageFeature;
            this.timeTrackerFeature = timeTrackerFeature;
            this.secondWindowService = secondWindowService;
            this.appStateHandler = appStateHandler;

            DateText = $"Today: {dataStorageFeature.TodayData.DateC:MM/dd/yyyy}";

            InitializeStartupLocation();
            SubscribeToEvents();
            OnSettingsChanged();
        }

        private void InitializeStartupLocation()
        {
            int ScreenWidth = (int)SystemParameters.PrimaryScreenWidth;
            int ScreenHeight = (int)SystemParameters.PrimaryScreenHeight;

            switch (dataStorageFeature.Settings.StartUpCornerC)
            {
                case AnchorCorner.TopLeft:
                    StartupLeft = 0;
                    StartupTop = 0;
                    break;
                case AnchorCorner.TopRight:
                    StartupLeft = ScreenWidth - 220;
                    StartupTop = 0;
                    break;
                case AnchorCorner.BootomLeft:
                    StartupLeft = 0;
                    StartupTop = ScreenHeight - 180;
                    break;
                case AnchorCorner.BottomRight:
                    StartupLeft = ScreenWidth - 220;
                    StartupTop = ScreenHeight - 180;
                    break;
            }
        }

        private void SubscribeToEvents()
        {
            appStateHandler.OnStateChanges += OnStateChanged;

            dataStorageFeature.Settings.OnSettingsChanged += OnSettingsChanged;

            dataStorageFeature.OnSaving += OnSavingData;
            dataStorageFeature.OnSaved += OnDataSaved;

            timeTrackerFeature.OnSpentTimeChange += OnTimeSpentChanged;
        }

        private void OnStateChanged(AppState state)
        {
            AppState = state;
        }

        private void OnSettingsChanged()
        {
            AutoDetectWork = dataStorageFeature.Settings.AutoDetectWorkingC;
        }

        private void OnSavingData()
        {
            DateText = "Saving data...";
        }

        private void OnDataSaved()
        {
            DateText = $"Today: {dataStorageFeature.TodayData.DateC:MM/dd/yyyy}";
        }

        private void OnTimeSpentChanged()
        {
            ElapsedWorkTime = dataStorageFeature.TodayData.WorkedAmmountC;
            ElapsedRestTime = dataStorageFeature.TodayData.RestedAmmountC;
        }

        [RelayCommand]
        public void OpenViewDataWindow()
        {
            secondWindowService.OpenWindowWith<ViewDataPageVM>();
        }

        [RelayCommand]
        public void OpenOptionsWindow()
        {
            secondWindowService.OpenWindowWith<OptionsPageVM>();
        }

        [RelayCommand]
        public void ToggleState()
        {
            if (!dataStorageFeature.IsAppReady || dataStorageFeature.IsClosingApp) return;

            switch (appStateHandler.AppTimerState)
            {
                case AppState.Working:
                    appStateHandler.AppTimerState = AppState.Resting;
                    break;

                case AppState.Resting:
                    appStateHandler.AppTimerState = AppState.Working;
                    break;
            }
        }

        [RelayCommand]
        public async Task CloseApp()
        {
            //if (dataStorageFeature.IsClosingApp) return;

            //dataStorageFeature.IsClosingApp = true;

            ////CloseSideBar(null, null);

            //await dataStorageFeature.SaveData();

            //Log.Information("------------------App Shuting Down------------------");

            //await Log.CloseAndFlushAsync();

            //Application.Current.Shutdown();
            await Task.Delay(55);
        }

    }
}
