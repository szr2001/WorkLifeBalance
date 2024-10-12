using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
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
        public string? elapsedWorkTime;

        [ObservableProperty]
        public string? elapsedRestTime;

        [ObservableProperty]
        public AppState appState;

        [ObservableProperty]
        public bool autoDetectWork;

        private AppTimer mainTimer;
        private LowLevelHandler lowLevelHandler;
        private DataStorageFeature dataStorageFeature;
        private TimeTrackerFeature timeTrackerFeature;
        private ISecondWindowService secondWindowService;
        public MainMenuVM(AppTimer mainTimer, LowLevelHandler lowLevelHandler, DataStorageFeature dataStorageFeature, TimeTrackerFeature timeTrackerFeature, ISecondWindowService secondWindowService)
        {
            this.mainTimer = mainTimer;
            this.lowLevelHandler = lowLevelHandler;
            this.dataStorageFeature = dataStorageFeature;
            this.timeTrackerFeature = timeTrackerFeature;

            mainTimer.OnStateChanges += (newAppState) => { AppState = newAppState; };
            dataStorageFeature.Settings.OnSettingsChanged += () => 
            {
                AutoDetectWork = dataStorageFeature.Settings.AutoDetectWorkingC;
            };
            dataStorageFeature.OnSaving += () => { DateText = $"Saving data..."; };
            dataStorageFeature.OnSaved += () => { DateText = $"Today: {dataStorageFeature.TodayData.DateC.ToString("MM/dd/yyyy")}"; };
            timeTrackerFeature.OnSpentTimeChange += UpdateElapsedTime;
            this.secondWindowService = secondWindowService;
        }

        private void UpdateElapsedTime()
        {
            ElapsedWorkTime = dataStorageFeature.TodayData.WorkedAmmountC.ToString("HH:mm:ss");
            ElapsedRestTime = dataStorageFeature.TodayData.RestedAmmountC.ToString("HH:mm:ss");
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

            switch (mainTimer.AppTimerState)
            {
                case AppState.Working:
                    mainTimer.AppTimerState = AppState.Resting;
                    break;

                case AppState.Resting:
                    mainTimer.AppTimerState = AppState.Working;
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
