using System;

namespace WorkLifeBalance.Handlers
{
    public class TimeTrackerHandler
    {
        private static TimeTrackerHandler? _instance;
        public static TimeTrackerHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TimeTrackerHandler();
                }
                return _instance;
            }
        }
        
        public delegate void SpentTimeEvent();

        public event SpentTimeEvent? OnSpentTimeChange;

        private TimeSpan OneSec = new TimeSpan(0, 0, 1);

        public void TriggerUpdateSpentTime()
        {
            switch (TimeHandler.Instance.AppTimmerState)
            {
                case AppState.Working:
                    DataHandler.Instance.TodayData.WorkedAmmountC = DataHandler.Instance.TodayData.WorkedAmmountC.Add(OneSec);
                    break;

                case AppState.Resting:
                    DataHandler.Instance.TodayData.RestedAmmountC = DataHandler.Instance.TodayData.RestedAmmountC.Add(OneSec);
                    break;

                case AppState.Idle:
                    DataHandler.Instance.TodayData.RestedAmmountC = DataHandler.Instance.TodayData.RestedAmmountC.Add(OneSec);
                    break;
            }
            OnSpentTimeChange?.Invoke();

            Console.WriteLine(TimeHandler.Instance.AppTimmerState);
        }
    }


    public enum AppState
    {
        Working,
        Resting,
        Idle
    }
}
