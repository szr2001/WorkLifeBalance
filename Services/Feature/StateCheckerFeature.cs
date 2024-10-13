using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WorkLifeBalance.Services.Feature
{
    public class StateCheckerFeature : FeatureBase
    {
        public bool IsFocusingOnWorkingWindow = false;
        private readonly DataStorageFeature dataStorageFeature;
        private readonly ActivityTrackerFeature activityTrackerFeature;
        private readonly AppTimer appTimer;
        public StateCheckerFeature(DataStorageFeature dataStorageFeature, ActivityTrackerFeature activityTrackerFeature, AppTimer appTimer)
        {
            this.dataStorageFeature = dataStorageFeature;
            this.activityTrackerFeature = activityTrackerFeature;
            this.appTimer = appTimer;
        }

        protected override Action ReturnFeatureMethod()
        {
            return TriggerWorkDetect;
        }

        private bool IsAutoWorkDetectionTriggered = false;
        private async void TriggerWorkDetect()
        {
            if (IsAutoWorkDetectionTriggered) return;

            IsAutoWorkDetectionTriggered = true;

            try
            {
                await Task.Delay(dataStorageFeature.Settings.AutoDetectInterval * 1000, CancelTokenS.Token);
                CheckStateChange();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "StateCheckerFeature timer loop");
            }
            finally
            {
                IsAutoWorkDetectionTriggered = false;
            }
        }

        private void CheckStateChange()
        {
            if (string.IsNullOrEmpty(activityTrackerFeature.ActiveWindow)) return;

            IsFocusingOnWorkingWindow = dataStorageFeature.AutoChangeData.WorkingStateWindows.Contains(activityTrackerFeature.ActiveWindow);

            switch (appTimer.AppTimerState)
            {
                case AppState.Working:
                    if (!IsFocusingOnWorkingWindow)
                    {
                        appTimer.SetAppState(AppState.Resting);
                    }
                    break;

                case AppState.Resting:
                    if (IsFocusingOnWorkingWindow)
                    {
                        appTimer.SetAppState(AppState.Working);
                    }
                    break;

                case AppState.Idle:
                    if (!IsFocusingOnWorkingWindow)
                    {
                        appTimer.SetAppState(AppState.Resting);
                    }
                    break;
            }
        }
    }
}
