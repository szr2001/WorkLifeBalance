using System;

namespace WorkLifeBalance.Services.Feature
{
    public class TimeTrackerFeature : FeatureBase
    {
        public delegate void SpentTimeEvent();
        public event SpentTimeEvent? OnSpentTimeChange;

        private readonly TimeSpan OneSec = new (0, 0, 1);
        private readonly AppStateHandler appStateHandler;
        private readonly DataStorageFeature dataStorageFeature;
        public TimeTrackerFeature(DataStorageFeature dataStorageFeature, AppStateHandler appStateHandler)
        {
            this.dataStorageFeature = dataStorageFeature;
            this.appStateHandler = appStateHandler;
        }
        protected override Action ReturnFeatureMethod()
        {
            return TriggerUpdateSpentTime;
        }

        private void TriggerUpdateSpentTime()
        {
            switch (appStateHandler.AppTimerState)
            {
                case AppState.Working:
                    dataStorageFeature.TodayData.WorkedAmmountC = dataStorageFeature.TodayData.WorkedAmmountC.Add(OneSec);
                    break;

                case AppState.Resting:
                    dataStorageFeature.TodayData.RestedAmmountC = dataStorageFeature.TodayData.RestedAmmountC.Add(OneSec);
                    break;

                case AppState.Idle:
                    dataStorageFeature.TodayData.IdleAmmountC = dataStorageFeature.TodayData.IdleAmmountC.Add(OneSec);
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
