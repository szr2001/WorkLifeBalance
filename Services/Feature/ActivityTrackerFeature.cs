using Serilog;
using System;
using System.Threading.Tasks;
using WorkLifeBalance.Helpers;

namespace WorkLifeBalance.Services.Feature;

public class ActivityTrackerFeature : FeatureBase
{
    public delegate void ActiveProcess(string ActiveWindow);

    public delegate void ActivePage(string activePage);

    public event ActiveProcess? OnWindowChange;

    public event ActivePage? OnPageChange;

    public string ActiveWindow { get; set; } = "";
    public string ActiveUrl { get; set; } = "";

    private readonly TimeSpan OneSec = new(0, 0, 1);

    private readonly LowLevelHandler lowLevelHandler;
    private readonly DataStorageFeature dataStorageFeature;

    public ActivityTrackerFeature(LowLevelHandler lowLevelHandler, DataStorageFeature dataStorageFeature)
    {
        this.lowLevelHandler = lowLevelHandler;
        this.dataStorageFeature = dataStorageFeature;
    }

    protected override Func<Task> ReturnFeatureMethod()
    {
        return TriggerRecordActivity;
    }

    private Task TriggerRecordActivity()
    {
        try
        {
            nint foregroundWindowHandle = lowLevelHandler.ReadForegroundWindow();

            ActiveWindow = lowLevelHandler.GetProcessWithId(foregroundWindowHandle, out uint processId);

            if (Constants.BrowserExecutables.Contains(ActiveWindow))
            {
                string? activeTab = lowLevelHandler.GetActiveTab(processId);
                if (UrlHelper.TryGetHost(activeTab, out string? host))
                {
                    ActiveUrl = host;
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
        try
        {
            TimeOnly IncreasedTimeSpan =
                dataStorageFeature.AutoChangeData.ProcessActivitiesC[ActiveWindow].Add(OneSec);
            dataStorageFeature.AutoChangeData.ProcessActivitiesC[ActiveWindow] = IncreasedTimeSpan;
        }
        catch
        {
            dataStorageFeature.AutoChangeData.ProcessActivitiesC.Add(ActiveWindow, new TimeOnly());
        }
    }

    private void RecordActivityForPage()
    {
        if (string.IsNullOrEmpty(ActiveUrl))
        {
            return;
        }

        try
        {
            TimeOnly IncreasedTimeSpan =
                dataStorageFeature.AutoChangeData.PageActivitiesC[ActiveUrl].Add(OneSec);
            dataStorageFeature.AutoChangeData.PageActivitiesC[ActiveUrl] = IncreasedTimeSpan;
        }
        catch
        {
            dataStorageFeature.AutoChangeData.PageActivitiesC.Add(ActiveUrl, new TimeOnly());
        }
    }
}