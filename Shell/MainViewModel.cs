using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkLifeBalance.Features.CloseApp;
using WorkLifeBalance.Features.ForceState;
using WorkLifeBalance.Features.Options;
using WorkLifeBalance.Features.Statistics;
using WorkLifeBalance.Features.Tracking;
using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Navigation;
using WorkLifeBalance.Shared.Scheduling;

namespace WorkLifeBalance.Shell
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DataStorageFeature _dataStorage;
        private readonly IFeaturesService _featuresService;
        private readonly IWindowService<SecondWindowPageBase> _secondWindowService;

        [ObservableProperty]
        private string? _dateText;

        [ObservableProperty]
        private TimeOnly _elapsedWorkTime;

        [ObservableProperty]
        private TimeOnly _elapsedRestTime;

        [ObservableProperty]
        private TimeOnly _elapsedIdleTime;

        [ObservableProperty]
        private AppState _appState = AppState.Resting;

        public MainViewModel(DataStorageFeature dataStorage, TimeTrackerFeature timeTracker, IWindowService<SecondWindowPageBase> secondWindowService, AppStateHandler appStateHandler, IWindowService<MainWindowDetailsPageBase> detailsService, IFeaturesService featuresService)
        {
            _dataStorage = dataStorage;
            _featuresService = featuresService;
            _secondWindowService = secondWindowService;
            MainWindowDetailsService = detailsService;

            ShowToday();
            appStateHandler.OnStateChanges += state => AppState = state;
            dataStorage.OnSaving += () => DateText = "Saving data...";
            dataStorage.OnSaved += ShowToday;
            timeTracker.OnSpentTimeChange += OnTimeSpentChanged;
        }

        public IWindowService<MainWindowDetailsPageBase> MainWindowDetailsService { get; }

        public bool MinimizeToTray => _dataStorage.Settings.MinimizeToTrayC;

        private void ShowToday() => DateText = $"Today: {_dataStorage.TodayData.DateC:MM/dd/yyyy}";

        private void OnTimeSpentChanged()
        {
            ElapsedWorkTime = _dataStorage.TodayData.WorkedAmmountC;
            ElapsedRestTime = _dataStorage.TodayData.RestedAmmountC;
            ElapsedIdleTime = _dataStorage.TodayData.IdleAmmountC;
        }

        [RelayCommand]
        private void ToggleForceState()
        {
            if (_featuresService.IsFeaturePresent<ForceStateFeature>())
                _featuresService.RemoveFeature<ForceStateFeature>();
            else
                _featuresService.AddFeature<ForceStateFeature>();
        }

        [RelayCommand]
        private void OpenViewDataWindow() => _secondWindowService.OpenWith<StatisticsViewModel>();

        [RelayCommand]
        private void OpenOptionsWindow() => _secondWindowService.OpenWith<OptionsViewModel>();

        [RelayCommand]
        private void CloseApp() => _secondWindowService.OpenWith<CloseWarningViewModel>();
    }
}
