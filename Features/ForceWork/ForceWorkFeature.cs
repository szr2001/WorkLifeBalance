using Serilog;
using WorkLifeBalance.Features.Tracking;
using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Native;
using WorkLifeBalance.Shared.Navigation;
using WorkLifeBalance.Shared.Scheduling;
using WorkLifeBalance.Shared.Sound;

namespace WorkLifeBalance.Features.ForceWork
{
    // Pomodoro: alternates work and rest stages and warns (then minimizes everything) when the user is
    // distracted during work or works during the rest
    public class ForceWorkFeature : FeatureBase
    {
        private const string WorkLifeBalanceProcess = "WorkLifeBalance.exe";
        private const string ExplorerProcess = "explorer.exe";
        private const int StageChangeDelay = 6000;
        private static readonly TimeSpan MinusOneSecond = TimeSpan.FromSeconds(-1);

        private readonly AppStateHandler _appStateHandler;
        private readonly ActivityTrackerFeature _activityTracker;
        private readonly LowLevelHandler _lowLevelHandler;
        private readonly IFeaturesService _featuresService;
        private readonly ISoundService _soundService;
        private readonly IWindowService<MainWindowDetailsPageBase> _detailsService;
        private readonly DataStorageFeature _dataStorage;
        private readonly Dictionary<string, int> _distractionApps = new();

        private int _workIterations;
        private int _warnings;
        private bool _distractionDetected;
        private int _delay;

        public ForceWorkFeature(AppStateHandler appStateHandler, ActivityTrackerFeature activityTracker, LowLevelHandler lowLevelHandler, IFeaturesService featuresService, ISoundService soundService, IWindowService<MainWindowDetailsPageBase> detailsService, DataStorageFeature dataStorage)
        {
            _appStateHandler = appStateHandler;
            _activityTracker = activityTracker;
            _lowLevelHandler = lowLevelHandler;
            _featuresService = featuresService;
            _soundService = soundService;
            _detailsService = detailsService;
            _dataStorage = dataStorage;
        }

        public event Action? OnDataUpdated;

        public AppState RequiredAppState { get; private set; } = AppState.Working;
        public string[] Distractions { get; private set; } = [];
        public int DistractionsCount { get; private set; }
        public int MaxWarnings { get; private set; } = 3;

        public TimeOnly TotalWorkTimeSetting { get; private set; }
        public TimeOnly WorkTimeSetting { get; private set; }
        public TimeOnly RestTimeSetting { get; private set; }
        public TimeOnly LongRestTimeSetting { get; private set; }
        public int LongRestIntervalSetting { get; private set; }

        public TimeOnly TotalWorkTimeRemaining { get; private set; }
        public TimeOnly CurrentStageTimeRemaining { get; private set; }

        private string[] WorkingWindows => _dataStorage.AutoChangeData.WorkingStateWindows;

        public void SetWorkTime(int hours, int minutes, int maxWarnings)
        {
            WorkTimeSetting = new TimeOnly(hours, minutes);
            MaxWarnings = maxWarnings;
        }

        public void SetRestTime(int hours, int minutes) => RestTimeSetting = new TimeOnly(hours, minutes);

        public void SetLongRestTime(int hours, int minutes, int interval)
        {
            LongRestTimeSetting = new TimeOnly(hours, minutes);
            LongRestIntervalSetting = interval;
        }

        public void SetTotalWorkTime(int hours, int minutes) => TotalWorkTimeSetting = new TimeOnly(hours, minutes);

        protected override void OnFeatureAdded()
        {
            // if there is no window set up as working, remove the feature
            if (WorkingWindows.Length == 0)
            {
                _featuresService.RemoveFeature<ForceWorkFeature>();
                OnDataUpdated?.Invoke();
                return;
            }

            _distractionDetected = false;
            TotalWorkTimeRemaining = TotalWorkTimeSetting;
            CurrentStageTimeRemaining = WorkTimeSetting;
            Distractions = [];
            _distractionApps.Clear();
            OnDataUpdated?.Invoke();
            DistractionsCount = 0;
            _workIterations = 0;
            _warnings = 0;
            _delay = 0;
            _detailsService.OpenWith<ForceWorkPanelViewModel>();
        }

        protected override void OnFeatureRemoved() => _detailsService.Close();

