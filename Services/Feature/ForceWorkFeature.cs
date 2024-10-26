using Serilog;
using System;
using System.Numerics;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;

namespace WorkLifeBalance.Services.Feature
{
    public partial class ForceWorkFeature : FeatureBase 
    {
        public TimeOnly workTime { get; private set; }
        public TimeOnly restTime { get; private set; }
        public TimeOnly totalWorkTime { get; private set; }
        public TimeOnly longRestTime { get; private set; }
        public int ReceievedWarnings { get; private set; }
        public Action OnDataUpdated { get; set; } = new(() => { });

        private readonly ActivityTrackerFeature activityTrackerFeature;
        private readonly AppStateHandler appStateHandler;
        private readonly LowLevelHandler lowLevelHandler;
        private readonly TimeOnly oneSecond = new(0,0,1);
        private int longRestInterval;
        private int maxWarnings = 5;

        private readonly int Delay = 1;
        public ForceWorkFeature(AppStateHandler appStateHandler, ActivityTrackerFeature activityTrackerFeature, LowLevelHandler lowLevelHandler)
        {
            this.appStateHandler = appStateHandler;
            this.activityTrackerFeature = activityTrackerFeature;
            this.lowLevelHandler = lowLevelHandler;
        }

        public void SetWorkTime(int hours, int minutes)
        {
            workTime = new(hours,minutes);
        }
        public void SetRestTime(int hours, int minutes)
        {

            restTime = new(hours,minutes);
        }
        public void SetLongRestTime(int hours, int minutes, int interval)
        {
            longRestTime = new(hours,minutes);
            longRestInterval = interval;
        }
        public void SetTotalWorkTime(int hours, int minutes)
        {
            totalWorkTime = new(hours, minutes);
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

        }
    }
}
