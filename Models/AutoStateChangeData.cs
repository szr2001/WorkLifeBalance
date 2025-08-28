using System;
using System.Collections.Generic;

namespace WorkLifeBalance.Models
{
    [Serializable]
    public class AutoStateChangeData
    {
        public ProcessActivityData[] ProcessActivities { get; set; } = Array.Empty<ProcessActivityData>();
        
        public PageActivityData[] PageActivities { get; set; } = Array.Empty<PageActivityData>();
        public string[] WorkingStateWindows { get; set; } = Array.Empty<string>();
        public string[] WorkingStateUrls { get; set; } = Array.Empty<string>();

        public Dictionary<string, TimeOnly> PageActivitiesC = new();
        public Dictionary<string, TimeOnly> ProcessActivitiesC = new();

        public void ConvertSaveDataToUsableData()
        {
            try
            {
                foreach (ProcessActivityData activity in ProcessActivities)
                {
                    activity.ConvertSaveDataToUsableData();
                    ProcessActivitiesC.Add(activity.Process, activity.TimeSpentC);
                }
                
                foreach (PageActivityData activity in PageActivities)
                {
                    activity.ConvertSaveDataToUsableData();
                    PageActivitiesC.Add(activity.Url, activity.TimeSpentC);
                }
            }
            catch (Exception ex)
            {
                //MainWindow.ShowErrorBox("StateChangeData Error", "Failed to convert data to usable data", ex);
            }
        }
        public void ConvertUsableDataToSaveData()
        {
            try
            {
                List<ProcessActivityData> processActivities = new();
                List<PageActivityData> pageActivities = new();
                
                foreach (KeyValuePair<string, TimeOnly> activity in ProcessActivitiesC)
                {
                    ProcessActivityData process = new()
                    {
                        //process.DateC = DataStorageFeature.Instance.TodayData.DateC;
                        Process = activity.Key,
                        TimeSpentC = activity.Value
                    };

                    process.ConvertUsableDataToSaveData();

                    processActivities.Add(process);
                }

                foreach (KeyValuePair<string, TimeOnly> activity in PageActivitiesC)
                {
                    PageActivityData page = new()
                    {
                        //process.DateC = DataStorageFeature.Instance.TodayData.DateC;
                        Url = activity.Key,
                        TimeSpentC = activity.Value
                    };

                    page.ConvertUsableDataToSaveData();

                    pageActivities.Add(page);
                }

                PageActivities = pageActivities.ToArray();
                ProcessActivities = processActivities.ToArray();
            }
            catch (Exception ex)
            {
                //MainWindow.ShowErrorBox("StateChangeData Error", "Failed to convert usable data to save data", ex);
            }
        }
    }
}
