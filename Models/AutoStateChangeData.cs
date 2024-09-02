using System;
using System.Collections.Generic;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.Models
{
    [Serializable]
    public class AutoStateChangeData
    {
        public ProcessActivity[] Activities { get; set; } = new ProcessActivity[0];
        public string[] WorkingStateWindows { get; set; } = new string[0];

        public Dictionary<string, TimeOnly> ActivitiesC = new();

        public void ConvertSaveDataToUsableData()
        {
            try
            {
                foreach (ProcessActivity activity in Activities)
                {
                    activity.ConvertSaveDataToUsableData();
                    ActivitiesC.Add(activity.Process, activity.TimeSpentC);
                }
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorBox("StateChangeData Error", "Failed to convert data to usable data", ex);
            }
        }
        public void ConvertUsableDataToSaveData()
        {
            try
            {
                List<ProcessActivity> processActivities = new();


                foreach (KeyValuePair<string, TimeOnly> activity in ActivitiesC)
                {
                    ProcessActivity process = new();
                    process.DateC = DataStorageFeature.Instance.TodayData.DateC;
                    process.Process = activity.Key;
                    process.TimeSpentC = activity.Value;

                    process.ConvertUsableDataToSaveData();

                    processActivities.Add(process);
                }

                Activities = processActivities.ToArray();
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorBox("StateChangeData Error", "Failed to convert usable data to save data", ex);
            }
        }
    }
}
