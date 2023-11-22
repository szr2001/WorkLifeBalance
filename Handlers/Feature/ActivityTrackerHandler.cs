using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WorkLifeBalance.Handlers.TimeHandler;

namespace WorkLifeBalance.Handlers.Feature
{
    public class ActivityTrackerHandler : FeatureBase
    {
        private static ActivityTrackerHandler? _instance;
        public static ActivityTrackerHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ActivityTrackerHandler();
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
                IntPtr foregroundWindowHandle = LowLevelHandler.GetForegroundWindow();

                ActiveWindow = LowLevelHandler.GetProcessname(foregroundWindowHandle);
                OnWindowChange?.Invoke(ActiveWindow);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                TimeOnly IncreasedTimeSpan = DataHandler.Instance.AutoChangeData.ActivitiesC[ActiveWindow].Add(OneSec);
                DataHandler.Instance.AutoChangeData.ActivitiesC[ActiveWindow] = IncreasedTimeSpan;
            }
            catch
            {
                DataHandler.Instance.AutoChangeData.ActivitiesC.Add(ActiveWindow, new TimeOnly());
            }
        }
    }
}
