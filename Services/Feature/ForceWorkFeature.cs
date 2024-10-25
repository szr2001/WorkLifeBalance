using Serilog;
using System;
using System.Numerics;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;

namespace WorkLifeBalance.Services.Feature
{
    public partial class ForceWorkFeature : FeatureBase 
    {
        private readonly AppStateHandler appStateHandler;
        private readonly ActivityTrackerFeature activityTrackerFeature;
        private readonly LowLevelHandler lowLevelHandler;
        private TimeOnly workTime;
        private TimeOnly restTime;
        private TimeOnly totalWorkTime;
        private TimeOnly longRestTime;
        private int longRestInterval;

        private readonly int Delay = 1;
        public ForceWorkFeature(AppStateHandler appStateHandler, ActivityTrackerFeature activityTrackerFeature, LowLevelHandler lowLevelHandler)
        {
            this.appStateHandler = appStateHandler;
            this.activityTrackerFeature = activityTrackerFeature;
            this.lowLevelHandler = lowLevelHandler;
        }

        public void SetWorkTime(int hours, int minutes)
        {

        }
        public void SetRestTime(int hours, int minutes)
        {

        }
        public void SetLongRestTime(int hours, int minutes, int interval)
        {

        }
        public void SetTotalWorkTime(int hours, int minutes)
        {

        }

        protected override Func<Task> ReturnFeatureMethod()
        {
            return TriggerForceWork;
        }
        private async Task TriggerForceWork()
        {
            if (IsFeatureRuning) return;

            try
            {
                IsFeatureRuning = true;

                await Task.Delay(Delay, CancelTokenS.Token);
                CheckIdle();
            }
            catch (TaskCanceledException taskCancel)
            {
                Log.Information($"Idle Checker: {taskCancel.Message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Idle Checker");
            }
            finally
            {
                IsFeatureRuning = false;
            }
        }

        private void CheckIdle()
        {

        }
    }
}
