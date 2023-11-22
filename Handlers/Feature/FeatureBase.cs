using System;
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
        protected CancellationTokenSource CancelTokenS = new();
        public TickEvent AddFeature() 
        {
            CancelTokenS = new();
            return ReturnFeatureMethod();
        }

        public TickEvent RemoveFeature() 
        {
            CancelToken();
            return ReturnFeatureMethod();
        }

        protected abstract TickEvent ReturnFeatureMethod();

        private void CancelToken()
        {
            CancelTokenS.Cancel();
            CancelTokenS = new();
        }
    }
}
