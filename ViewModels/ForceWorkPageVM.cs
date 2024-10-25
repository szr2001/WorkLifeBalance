using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private readonly ForceWorkFeature forceWorkFeature;
        private readonly IMainWindowDetailsService mainWindowDetailsService;
        private readonly ISecondWindowService secondWindowService;
        private readonly IFeaturesServices featuresServices;

        public ForceWorkPageVM(ForceWorkFeature forceWorkFeature, IMainWindowDetailsService mainWindowDetailsService, ISecondWindowService secondWindowService, IFeaturesServices featuresServices)
        {
            this.forceWorkFeature = forceWorkFeature;
            this.mainWindowDetailsService = mainWindowDetailsService;
            this.featuresServices = featuresServices;
            this.secondWindowService = secondWindowService;
            PageHeight = 410;
            PageWidth = 400;
            PageName = "Force Work";
        }

        public override Task OnPageOppeningAsync(object? args = null)
        {
            IsFeatureActiv = featuresServices.IsFeaturePresent<ForceWorkFeature>();
            return Task.CompletedTask;
        }

        [RelayCommand]
        private void ReturnToOptions()
        {
            secondWindowService.OpenWindowWith<OptionsPageVM>();
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
                forceWorkFeature.SetWorkTime(WorkHours, WorkMinutes);
                forceWorkFeature.SetRestTime(RestHours, RestMinutes);
                forceWorkFeature.SetTotalWorkTime(TotalWorkHours, TotalWorkMinutes);
                forceWorkFeature.SetLongRestTime(LongRestHours, LongRestMinutes, LongRestInterval);

                featuresServices.AddFeature<ForceWorkFeature>();
                IsFeatureActiv = true;
            }
        }
    }
}
