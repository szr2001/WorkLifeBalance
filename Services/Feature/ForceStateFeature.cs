using System;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Services.Feature
{
    public class ForceStateFeature : FeatureBase
    {
        private readonly IMainWindowDetailsService mainWindowDetailsService;
        private readonly DataStorageFeature dataStorageFeature;
        private readonly IFeaturesServices featuresServices;
        private readonly AppStateHandler appStateHandler;
        public ForceStateFeature(IMainWindowDetailsService mainWindowDetailsService, DataStorageFeature dataStorageFeature, IFeaturesServices featuresServices, AppStateHandler appStateHandler)
        {
            this.mainWindowDetailsService = mainWindowDetailsService;
            this.dataStorageFeature = dataStorageFeature;
            this.featuresServices = featuresServices;
            this.appStateHandler = appStateHandler;
        }

        protected override void OnFeatureAdded()
        {
            featuresServices.RemoveFeature<IdleCheckerFeature>();
            featuresServices.RemoveFeature<StateCheckerFeature>();
            
            dataStorageFeature.Settings.IsForceStateActive = true;
            mainWindowDetailsService.OpenDetailsPageWith<ForceStateMainMenuDetailsPageVM>();
        }

        public void SetForcedAppState(AppState state)
        {
            appStateHandler.SetAppState(state);
        }

        protected override void OnFeatureRemoved()
        {
            featuresServices.AddFeature<IdleCheckerFeature>();
            featuresServices.AddFeature<StateCheckerFeature>();

            dataStorageFeature.Settings.IsForceStateActive = false;
            mainWindowDetailsService.CloseWindow();
        }

        protected override Func<Task> ReturnFeatureMethod()
        {
            return ForceStateMethod;
        }

        private Task ForceStateMethod()
        {
            return Task.CompletedTask;
        }
    }
}
