using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.ViewModels
{
    public partial class MainWindowVM : ObservableObject
    {
        [ObservableProperty]
        private string? dateText;

        [ObservableProperty]
        private TimeOnly elapsedWorkTime;

        [ObservableProperty]
        private TimeOnly elapsedRestTime;

        [ObservableProperty]
        private TimeOnly elapsedIdleTime;

        [ObservableProperty]
        private AppState appState = AppState.Resting;

        public IMainWindowDetailsService MainWindowDetailsService { get; set; }

        private readonly AppStateHandler appStateHandler;
        private readonly LowLevelHandler lowLevelHandler;
        private readonly DataStorageFeature dataStorageFeature;
        private readonly TimeTrackerFeature timeTrackerFeature;
        private readonly ISecondWindowService secondWindowService;

        public MainWindowVM(AppTimer mainTimer, LowLevelHandler lowLevelHandler, DataStorageFeature dataStorageFeature, TimeTrackerFeature timeTrackerFeature, ISecondWindowService secondWindowService, AppStateHandler appStateHandler, IMainWindowDetailsService mainWindowDetailsService)
        {
            this.lowLevelHandler = lowLevelHandler;
            this.dataStorageFeature = dataStorageFeature;
            this.timeTrackerFeature = timeTrackerFeature;
            this.secondWindowService = secondWindowService;
            this.appStateHandler = appStateHandler;
            this.MainWindowDetailsService = mainWindowDetailsService;

            DateText = $"Today: {dataStorageFeature.TodayData.DateC:MM/dd/yyyy}";

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            appStateHandler.OnStateChanges += OnStateChanged;

            dataStorageFeature.OnSaving += OnSavingData;
            dataStorageFeature.OnSaved += OnDataSaved;

            timeTrackerFeature.OnSpentTimeChange += OnTimeSpentChanged;
        }

        private void OnStateChanged(AppState state)
        {
            AppState = state;
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
        public void CloseApp()
        {
            if (dataStorageFeature.IsClosingApp) return;

            dataStorageFeature.IsClosingApp = true;

            Log.Information("------------------App Shuting Down------------------");
            
            _ = Task.Run(async () => 
            {
                await dataStorageFeature.SaveData();
                await Log.CloseAndFlushAsync();

                App.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            });
        }
    }
}
