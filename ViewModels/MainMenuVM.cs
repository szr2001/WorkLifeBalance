using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.ViewModels.Base;

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

        public bool MinimizeToTray 
        {
            get 
            { 
                return dataStorageFeature.Settings.MinimizeToTrayC;
            } 
        }

        public IWindowService<MainWindowDetailsPageBase> MainWindowDetailsService { get; set; }

        private readonly AppStateHandler appStateHandler;
        private readonly LowLevelHandler lowLevelHandler;
        private readonly DataStorageFeature dataStorageFeature;
        private readonly TimeTrackerFeature timeTrackerFeature;
        private readonly IFeaturesServices featuresServices;
        private readonly IWindowService<SecondWindowPageVMBase> secondWindowService;

        public MainWindowVM(AppTimer mainTimer, LowLevelHandler lowLevelHandler, DataStorageFeature dataStorageFeature, TimeTrackerFeature timeTrackerFeature, IWindowService<SecondWindowPageVMBase> secondWindowService, AppStateHandler appStateHandler, IWindowService<MainWindowDetailsPageBase> mainWindowDetailsService, IFeaturesServices featuresServices)
        {
            this.lowLevelHandler = lowLevelHandler;
            this.dataStorageFeature = dataStorageFeature;
            this.timeTrackerFeature = timeTrackerFeature;
            this.secondWindowService = secondWindowService;
            this.appStateHandler = appStateHandler;
            this.MainWindowDetailsService = mainWindowDetailsService;
            this.featuresServices = featuresServices;

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
        private void ToggleForceState()
        {
            if (featuresServices.IsFeaturePresent<ForceStateFeature>())
            {
                featuresServices.RemoveFeature<ForceStateFeature>();
            }
            else
            {
                featuresServices.AddFeature<ForceStateFeature>();
            }
        }

        [RelayCommand]
        private void OpenViewDataWindow()
        {
            secondWindowService.OpenWith<ViewDataPageVM>();
        }

        [RelayCommand]
        private void OpenOptionsWindow()
        {
            secondWindowService.OpenWith<OptionsPageVM>();
        }

        [RelayCommand]
        private void CloseApp()
        {
            secondWindowService.OpenWith<CloseWarningPageVM>();
        }
    }
}
