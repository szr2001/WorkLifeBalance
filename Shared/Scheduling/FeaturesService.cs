namespace WorkLifeBalance.Shared.Scheduling
{
    public class FeaturesService : IFeaturesService
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
            if (IsFeaturePresent<TFeature>())
                return;

            var feature = _featureFactory(typeof(TFeature));
            _appTimer.Subscribe(feature.AddFeature());
        }

        public bool IsFeaturePresent<TFeature>() where TFeature : FeatureBase
        {
            var feature = _featureFactory(typeof(TFeature));
            return _appTimer.IsFeaturePresent(feature.GetFeature());
        }

        public void RemoveFeature<TFeature>() where TFeature : FeatureBase
        {
            if (!IsFeaturePresent<TFeature>())
                return;

            var feature = _featureFactory(typeof(TFeature));
            _appTimer.UnSubscribe(feature.RemoveFeature());
        }
    }
}
