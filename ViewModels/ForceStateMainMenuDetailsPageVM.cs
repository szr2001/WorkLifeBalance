using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.ViewModels
{
    public partial class ForceStateMainMenuDetailsPageVM : MainWindowDetailsPageBase
    {
        private readonly IFeaturesServices featuresServices;
        private readonly ForceStateFeature forceStateFeature;
        public ForceStateMainMenuDetailsPageVM(ForceStateFeature forceStateFeature, IFeaturesServices featuresServices)
        {
            this.forceStateFeature = forceStateFeature;
            this.featuresServices = featuresServices;
        }

        public override Task OnPageClosingAsync()
        {
            featuresServices.RemoveFeature<ForceStateFeature>();
            return base.OnPageClosingAsync();
        }
    }
}
