using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.ViewModels
{
    public partial class ForceStateMainMenuDetailsPageVM : MainWindowDetailsPageBase
    {
        private readonly ForceStateFeature forceStateFeature;
        public ForceStateMainMenuDetailsPageVM(ForceStateFeature forceStateFeature)
        {
            this.forceStateFeature = forceStateFeature;
        }
    }
}
