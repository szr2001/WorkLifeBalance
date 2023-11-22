using System;
using System.Linq;
using System.Threading.Tasks;
using WorkLifeBalance.Handlers;
using static WorkLifeBalance.Handlers.TimeHandler;

namespace WorkLifeBalance.Handlers.Feature
{
    public class StateChangerHandler : FeatureBase
    {
        private static StateChangerHandler? _instance;
        public static StateChangerHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StateChangerHandler();
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
                await Task.Delay(DataHandler.Instance.Settings.AutoDetectInterval * 1000,CancelTokenS.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            CheckStateChange();

            IsAutoWorkDetectionTriggered = false;
        }

        private void CheckStateChange()
        {
            if (string.IsNullOrEmpty(ActivityTrackerHandler.Instance.ActiveWindow)) return;

            IsFocusingOnWorkingWindow = DataHandler.Instance.AutoChangeData.WorkingStateWindows.Contains(ActivityTrackerHandler.Instance.ActiveWindow);

            switch (TimeHandler.Instance.AppTimmerState)
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
                    if(!IsFocusingOnWorkingWindow)
                    {
                        MainWindow.instance.SetAppState(AppState.Resting);
                    }
                    break;
            }
        }
    }
}
