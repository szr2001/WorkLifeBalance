using System;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;

namespace WorkLifeBalance.Services.Feature
{
    public abstract class FeatureBase
    {
        //base class for every feature, it cancels the feature delay if feature removed.
        //features use a bool to ignore the main timer event when the feature was triggered
        //if the main timer runs every second and a feature has a 5 minute trigger interval
        //it will run once then ignore the main timer for 5 minutes and repeat.
        protected CancellationTokenSource CancelTokenS = new();

        public bool IsFeatureRuning = false;

        //used to add the current feature and create a new canceltoken,returns overrided method
        public Func<Task> AddFeature()
        {
            IsFeatureRuning = false;
            CancelTokenS = new();
            OnFeatureAdded();
            return ReturnFeatureMethod();
        }

        //used to remove the feature, cancels the features and returns the specific method
        //that needs to be removed from main timer
        public Func<Task> RemoveFeature()
        {
            CancelToken();
            OnFeatureRemoved();
            return ReturnFeatureMethod();
        }

        //override in childrens to return the feature specific main method
        protected abstract Func<Task> ReturnFeatureMethod();

        protected virtual void OnFeatureAdded() { }
        protected virtual void OnFeatureRemoved() { }

        private void CancelToken()
        {
            CancelTokenS.Cancel();
            CancelTokenS = new();
        }
    }
}
