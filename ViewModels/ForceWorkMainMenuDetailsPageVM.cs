using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.ViewModels
{
    public partial class ForceWorkMainMenuDetailsPageVM : MainWindowDetailsPageBase
    {

        [ObservableProperty]
        private AppState requiredAppState;

        [ObservableProperty]
        private TimeOnly currentStageTimeRemaining;

        [ObservableProperty]
        private TimeOnly totalWorkTimeRemaining;

        private readonly ForceWorkFeature forceWorkFeature;
        private readonly IWindowService<SecondWindowPageVMBase> secondWindowService;
        private readonly IFeaturesServices featuresServices;

        public ForceWorkMainMenuDetailsPageVM(ForceWorkFeature forceWorkFeature, IWindowService<SecondWindowPageVMBase> secondWindowService, IFeaturesServices featuresServices)
        {
            this.forceWorkFeature = forceWorkFeature;
            this.secondWindowService = secondWindowService;
            this.featuresServices = featuresServices;
        }

        public override Task OnPageOpeningAsync(object? args = null)
        {
            forceWorkFeature.OnDataUpdated += UpdateDataFromForceWork;
            UpdateDataFromForceWork();
            return Task.CompletedTask;
        }

        public override Task OnPageClosingAsync()
        {
            forceWorkFeature.OnDataUpdated -= UpdateDataFromForceWork;
            featuresServices.RemoveFeature<ForceWorkFeature>();
            return Task.CompletedTask;
        }

        private void UpdateDataFromForceWork()
        {
            RequiredAppState = forceWorkFeature.RequiredAppState;
            CurrentStageTimeRemaining = forceWorkFeature.CurrentStageTimeRemaining;
            TotalWorkTimeRemaining = forceWorkFeature.TotalWorkTimeRemaining;
        }

        [RelayCommand]
        private void EditForceWork()
        {
            secondWindowService.OpenWith<ForceWorkPageVM>();
        }
    }
}
