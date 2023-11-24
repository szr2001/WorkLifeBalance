using System;
using static WorkLifeBalance.Handlers.TimeHandler;

namespace WorkLifeBalance.Handlers.Feature
{
    public class TimeTrackerFeature : FeatureBase
    {
        private static TimeTrackerFeature? _instance;
        public static TimeTrackerFeature Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TimeTrackerFeature();
                }
                return _instance;
            }
        }
        
        public delegate void SpentTimeEvent();

        public event SpentTimeEvent? OnSpentTimeChange;

        private TimeSpan OneSec = new TimeSpan(0, 0, 1);

        protected override TickEvent ReturnFeatureMethod()
        {
            return TriggerUpdateSpentTime;
        }

        private void TriggerUpdateSpentTime()
        {
            switch (TimeHandler.AppTimmerState)
            {
                case AppState.Working:
                    DataStorageFeature.Instance.TodayData.WorkedAmmountC = DataStorageFeature.Instance.TodayData.WorkedAmmountC.Add(OneSec);
                    break;

                case AppState.Resting:
                    DataStorageFeature.Instance.TodayData.RestedAmmountC = DataStorageFeature.Instance.TodayData.RestedAmmountC.Add(OneSec);
                    break;

                case AppState.Idle:
                    DataStorageFeature.Instance.TodayData.RestedAmmountC = DataStorageFeature.Instance.TodayData.RestedAmmountC.Add(OneSec);
                    break;
            }
            OnSpentTimeChange?.Invoke();
        }
    }


    public enum AppState
    {
        Working,
        Resting,
        Idle
    }
}
