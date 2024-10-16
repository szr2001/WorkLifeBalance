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

        public void AddFeature<TFeature>() where TFeature : FeatureBase
        {
            FeatureBase feature = _featureFactory.Invoke(typeof(TFeature));
            _appTimer.Subscribe(feature.AddFeature());
        }

        public void RemoveFeature<TFeature>() where TFeature : FeatureBase
        {
            FeatureBase feature = _featureFactory.Invoke(typeof(TFeature));
            _appTimer.UnSubscribe(feature.RemoveFeature());
        }
    }
}
