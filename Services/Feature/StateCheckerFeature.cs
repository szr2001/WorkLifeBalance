using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WorkLifeBalance.Services.Feature
{
    public class StateCheckerFeature : FeatureBase
    {
        public bool IsFocusingOnWorkingWindow { get; set; }
        private readonly DataStorageFeature dataStorageFeature;
        private readonly ActivityTrackerFeature activityTrackerFeature;
        private readonly AppStateHandler appStateHandler;
        public StateCheckerFeature(DataStorageFeature dataStorageFeature, ActivityTrackerFeature activityTrackerFeature, AppStateHandler appStateHandler)
        {
            this.dataStorageFeature = dataStorageFeature;
            this.activityTrackerFeature = activityTrackerFeature;
            this.appStateHandler = appStateHandler;
        }

        protected override Func<Task> ReturnFeatureMethod()
        {
            return TriggerWorkDetect;
        }

        private async Task TriggerWorkDetect()
        {
            if (IsFeatureRuning) return;

            try
            {
                IsFeatureRuning = true;
                await Task.Delay(dataStorageFeature.Settings.AutoDetectInterval * 1000, CancelTokenS.Token);
                CheckStateChange();
            }
            catch (TaskCanceledException taskCancel)
            {
                Log.Information($"State Checker: {taskCancel.Message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "State Checker");
            }
            finally
            {
                IsFeatureRuning = false;
            }
        }

        private void CheckStateChange()
        {
            if (string.IsNullOrEmpty(activityTrackerFeature.ActiveWindow)) return;

            IsFocusingOnWorkingWindow = dataStorageFeature.AutoChangeData.WorkingStateWindows.Contains(activityTrackerFeature.ActiveWindow);

            switch (appStateHandler.AppTimerState)
            {
                case AppState.Working:
                    if (!IsFocusingOnWorkingWindow)
                    {
                        appStateHandler.SetAppState(AppState.Resting);
                    }
                    break;

                case AppState.Resting:
                    if (IsFocusingOnWorkingWindow)
                    {
                        appStateHandler.SetAppState(AppState.Working);
                    }
                    break;
                case AppState.Idle:
                    if (IsFocusingOnWorkingWindow)
                    {
                        appStateHandler.SetAppState(AppState.Working);
                    }
                    else
                    {
                        appStateHandler.SetAppState(AppState.Resting);
                    }
                    break;
            }
        }
    }
}
