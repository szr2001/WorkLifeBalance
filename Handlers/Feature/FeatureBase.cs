﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static WorkLifeBalance.Handlers.TimeHandler;

namespace WorkLifeBalance.Handlers.Feature
{
    public abstract class FeatureBase
    {
        //base class for every feature, it cancels the feature delay if feature removed.
        //features use a bool to ignore the main timer event when the feature was triggered
        //if the main timer runs every second and a feature has a 5 minute trigger interval
        //it will run once then ignore the main timer for 5 minutes and repeat.
        protected CancellationTokenSource CancelTokenS = new();

        //used to add the current feature and create a new canceltoken,returns overrided method
        public TickEvent AddFeature() 
        {
            CancelTokenS = new();
            return ReturnFeatureMethod();
        }

        //used to remove the feature, cancels the features and returns the specific method
        //that needs to be removed from main timer
        public TickEvent RemoveFeature() 
        {
            CancelToken();
            return ReturnFeatureMethod();
        }

        //override in childrens to return the feature specific main method
        protected abstract TickEvent ReturnFeatureMethod();

        private void CancelToken()
        {
            CancelTokenS.Cancel();
            CancelTokenS = new();
        }
    }
}
