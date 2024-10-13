using System;

namespace WorkLifeBalance.Services.Feature
{
    public class TimeTrackerFeature : FeatureBase
    {
        public delegate void SpentTimeEvent();
        public event SpentTimeEvent? OnSpentTimeChange;

        private TimeSpan OneSec = new (0, 0, 1);
        private AppTimer appTimer;
        private DataStorageFeature dataStorageFeature;
        public TimeTrackerFeature(AppTimer appTimer, DataStorageFeature dataStorageFeature)
        {
            this.appTimer = appTimer;
            this.dataStorageFeature = dataStorageFeature;
        }
        protected override Action ReturnFeatureMethod()
        {
            return TriggerUpdateSpentTime;
        }

        private void TriggerUpdateSpentTime()
        {
            switch (appTimer.AppTimerState)
            {
                case AppState.Working:
                    dataStorageFeature.TodayData.WorkedAmmountC = dataStorageFeature.TodayData.WorkedAmmountC.Add(OneSec);
                    break;

                case AppState.Resting:
                    dataStorageFeature.TodayData.RestedAmmountC = dataStorageFeature.TodayData.RestedAmmountC.Add(OneSec);
                    break;

                case AppState.Idle:
                    dataStorageFeature.TodayData.RestedAmmountC = dataStorageFeature.TodayData.RestedAmmountC.Add(OneSec);
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
