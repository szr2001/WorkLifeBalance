using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkLifeBalance.Services.Feature
{
    partial class ForceWorkFeature : FeatureBase 
    {
        private readonly AppStateHandler appStateHandler;
        private readonly ActivityTrackerFeature activityTrackerFeature;
        public ForceWorkFeature(AppStateHandler appStateHandler, ActivityTrackerFeature activityTrackerFeature)
        {
            this.appStateHandler = appStateHandler;
            this.activityTrackerFeature = activityTrackerFeature;
        }

        protected override Func<Task> ReturnFeatureMethod()
        {
            throw new NotImplementedException();
        }
    }
}
