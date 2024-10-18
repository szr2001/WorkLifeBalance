using Serilog;
using System;
using System.Threading.Tasks;

namespace WorkLifeBalance.Services.Feature
{
    public class ActivityTrackerFeature : FeatureBase
    {
        public delegate void ActiveProcess(string ActiveWindow);
        public event ActiveProcess? OnWindowChange;

        public string ActiveWindow = "";

        private readonly TimeSpan OneSec = new (0, 0, 1);

        private readonly LowLevelHandler lowLevelHandler;
        private readonly DataStorageFeature dataStorageFeature;
        public ActivityTrackerFeature(LowLevelHandler lowLevelHandler, DataStorageFeature dataStorageFeature)
        {
            this.lowLevelHandler = lowLevelHandler;
            this.dataStorageFeature = dataStorageFeature;
        }

        protected override Func<Task> ReturnFeatureMethod()
        {
            return TriggerRecordActivity;
        }

        private Task TriggerRecordActivity()
        {
            try
            {
                nint foregroundWindowHandle = lowLevelHandler.ReadForegroundWindow();

                ActiveWindow = lowLevelHandler.GetProcessname(foregroundWindowHandle);
                OnWindowChange?.Invoke(ActiveWindow);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to get process of window");
            }

            try
            {
                TimeOnly IncreasedTimeSpan = dataStorageFeature.AutoChangeData.ActivitiesC[ActiveWindow].Add(OneSec);
                dataStorageFeature.AutoChangeData.ActivitiesC[ActiveWindow] = IncreasedTimeSpan;
            }
            catch
            {
                dataStorageFeature.AutoChangeData.ActivitiesC.Add(ActiveWindow, new TimeOnly());
            }
            return Task.CompletedTask;
        }
    }
}
