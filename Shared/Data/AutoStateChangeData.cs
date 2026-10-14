using Serilog;

namespace WorkLifeBalance.Shared.Data
{
    public class AutoStateChangeData
    {
        public ProcessActivityData[] ProcessActivities { get; set; } = [];
        public PageActivityData[] PageActivities { get; set; } = [];
        public string[] WorkingStateWindows { get; set; } = [];
        public string[] WorkingStateUrls { get; set; } = [];

        public Dictionary<string, TimeOnly> PageActivitiesC { get; } = new();
        public Dictionary<string, TimeOnly> ProcessActivitiesC { get; } = new();

        public void ConvertSaveDataToUsableData()
        {
            try
            {
                foreach (var activity in ProcessActivities.Where(activity => !string.IsNullOrEmpty(activity.Process)))
                {
                    activity.ConvertSaveDataToUsableData();
                    ProcessActivitiesC.TryAdd(activity.Process, activity.TimeSpentC);
                }

                foreach (var activity in PageActivities.Where(activity => !string.IsNullOrEmpty(activity.Url)))
                {
                    activity.ConvertSaveDataToUsableData();
                    PageActivitiesC.TryAdd(activity.Url!, activity.TimeSpentC);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AutoStateChangeData: failed to convert data to usable data");
            }
        }

        public void ConvertUsableDataToSaveData()
        {
            ProcessActivities = ProcessActivitiesC.Select(activity =>
            {
                var process = new ProcessActivityData { Process = activity.Key, TimeSpentC = activity.Value };
                process.ConvertUsableDataToSaveData();
                return process;
            }).ToArray();

            PageActivities = PageActivitiesC.Select(activity =>
            {
                var page = new PageActivityData { Url = activity.Key, TimeSpentC = activity.Value };
                page.ConvertUsableDataToSaveData();
                return page;
            }).ToArray();
        }
    }
}
