using WorkLifeBalance.Features.Tracking;
using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Navigation;
using WorkLifeBalance.Shared.Scheduling;

namespace WorkLifeBalance.Features.ForceState
{
    // Turns the automatic detection off and lets the user pick the state by hand
    public class ForceStateFeature : FeatureBase
    {
        private readonly IWindowService<MainWindowDetailsPageBase> _detailsService;
        private readonly DataStorageFeature _dataStorage;
        private readonly IFeaturesService _featuresService;
        private readonly AppStateHandler _appStateHandler;

        public ForceStateFeature(IWindowService<MainWindowDetailsPageBase> detailsService, DataStorageFeature dataStorage, IFeaturesService featuresService, AppStateHandler appStateHandler)
        {
            _detailsService = detailsService;
            _dataStorage = dataStorage;
            _featuresService = featuresService;
            _appStateHandler = appStateHandler;
        }

        public void SetForcedAppState(AppState state) => _appStateHandler.SetAppState(state);

        protected override void OnFeatureAdded()
        {
            _featuresService.RemoveFeature<IdleCheckerFeature>();
            _featuresService.RemoveFeature<StateCheckerFeature>();

            _dataStorage.Settings.IsForceStateActive = true;
            _detailsService.OpenWith<ForceStatePanelViewModel>();
        }

        protected override void OnFeatureRemoved()
        {
            _featuresService.AddFeature<IdleCheckerFeature>();
            _featuresService.AddFeature<StateCheckerFeature>();

            _dataStorage.Settings.IsForceStateActive = false;
            _detailsService.Close();
        }

        protected override Func<Task> ReturnFeatureMethod() => ForceStateMethod;

        private Task ForceStateMethod() => Task.CompletedTask;
    }
}
