using Serilog;
using System;
using static WorkLifeBalance.Services.TimeHandler;

namespace WorkLifeBalance.Services.Feature
{
    public class ActivityTrackerFeature : FeatureBase
    {
        private static ActivityTrackerFeature? _instance;
        public static ActivityTrackerFeature Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ActivityTrackerFeature();
                }
                return _instance;
            }
        }

        public delegate void ActiveProcess(string ActiveWindow);
        public event ActiveProcess? OnWindowChange;

        public string ActiveWindow = "";

        private TimeSpan OneSec = new TimeSpan(0, 0, 1);

        protected override TickEvent ReturnFeatureMethod()
        {
            return TriggerRecordActivity;
        }

        private void TriggerRecordActivity()
        {
            try
            {
                nint foregroundWindowHandle = LowLevelHandler.GetForegroundWindow();

                ActiveWindow = LowLevelHandler.GetProcessname(foregroundWindowHandle);
                OnWindowChange?.Invoke(ActiveWindow);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to get process of window");
            }

            try
            {
                TimeOnly IncreasedTimeSpan = DataStorageFeature.Instance.AutoChangeData.ActivitiesC[ActiveWindow].Add(OneSec);
                DataStorageFeature.Instance.AutoChangeData.ActivitiesC[ActiveWindow] = IncreasedTimeSpan;
            }
            catch
            {
                DataStorageFeature.Instance.AutoChangeData.ActivitiesC.Add(ActiveWindow, new TimeOnly());
            }
        }
    }
}
