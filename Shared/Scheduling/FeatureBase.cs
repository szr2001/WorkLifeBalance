namespace WorkLifeBalance.Shared.Scheduling
{
    // Base class for every feature, it cancels the feature delay if the feature is removed.
    // Features use a bool to ignore the main timer event while they are running: if the main timer
    // runs every second and a feature has a 5 minute interval, it runs once, then ignores the
    // main timer for 5 minutes and repeats.
    public abstract class FeatureBase
    {
        protected CancellationTokenSource CancelTokenSource { get; set; } = new();

        public bool IsFeatureRunning { get; set; }

        public Func<Task> AddFeature()
        {
            IsFeatureRunning = false;
            CancelTokenSource = new();
            OnFeatureAdded();
            return ReturnFeatureMethod();
        }

        public Func<Task> GetFeature() => ReturnFeatureMethod();

        public Func<Task> RemoveFeature()
        {
            CancelTokenSource.Cancel();
            CancelTokenSource = new();
            OnFeatureRemoved();
            return ReturnFeatureMethod();
        }

        protected abstract Func<Task> ReturnFeatureMethod();

        protected virtual void OnFeatureAdded()
        {
        }

        protected virtual void OnFeatureRemoved()
        {
        }
    }
}
