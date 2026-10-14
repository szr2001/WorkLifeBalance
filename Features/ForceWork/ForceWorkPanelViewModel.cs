using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkLifeBalance.Features.Tracking;
using WorkLifeBalance.Shared.Navigation;
using WorkLifeBalance.Shared.Scheduling;

namespace WorkLifeBalance.Features.ForceWork
{
    public partial class ForceWorkPanelViewModel : MainWindowDetailsPageBase
    {
        private readonly ForceWorkFeature _forceWorkFeature;
        private readonly IWindowService<SecondWindowPageBase> _secondWindowService;
        private readonly IFeaturesService _featuresService;

        [ObservableProperty]
        private AppState _requiredAppState;

        [ObservableProperty]
        private TimeOnly _currentStageTimeRemaining;

        [ObservableProperty]
        private TimeOnly _totalWorkTimeRemaining;

        public ForceWorkPanelViewModel(ForceWorkFeature forceWorkFeature, IWindowService<SecondWindowPageBase> secondWindowService, IFeaturesService featuresService)
        {
            _forceWorkFeature = forceWorkFeature;
            _secondWindowService = secondWindowService;
            _featuresService = featuresService;
        }

        public override Task OnPageOpeningAsync(object? args = null)
        {
            _forceWorkFeature.OnDataUpdated += UpdateDataFromForceWork;
            UpdateDataFromForceWork();
            return Task.CompletedTask;
        }

        public override Task OnPageClosingAsync()
        {
            _forceWorkFeature.OnDataUpdated -= UpdateDataFromForceWork;
            _featuresService.RemoveFeature<ForceWorkFeature>();
            return Task.CompletedTask;
        }

        private void UpdateDataFromForceWork()
        {
            RequiredAppState = _forceWorkFeature.RequiredAppState;
            CurrentStageTimeRemaining = _forceWorkFeature.CurrentStageTimeRemaining;
            TotalWorkTimeRemaining = _forceWorkFeature.TotalWorkTimeRemaining;
        }

        [RelayCommand]
        private void EditForceWork() => _secondWindowService.OpenWith<ForceWorkViewModel>();
    }
}
