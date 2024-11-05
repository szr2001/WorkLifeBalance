using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Services.Feature
{
    public partial class ForceWorkFeature : FeatureBase 
    {
        public Action OnDataUpdated { get; set; } = new(() => { });
        public AppState RequiredAppState { get; private set; } = AppState.Working;
        public string[] Distractions { get; private set; } = Array.Empty<string>();
        public int DistractionsCount { get; private set; }
        public int MaxWarnings { get; private set; } = 3;

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
        private readonly DataStorageFeature dataStorageFeature;
        private readonly IFeaturesServices featuresServices;
        private readonly IMainWindowDetailsService mainWindowDetailsService;
        private readonly ISoundService soundService;
        private readonly string workLifeBalanceProcess = "WorkLifeBalance.exe";
        private readonly string explorerProcess = "explorer.exe";
        private Dictionary<string, int> DistractionApps = new();
 
        private int workIterations;
        private int warnings;
        private bool distractionDetected; 
        private readonly TimeSpan minusOneSecond = new(0,0,-1);
        private int defaultDelay = 6000;
        private int delay;
        public ForceWorkFeature(AppStateHandler appStateHandler, ActivityTrackerFeature activityTrackerFeature, LowLevelHandler lowLevelHandler, IFeaturesServices featuresServices, ISoundService soundService, IMainWindowDetailsService mainWindowDetailsService, DataStorageFeature dataStorageFeature)
        {
            this.appStateHandler = appStateHandler;
            this.activityTrackerFeature = activityTrackerFeature;
            this.lowLevelHandler = lowLevelHandler;
            this.featuresServices = featuresServices;
            this.soundService = soundService;
            this.mainWindowDetailsService = mainWindowDetailsService;
            this.dataStorageFeature = dataStorageFeature;
        }

        public void SetWorkTime(int hours, int minutes, int maxWarnings)
        {
            WorkTimeSetting = new(hours, minutes);
            MaxWarnings = maxWarnings;
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
            //if there is no window set up as working, remove the feature
            if(dataStorageFeature.AutoChangeData.WorkingStateWindows.Length == 0)
            {
                featuresServices.RemoveFeature<ForceWorkFeature>();
                OnDataUpdated.Invoke();
                return;
            }

            //Reset values
            distractionDetected = false;
            TotalWorkTimeRemaining = TotalWorkTimeSetting;
            CurrentStageTimeRemaining = WorkTimeSetting;
            Distractions = Array.Empty<string>();
            DistractionApps.Clear();
            OnDataUpdated.Invoke();
            DistractionsCount = 0;
            workIterations = 0;
            warnings = 0;
            delay = 0;
            mainWindowDetailsService.OpenDetailsPageWith<ForceWorkMainMenuDetailsPageVM>();
        }
        
        protected override void OnFeatureRemoved()
        {
            mainWindowDetailsService.CloseWindow();
        }

        protected override Func<Task> ReturnFeatureMethod()
        {
            return TriggerForceWork;
        }

        private async Task TriggerForceWork()
        {
            if (IsFeatureRuning) return;

            IsFeatureRuning = true;

            await Task.Delay(delay);
            ForceWorkLogic();
            IsFeatureRuning = false;
        }

        private void ForceWorkLogic()
        {
            if(TotalWorkTimeRemaining == TimeOnly.MinValue)
            {
                featuresServices.RemoveFeature<ForceWorkFeature>();
                OnDataUpdated?.Invoke();
                return;
            }

            delay = 0;
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
            if (CurrentStageTimeRemaining == TimeOnly.MinValue)
            {
                workIterations++;
                soundService.PlaySound(ISoundService.SoundType.Finish);
                RequiredAppState = AppState.Resting;
                delay = defaultDelay;

                if (workIterations >= LongRestIntervalSetting)
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

            if (activityTrackerFeature.ActiveWindow == workLifeBalanceProcess ||
                activityTrackerFeature.ActiveWindow == explorerProcess)
            {
                distractionDetected = false;
                warnings = 0;
                return;
            }


            switch (appStateHandler.AppTimerState)
            {
                case AppState.Working:
                    TotalWorkTimeRemaining = TotalWorkTimeRemaining.Add(minusOneSecond);
                    CurrentStageTimeRemaining = CurrentStageTimeRemaining.Add(minusOneSecond);
                    warnings = 0;
                    distractionDetected = false;
                    break;
                case AppState.Resting:
                    //handle when the app is transitioning from resting to working
                    //there is a small time span when the app is in resting but the user is on the working apps
                    if (!dataStorageFeature.AutoChangeData.WorkingStateWindows.Contains(activityTrackerFeature.ActiveWindow))
                    {
                        PunishUser();
                    }
                    break;
                case AppState.Idle:
                    WarnUser();
                    break;
            }
        }

        private void HandleRestingTime()
        {
            if (CurrentStageTimeRemaining == TimeOnly.MinValue)
            {
                RequiredAppState = AppState.Working;
                CurrentStageTimeRemaining = WorkTimeSetting;
                soundService.PlaySound(ISoundService.SoundType.Finish);
                delay = defaultDelay;
                return;
            }

            switch (appStateHandler.AppTimerState)
            {
                case AppState.Working:
                    //handle when the app is transitioning from working to resting
                    //there is a small time span when the app is in working but the user is on the resting apps
                    if (dataStorageFeature.AutoChangeData.WorkingStateWindows.Contains(activityTrackerFeature.ActiveWindow))
                    {
                        PunishUser();
                        return;
                    }
                    break;
                case AppState.Resting:
                    warnings = 0;
                    break;
                case AppState.Idle:
                    WarnUser();
                    break;
            }
            CurrentStageTimeRemaining = CurrentStageTimeRemaining.Add(minusOneSecond);
        }

        private void WarnUser()
        {
            if (RequiredAppState == AppState.Working && distractionDetected == false)
            {
                string currentWindow = activityTrackerFeature.ActiveWindow;
                if (DistractionApps.ContainsKey(currentWindow))
                {
                    DistractionApps[currentWindow]++;
                }
                else
                {
                    DistractionApps.Add(currentWindow, 1);
                }
                DistractionsCount++;
                Distractions = DistractionApps.OrderByDescending(kv => kv.Value).Take(3).Select((pair) => pair.Key).ToArray();
                OnDataUpdated.Invoke();
                distractionDetected = true;
            }

            soundService.PlaySound(ISoundService.SoundType.Warning);
        }

        private void PunishUser()
        {
            if(warnings >= MaxWarnings)
            {
                MinimizeApps();
                warnings = 0;
                return;
            }

            WarnUser();
            warnings++;
        }

        private void MinimizeApps()
        {
            try
            {
                lowLevelHandler.MinimizeAllApps();
                soundService.PlaySound(ISoundService.SoundType.Termination);
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}
