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
        public ForceStateFeature(IMainWindowDetailsService mainWindowDetailsService, DataStorageFeature dataStorageFeature)
        {
            this.mainWindowDetailsService = mainWindowDetailsService;
            this.dataStorageFeature = dataStorageFeature;
        }

        protected override void OnFeatureAdded()
        {
            dataStorageFeature.Settings.IsForceStateActive = true;
            mainWindowDetailsService.OpenDetailsPageWith<ForceStateMainMenuDetailsPageVM>();
        }

        protected override void OnFeatureRemoved()
        {
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
