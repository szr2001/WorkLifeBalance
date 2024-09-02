using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using static WorkLifeBalance.Services.TimeHandler;

namespace WorkLifeBalance.Services.Feature
{
    public class StateCheckerFeature : FeatureBase
    {
        private static StateCheckerFeature? _instance;
        public static StateCheckerFeature Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StateCheckerFeature();
                }
                return _instance;
            }
        }

        public bool IsFocusingOnWorkingWindow = false;

        protected override TickEvent ReturnFeatureMethod()
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
                await Task.Delay(DataStorageFeature.Instance.Settings.AutoDetectInterval * 1000, CancelTokenS.Token);
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
            if (string.IsNullOrEmpty(ActivityTrackerFeature.Instance.ActiveWindow)) return;

            IsFocusingOnWorkingWindow = DataStorageFeature.Instance.AutoChangeData.WorkingStateWindows.Contains(ActivityTrackerFeature.Instance.ActiveWindow);

            switch (AppTimmerState)
            {
                case AppState.Working:
                    if (!IsFocusingOnWorkingWindow)
                    {
                        MainWindow.instance.SetAppState(AppState.Resting);
                    }
                    break;

                case AppState.Resting:
                    if (IsFocusingOnWorkingWindow)
                    {
                        MainWindow.instance.SetAppState(AppState.Working);
                    }
                    break;

                case AppState.Idle:
                    if (!IsFocusingOnWorkingWindow)
                    {
                        MainWindow.instance.SetAppState(AppState.Resting);
                    }
                    break;
            }
        }
    }
}
