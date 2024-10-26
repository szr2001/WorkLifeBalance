using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.ViewModels
{
    public partial class ForceWorkMainMenuDetailsPageVM : MainWindowDetailsPageBase
    {
        private readonly ForceWorkFeature forceWorkFeature;
        private readonly ISecondWindowService secondWindowService;
        
        public ForceWorkMainMenuDetailsPageVM(ForceWorkFeature forceWorkFeature, ISecondWindowService secondWindowService)
        {
            this.forceWorkFeature = forceWorkFeature;
            this.secondWindowService = secondWindowService;
            forceWorkFeature.OnDataUpdated += UpdateVMData;
        }

        private void UpdateVMData()
        {
        }

        [RelayCommand]
        private void EditForceWork()
        {
            secondWindowService.OpenWindowWith<ForceWorkPageVM>();
        }
    }
}
