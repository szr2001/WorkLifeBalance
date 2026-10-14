using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Scheduling;

namespace WorkLifeBalance.Features.Tracking
{
    // Adds one second to today's worked, rested or idle time on every tick
    public class TimeTrackerFeature : FeatureBase
    {
        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        private readonly DataStorageFeature _dataStorage;
        private readonly AppStateHandler _appStateHandler;

        public TimeTrackerFeature(DataStorageFeature dataStorage, AppStateHandler appStateHandler)
        {
            _dataStorage = dataStorage;
            _appStateHandler = appStateHandler;
        }

        public event Action? OnSpentTimeChange;

        protected override Func<Task> ReturnFeatureMethod() => TriggerUpdateSpentTime;

        private Task TriggerUpdateSpentTime()
        {
            var today = _dataStorage.TodayData;
            switch (_appStateHandler.AppTimerState)
            {
                case AppState.Working:
                    today.WorkedAmmountC = today.WorkedAmmountC.Add(OneSecond);
                    break;
                case AppState.Resting:
                    today.RestedAmmountC = today.RestedAmmountC.Add(OneSecond);
                    break;
                case AppState.Idle:
                    today.IdleAmmountC = today.IdleAmmountC.Add(OneSecond);
                    break;
            }

            OnSpentTimeChange?.Invoke();
            return Task.CompletedTask;
        }
    }
}
