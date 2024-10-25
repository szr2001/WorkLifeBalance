using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.ViewModels
{
    class ForceWorkPageVM : SecondWindowPageVMBase
    {
        public ForceWorkFeature forceWorkFeature;
        public IMainWindowDetailsService mainWindowDetailsService;

        public ForceWorkPageVM(ForceWorkFeature forceWorkFeature, IMainWindowDetailsService mainWindowDetailsService)
        {
            this.forceWorkFeature = forceWorkFeature;
            this.mainWindowDetailsService = mainWindowDetailsService;
            PageHeight = 400;
            PageWidth = 400;
            PageName = "Force Work";
        }
    }
}
