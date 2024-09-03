using System;
using static WorkLifeBalance.Services.AppTimer;

namespace WorkLifeBalance.Services.Feature
{
    public class TimeTrackerFeature : FeatureBase
    {
        public delegate void SpentTimeEvent();

        public event SpentTimeEvent? OnSpentTimeChange;

        private TimeSpan OneSec = new TimeSpan(0, 0, 1);

        protected override TickEvent ReturnFeatureMethod()
        {
            return TriggerUpdateSpentTime;
        }

        private void TriggerUpdateSpentTime()
        {
            switch (AppTimmerState)
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
