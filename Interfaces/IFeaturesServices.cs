using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.Interfaces
{
    public interface IFeaturesServices
    {
        public void AddFeature<T>() where T : FeatureBase;
        public void RemoveFeature<T>() where T : FeatureBase;
    }
}
