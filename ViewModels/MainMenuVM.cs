using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Threading.Tasks;
using System.Windows;
using WorkLifeBalance.Services;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.ViewModels
{
    public partial class MainMenuVM : ObservableObject
    {
        [ObservableProperty]
        private string? dateText;

        [ObservableProperty]
        private string? elapsedWorkTime;

        [ObservableProperty]
        private string? elapsedRestTime;

        [ObservableProperty]
        private AppState appState;

        private AppTimer mainTimer;
        private LowLevelHandler lowLevelHandler;
        private DataStorageFeature dataStorageFeature;
        private TimeTrackerFeature timeTrackerFeature;
        public MainMenuVM(AppTimer mainTimer, LowLevelHandler lowLevelHandler, DataStorageFeature dataStorageFeature, TimeTrackerFeature timeTrackerFeature)
        {
            this.mainTimer = mainTimer;
            this.lowLevelHandler = lowLevelHandler;
            this.dataStorageFeature = dataStorageFeature;
            this.timeTrackerFeature = timeTrackerFeature;

            mainTimer.OnStateChanges += (newAppState) => { AppState = newAppState; };
            dataStorageFeature.OnSaving += () => { DateText = $"Saving data..."; };
            dataStorageFeature.OnSaved += () => { DateText = $"Today: {dataStorageFeature.TodayData.DateC.ToString("MM/dd/yyyy")}"; };
            timeTrackerFeature.OnSpentTimeChange += UpdateElapsedTime;
        }

        private void UpdateElapsedTime()
        {
            ElapsedWorkTime = dataStorageFeature.TodayData.WorkedAmmountC.ToString("HH:mm:ss");
            ElapsedRestTime = dataStorageFeature.TodayData.RestedAmmountC.ToString("HH:mm:ss");
        }

        [RelayCommand]
        public void OpenViewDataWindow()
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.ViewData);
        }

        [RelayCommand]
        public void OpenOptionsWindow()
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.Options);
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
            if (dataStorageFeature.IsClosingApp) return;

            dataStorageFeature.IsClosingApp = true;

            //CloseSideBar(null, null);

            await dataStorageFeature.SaveData();

            Log.Information("------------------App Shuting Down------------------");

            await Log.CloseAndFlushAsync();

            Application.Current.Shutdown();
        }

    }
}
