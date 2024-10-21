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
        [ObservableProperty]
        public string? dateText;

        [ObservableProperty]
        public TimeOnly elapsedWorkTime;

        [ObservableProperty]
        public TimeOnly elapsedRestTime;

        [ObservableProperty]
        public TimeOnly elapsedIdleTime;

        [ObservableProperty]
        public AppState appState = AppState.Resting;

        [ObservableProperty]
        public bool autoDetectWork;

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

            SubscribeToEvents();
            OnSettingsChanged();
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
            ElapsedIdleTime = dataStorageFeature.TodayData.IdleAmmountC;
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
