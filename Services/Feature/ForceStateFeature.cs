using System;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.Services.Feature
{
    public class ForceStateFeature : FeatureBase
    {
        private readonly IWindowService<MainWindowDetailsPageBase> mainWindowDetailsService;
        private readonly DataStorageFeature dataStorageFeature;
        private readonly IFeaturesServices featuresServices;
        private readonly AppStateHandler appStateHandler;
        public ForceStateFeature(IWindowService<MainWindowDetailsPageBase> mainWindowDetailsService, DataStorageFeature dataStorageFeature, IFeaturesServices featuresServices, AppStateHandler appStateHandler)
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
            mainWindowDetailsService.OpenWith<ForceStateMainMenuDetailsPageVM>();
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
            mainWindowDetailsService.Close();
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
