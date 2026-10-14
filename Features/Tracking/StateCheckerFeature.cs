using Serilog;
using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Scheduling;

namespace WorkLifeBalance.Features.Tracking
{
    // Switches between Working and Resting depending on whether the focused window or page is marked as working
    public class StateCheckerFeature : FeatureBase
    {
        private readonly DataStorageFeature _dataStorage;
        private readonly ActivityTrackerFeature _activityTracker;
        private readonly AppStateHandler _appStateHandler;

        public StateCheckerFeature(DataStorageFeature dataStorage, ActivityTrackerFeature activityTracker, AppStateHandler appStateHandler)
        {
            _dataStorage = dataStorage;
            _activityTracker = activityTracker;
            _appStateHandler = appStateHandler;
        }

        public bool IsFocusingOnWorkingWindow { get; set; }
        public bool IsFocusingOnWorkingPage { get; set; }

        protected override Func<Task> ReturnFeatureMethod() => TriggerWorkDetect;

        private async Task TriggerWorkDetect()
        {
            if (IsFeatureRunning)
                return;

            try
            {
                IsFeatureRunning = true;
                await Task.Delay(_dataStorage.Settings.AutoDetectInterval * 1000, CancelTokenSource.Token);
                CheckStateChange();
            }
            catch (TaskCanceledException taskCancel)
            {
                Log.Information("State Checker: {Message}", taskCancel.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "State Checker");
            }
            finally
            {
                IsFeatureRunning = false;
            }
        }

        private void CheckStateChange()
        {
            if (string.IsNullOrEmpty(_activityTracker.ActiveWindow))
                return;

            IsFocusingOnWorkingWindow = _dataStorage.AutoChangeData.WorkingStateWindows.Contains(_activityTracker.ActiveWindow);
            IsFocusingOnWorkingPage = _dataStorage.AutoChangeData.WorkingStateUrls.Contains(_activityTracker.ActiveUrl);

            switch (_appStateHandler.AppTimerState)
            {
                case AppState.Working:
                    if (!IsFocusingOnWorkingWindow && !IsFocusingOnWorkingPage)
                        _appStateHandler.SetAppState(AppState.Resting);
                    break;
                case AppState.Resting:
                    if (IsFocusingOnWorkingWindow || IsFocusingOnWorkingPage)
                        _appStateHandler.SetAppState(AppState.Working);
                    break;
                case AppState.Idle:
                    _appStateHandler.SetAppState(IsFocusingOnWorkingWindow ? AppState.Working : AppState.Resting);
                    break;
            }
        }
    }
}
