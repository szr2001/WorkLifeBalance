using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.Interfaces
{
    public interface IFeaturesServices
    {
        public void AddFeature<TFeature>() where TFeature : FeatureBase;
        public void RemoveFeature<TFeature>() where TFeature : FeatureBase;
    }
}