        protected override Func<Task> ReturnFeatureMethod() => TriggerForceWork;

        private async Task TriggerForceWork()
        {
            if (IsFeatureRunning)
                return;

            IsFeatureRunning = true;
            await Task.Delay(_delay);
            ForceWorkLogic();
            IsFeatureRunning = false;
        }

        private void ForceWorkLogic()
        {
            if (TotalWorkTimeRemaining == TimeOnly.MinValue)
            {
                _featuresService.RemoveFeature<ForceWorkFeature>();
                OnDataUpdated?.Invoke();
                return;
            }

            _delay = 0;
            switch (RequiredAppState)
            {
                case AppState.Working:
                    HandleWorkingTime();
                    break;
                case AppState.Resting:
                    HandleRestingTime();
                    break;
            }

            OnDataUpdated?.Invoke();
        }

        private void HandleWorkingTime()
        {
            if (CurrentStageTimeRemaining == TimeOnly.MinValue)
            {
                _workIterations++;
                _soundService.PlaySound(SoundType.Finish);
                RequiredAppState = AppState.Resting;
                _delay = StageChangeDelay;

                if (_workIterations >= LongRestIntervalSetting)
                {
                    CurrentStageTimeRemaining = LongRestTimeSetting;
                    _workIterations = 0;
                }
                else
                {
                    CurrentStageTimeRemaining = RestTimeSetting;
                }

                return;
            }

            if (_activityTracker.ActiveWindow is WorkLifeBalanceProcess or ExplorerProcess)
            {
                _distractionDetected = false;
                _warnings = 0;
                return;
            }

            switch (_appStateHandler.AppTimerState)
            {
                case AppState.Working:
                    TotalWorkTimeRemaining = TotalWorkTimeRemaining.Add(MinusOneSecond);
                    CurrentStageTimeRemaining = CurrentStageTimeRemaining.Add(MinusOneSecond);
                    _warnings = 0;
                    _distractionDetected = false;
                    break;
                case AppState.Resting:
                    // handle when the app is transitioning from resting to working
                    // there is a small time span when the app is in resting but the user is on the working apps
                    if (!WorkingWindows.Contains(_activityTracker.ActiveWindow))
                        PunishUser();
                    break;
                case AppState.Idle:
                    WarnUser();
                    break;
            }
        }

        private void HandleRestingTime()
        {
            if (CurrentStageTimeRemaining == TimeOnly.MinValue)
            {
                RequiredAppState = AppState.Working;
                CurrentStageTimeRemaining = WorkTimeSetting;
                _soundService.PlaySound(SoundType.Finish);
                _delay = StageChangeDelay;
                return;
            }

            switch (_appStateHandler.AppTimerState)
            {
                case AppState.Working:
                    // handle when the app is transitioning from working to resting
                    // there is a small time span when the app is in working but the user is on the resting apps
                    if (WorkingWindows.Contains(_activityTracker.ActiveWindow))
                    {
                        PunishUser();
                        return;
                    }
                    break;
                case AppState.Resting:
                    _warnings = 0;
                    break;
                case AppState.Idle:
                    WarnUser();
                    break;
            }

            CurrentStageTimeRemaining = CurrentStageTimeRemaining.Add(MinusOneSecond);
        }

        private void WarnUser()
        {
            if (RequiredAppState == AppState.Working && !_distractionDetected)
            {
                var currentWindow = _activityTracker.ActiveWindow;
                _distractionApps[currentWindow] = _distractionApps.GetValueOrDefault(currentWindow) + 1;
                DistractionsCount++;
                Distractions = _distractionApps.OrderByDescending(pair => pair.Value).Take(3).Select(pair => pair.Key).ToArray();
                OnDataUpdated?.Invoke();
                _distractionDetected = true;
            }

            _soundService.PlaySound(SoundType.Warning);
        }

        private void PunishUser()
        {
            if (_warnings >= MaxWarnings)
            {
                MinimizeApps();
                _warnings = 0;
                return;
            }

            WarnUser();
            _warnings++;
        }

        private void MinimizeApps()
        {
            try
            {
                _lowLevelHandler.MinimizeAllApps();
                _soundService.PlaySound(SoundType.Termination);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to minimize the apps");
            }
        }
    }
}
