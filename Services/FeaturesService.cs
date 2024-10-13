using System;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Services
{
    public class FeaturesService : IFeaturesServices
    {
        private readonly Func<Type, FeatureBase> _featureFactory;
        private readonly AppTimer _appTimer;
        public FeaturesService(Func<Type, FeatureBase> featureFactory, AppTimer appTimer)
        {
            _featureFactory = featureFactory;
            _appTimer = appTimer;
        }

        public void AddFeature<T>() where T : FeatureBase
        {
            FeatureBase feature = _featureFactory.Invoke(typeof(T));
            _appTimer.Subscribe(feature.AddFeature());
        }

        public void RemoveFeature<T>() where T : FeatureBase
        {
            FeatureBase feature = _featureFactory.Invoke(typeof(T));
            _appTimer.UnSubscribe(feature.RemoveFeature());
        }
    }
}
