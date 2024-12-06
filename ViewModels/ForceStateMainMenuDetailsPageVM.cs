using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.ViewModels
{
    public partial class ForceStateMainMenuDetailsPageVM : MainWindowDetailsPageBase
    {
        [ObservableProperty]
        private AppState forcedAppState = AppState.Resting;

        private readonly IFeaturesServices featuresServices;
        private readonly ForceStateFeature forceStateFeature;

        private int AppstatesCount;
        public ForceStateMainMenuDetailsPageVM(ForceStateFeature forceStateFeature, IFeaturesServices featuresServices)
        {
            this.forceStateFeature = forceStateFeature;
            this.featuresServices = featuresServices;
            AppstatesCount = Enum.GetValues(typeof(AppState)).Length - 1;
        }

        partial void OnForcedAppStateChanged(AppState value)
        {
            forceStateFeature.SetForcedAppState(value);
        }

        [RelayCommand]
        private void ChangeForcedState()
        {
            if((int)ForcedAppState == AppstatesCount)
            {
                ForcedAppState = 0;
            }
            else
            {
                ForcedAppState++;
            }
        }

        public override Task OnPageOppeningAsync(object? args = null)
        {

            forceStateFeature.SetForcedAppState(ForcedAppState);
            return base.OnPageOppeningAsync(args);
        }

        public override Task OnPageClosingAsync()
        {
            featuresServices.RemoveFeature<ForceStateFeature>();
            return base.OnPageClosingAsync();
        }
    }
}
