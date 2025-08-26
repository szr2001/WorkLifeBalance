using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.ViewModels
{
    public partial class ForceWorkPageVM : SecondWindowPageVMBase
    {
        [ObservableProperty]
        private int[] hours = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
        
        [ObservableProperty]
        private int[] minutes = {0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55};

        [ObservableProperty]
        private int totalWorkHours = 2;

        [ObservableProperty]
        private int totalWorkMinutes;

        [ObservableProperty]
        private int workHours;

        [ObservableProperty]
        private int workMinutes = 25;

        [ObservableProperty]
        private int restHours;

        [ObservableProperty]
        private int restMinutes = 5;

        [ObservableProperty]
        private int longRestHours;

        [ObservableProperty]
        private int longRestMinutes = 25;

        [ObservableProperty]
        private int longRestInterval = 4;

        [ObservableProperty]
        private bool isFeatureActiv;

        [ObservableProperty]
        private int maxWarnings = 3;

        [ObservableProperty]
        private TimeOnly totalWorkTimeSetting;
        [ObservableProperty]
        private TimeOnly workTimeSetting;
        [ObservableProperty]
        private TimeOnly restTimeSetting;
        [ObservableProperty]
        private TimeOnly longRestTimeSetting;
        [ObservableProperty]
        private int longRestIntervalSetting;

        [ObservableProperty]
        private int distractionCount;

        [ObservableProperty]
        private string[] distractions = { "Process.exe", "Process.exe", "Process.exe" };

        private readonly ForceWorkFeature forceWorkFeature;
        private readonly IWindowService<SecondWindowPageVMBase> secondWindowService;
        private readonly IFeaturesServices featuresServices;

        public ForceWorkPageVM(ForceWorkFeature forceWorkFeature, IWindowService<SecondWindowPageVMBase> secondWindowService, IFeaturesServices featuresServices)
        {
            this.forceWorkFeature = forceWorkFeature;
            this.featuresServices = featuresServices;
            this.secondWindowService = secondWindowService;
            PageHeight = 410;
            PageWidth = 400;
            PageName = "Force Work";
        }

        //maybe use observable pattern for properties instead of one onUpdate event
        private void UpdateDataFromForceWork()
        {
            Distractions = forceWorkFeature.Distractions;
            DistractionCount = forceWorkFeature.DistractionsCount;
            IsFeatureActiv = featuresServices.IsFeaturePresent<ForceWorkFeature>();
        }

        private void GetForceWorkSettings()
        {
            TotalWorkTimeSetting = forceWorkFeature.TotalWorkTimeSetting;
            WorkTimeSetting = forceWorkFeature.WorkTimeSetting;
            RestTimeSetting = forceWorkFeature.RestTimeSetting;
            LongRestTimeSetting = forceWorkFeature.LongRestTimeSetting;
            LongRestIntervalSetting = forceWorkFeature.LongRestIntervalSetting;
        }

        public override Task OnPageOpeningAsync(object? args = null)
        {
            IsFeatureActiv = featuresServices.IsFeaturePresent<ForceWorkFeature>();
            forceWorkFeature.OnDataUpdated += UpdateDataFromForceWork;
            return Task.CompletedTask;
        }

        public override Task OnPageClosingAsync()
        {
            forceWorkFeature.OnDataUpdated -= UpdateDataFromForceWork;
            return Task.CompletedTask;
        }

        [RelayCommand]
        private void ReturnToOptions()
        {
            secondWindowService.OpenWith<OptionsPageVM>();
        }

        [RelayCommand]
        private void ToggleForceWork()
        {
            if (IsFeatureActiv)
            {
                featuresServices.RemoveFeature<ForceWorkFeature>();
                IsFeatureActiv = false;
            }
            else
            {
                forceWorkFeature.SetWorkTime(WorkHours, WorkMinutes, MaxWarnings);
                forceWorkFeature.SetRestTime(RestHours, RestMinutes);
                forceWorkFeature.SetTotalWorkTime(TotalWorkHours, TotalWorkMinutes);
                forceWorkFeature.SetLongRestTime(LongRestHours, LongRestMinutes, LongRestInterval);

                GetForceWorkSettings();
                featuresServices.AddFeature<ForceWorkFeature>();
                IsFeatureActiv = true;
            }
        }
    }
}
