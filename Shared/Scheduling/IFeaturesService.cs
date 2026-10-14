namespace WorkLifeBalance.Shared.Scheduling
{
    public interface IFeaturesService
    {
        bool IsFeaturePresent<TFeature>() where TFeature : FeatureBase;

        void AddFeature<TFeature>() where TFeature : FeatureBase;

        void RemoveFeature<TFeature>() where TFeature : FeatureBase;
    }
}
