using Serilog;
using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
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
        private readonly IFeaturesServices featuresServices;
        private readonly ISoundService soundService;
        private readonly string workLifeBalanceProcess = "WorkLifeBalance.exe";
        private readonly string explorerProcess = "explorer.exe";

        private int workIterations;
        private int maxWarnings = 5;
        private int warnings;

        private readonly TimeSpan minusOneSecond = new(0,0,-1);
        public ForceWorkFeature(AppStateHandler appStateHandler, ActivityTrackerFeature activityTrackerFeature, LowLevelHandler lowLevelHandler, IFeaturesServices featuresServices, ISoundService soundService)
        {
            this.appStateHandler = appStateHandler;
            this.activityTrackerFeature = activityTrackerFeature;
            this.lowLevelHandler = lowLevelHandler;
            this.featuresServices = featuresServices;
            this.soundService = soundService;
        }

        public void SetWorkTime(int hours, int minutes)
        {
            WorkTimeSetting = new(hours, minutes);
        }
        public void SetRestTime(int hours, int minutes)
        {

            RestTimeSetting = new(hours, minutes);
        }
        public void SetLongRestTime(int hours, int minutes, int interval)
        {
            LongRestTimeSetting = new(hours, minutes);
            LongRestIntervalSetting = interval;
        }
        public void SetTotalWorkTime(int hours, int minutes)
        {
            TotalWorkTimeSetting = new(hours, minutes);
        }

        protected override void OnFeatureAdded()
        {
            //create a copy of the settings
            TotalWorkTimeRemaining = TotalWorkTimeSetting;
            CurrentStageTimeRemaining = WorkTimeSetting;
        }

        protected override Func<Task> ReturnFeatureMethod()
        {
            return TriggerForceWork;
        }

        private Task TriggerForceWork()
        {
            ForceWorkLogic();
            return Task.CompletedTask;
        }

        private void ForceWorkLogic()
        {
            if(TotalWorkTimeRemaining == TimeOnly.MinValue)
            {
                featuresServices.RemoveFeature<ForceWorkFeature>();
                return;
            }

            switch (RequiredAppState)
            {
                case AppState.Working:
                    HandleWorkingTime();
                    break;
                case AppState.Resting:
                    HandleRestingTime();
                    break;
            }
            OnDataUpdated?.Invoke();
        }

        private void HandleWorkingTime()
        {
            if (activityTrackerFeature.ActiveWindow == workLifeBalanceProcess ||
                activityTrackerFeature.ActiveWindow == explorerProcess) return;

            switch (appStateHandler.AppTimerState)
            {
                case AppState.Working:
                    if(CurrentStageTimeRemaining == TimeOnly.MinValue)
                    {
                        workIterations++;
                        RequiredAppState = AppState.Resting;

                        if (workIterations == maxWarnings)
                        {
                            CurrentStageTimeRemaining = LongRestTimeSetting;
                            workIterations = 0;
                        }
                        else
                        {
                            CurrentStageTimeRemaining = RestTimeSetting;
                        }
                        return;
                    }
                    TotalWorkTimeRemaining = TotalWorkTimeRemaining.Add(minusOneSecond);
                    CurrentStageTimeRemaining = CurrentStageTimeRemaining.Add(minusOneSecond);
                    warnings = 0;
                    break;
                case AppState.Resting:
                    PunishUser();
                    break;
                case AppState.Idle:
                    WarnUser();
                    break;
            }
        }

        private void WarnUser()
        {
            soundService.PlaySound(ISoundService.SoundType.Warning);
            Console.WriteLine("WARNINGS BEACH");
        }

        private void PunishUser()
        {
            if(warnings == maxWarnings)
            {
                MinimizeForegroundWindow();
                warnings = 0;
                return;
            }

            WarnUser();
            warnings++;
        }

        private void MinimizeForegroundWindow()
        {
            soundService.PlaySound(ISoundService.SoundType.Termination);
        }

        private void HandleRestingTime()
        {
            if (CurrentStageTimeRemaining == TimeOnly.MinValue)
            {
                RequiredAppState = AppState.Working;
                CurrentStageTimeRemaining = WorkTimeSetting;
                return;
            }
            CurrentStageTimeRemaining = CurrentStageTimeRemaining.Add(minusOneSecond);

            switch (appStateHandler.AppTimerState)
            {
                case AppState.Working:
                    PunishUser();
                    break;
                case AppState.Resting:
                    warnings = 0;
                    break;
                case AppState.Idle:
                    WarnUser();
                    break;
            }
        }
    }
}
