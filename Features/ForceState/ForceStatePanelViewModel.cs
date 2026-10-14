using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkLifeBalance.Features.Tracking;
using WorkLifeBalance.Shared.Navigation;
using WorkLifeBalance.Shared.Scheduling;

namespace WorkLifeBalance.Features.ForceState
{
    public partial class ForceStatePanelViewModel : MainWindowDetailsPageBase
    {
        private readonly ForceStateFeature _forceStateFeature;
        private readonly IFeaturesService _featuresService;

        [ObservableProperty]
        private AppState _forcedAppState = AppState.Resting;

        public ForceStatePanelViewModel(ForceStateFeature forceStateFeature, IFeaturesService featuresService)
        {
            _forceStateFeature = forceStateFeature;
            _featuresService = featuresService;
        }

        public override Task OnPageOpeningAsync(object? args = null)
        {
            _forceStateFeature.SetForcedAppState(ForcedAppState);
            return Task.CompletedTask;
        }

        public override Task OnPageClosingAsync()
        {
            _featuresService.RemoveFeature<ForceStateFeature>();
            return Task.CompletedTask;
        }

        partial void OnForcedAppStateChanged(AppState value) => _forceStateFeature.SetForcedAppState(value);

        // Working -> Resting -> Idle -> Working...
        [RelayCommand]
        private void ChangeForcedState()
        {
            var lastState = Enum.GetValues<AppState>().Length - 1;
            ForcedAppState = (int)ForcedAppState == lastState ? 0 : ForcedAppState + 1;
        }
    }
}
