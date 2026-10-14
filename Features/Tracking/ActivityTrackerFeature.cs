using Serilog;
using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Native;
using WorkLifeBalance.Shared.Scheduling;

namespace WorkLifeBalance.Features.Tracking
{
    // Records the time spent on the focused process and, for browsers, on the host of the active tab
    public class ActivityTrackerFeature : FeatureBase
    {
        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        private readonly LowLevelHandler _lowLevelHandler;
        private readonly DataStorageFeature _dataStorage;

        public ActivityTrackerFeature(LowLevelHandler lowLevelHandler, DataStorageFeature dataStorage)
        {
            _lowLevelHandler = lowLevelHandler;
            _dataStorage = dataStorage;
        }

        public event Action<string>? OnWindowChange;
        public event Action<string>? OnPageChange;

        public string ActiveWindow { get; set; } = "";
        public string ActiveUrl { get; set; } = "";

        protected override Func<Task> ReturnFeatureMethod() => TriggerRecordActivity;

        private Task TriggerRecordActivity()
        {
            try
            {
                var foregroundWindow = _lowLevelHandler.ReadForegroundWindow();
                ActiveWindow = _lowLevelHandler.GetProcessWithId(foregroundWindow, out var processId);

                if (BrowserHelper.BrowserExecutables.Contains(ActiveWindow))
                {
                    var activeTab = _lowLevelHandler.GetActiveTab(processId);
                    if (UrlHelper.TryGetHost(activeTab, out var host))
                    {
                        ActiveUrl = host!;
                        OnPageChange?.Invoke(ActiveUrl);
                        RecordActivityForPage();
                    }
                }
                else
                {
                    ActiveUrl = "";
                    OnPageChange?.Invoke(ActiveUrl);
                    RecordActivityForPage();
                }

                OnWindowChange?.Invoke(ActiveWindow);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to get process of window");
            }

            RecordActivityForProcess();
            return Task.CompletedTask;
        }

        private void RecordActivityForProcess()
        {
            if (!string.IsNullOrEmpty(ActiveWindow))
                AddSecond(_dataStorage.AutoChangeData.ProcessActivitiesC, ActiveWindow);
        }

        private void RecordActivityForPage()
        {
            if (!string.IsNullOrEmpty(ActiveUrl))
                AddSecond(_dataStorage.AutoChangeData.PageActivitiesC, ActiveUrl);
        }

        // The first second of a new activity only creates the entry, like it always did
        private static void AddSecond(Dictionary<string, TimeOnly> activities, string key)
        {
            if (activities.TryGetValue(key, out var spent))
                activities[key] = spent.Add(OneSecond);
            else
                activities.Add(key, new TimeOnly());
        }
    }
}
