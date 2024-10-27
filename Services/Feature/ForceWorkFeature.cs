using Serilog;
using System;
using System.Numerics;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;

namespace WorkLifeBalance.Services.Feature
{
    public partial class ForceWorkFeature : FeatureBase 
    {
        public Action OnDataUpdated { get; set; } = new(() => { });
        public AppState RequiredAppState { get; private set; } = AppState.Working;
        public string[] Distractions { get; private set; } = new string[3];
        public int DistractionsCount { get; private set; }

        public TimeOnly TotalWorkTimeSetting { get; private set; }
        public TimeOnly WorkTimeSetting { get; private set; }
        public TimeOnly RestTimeSetting { get; private set; }
        public TimeOnly LongRestTimeSetting { get; private set; }
        public int LongRestIntervalSetting { get; private set; }

        public TimeOnly TotalWorkTimeRemaining { get; private set; }
        public TimeOnly CurrentStageTimeRemaining { get; private set; }

        private readonly ActivityTrackerFeature activityTrackerFeature;
        private readonly AppStateHandler appStateHandler;
        private readonly LowLevelHandler lowLevelHandler;

        private int maxWarnings = 5;
        private int warnings;

        private readonly TimeOnly oneSecond = new(0,0,1);
        public ForceWorkFeature(AppStateHandler appStateHandler, ActivityTrackerFeature activityTrackerFeature, LowLevelHandler lowLevelHandler)
        {
            this.appStateHandler = appStateHandler;
            this.activityTrackerFeature = activityTrackerFeature;
            this.lowLevelHandler = lowLevelHandler;
        }

        public void SetWorkTime(int hours, int minutes)
        {
            WorkTimeSetting = new(hours,minutes);
        }
        public void SetRestTime(int hours, int minutes)
        {

            RestTimeSetting = new(hours,minutes);
        }
        public void SetLongRestTime(int hours, int minutes, int interval)
        {
            LongRestTimeSetting = new(hours,minutes);
            LongRestIntervalSetting = interval;
        }
        public void SetTotalWorkTime(int hours, int minutes)
        {
            TotalWorkTimeSetting = new(hours, minutes);
        }

        protected override Func<Task> ReturnFeatureMethod()
        {
            return TriggerForceWork;
        }

        private Task TriggerForceWork()
        {
            CheckIdle();
            return Task.CompletedTask;
        }

        private void CheckIdle()
        {
            switch (appStateHandler.AppTimerState)
            {
                case AppState.Working:
                    break;
                case AppState.Resting:
                    break;
                case AppState.Idle:
                    break;
            }
        }
    }
}
