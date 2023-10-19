using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkLifeBalance.Data
{
    [Serializable]
    public class AutoStateChangeData
    {
        public ProcessActivity[] Activities { get; set; } = new ProcessActivity[0];
        public string[] WorkingStateWindows { get; set; } = new string[0];

        public List<ProcessActivity> ActivitiesC = new();

        public void ConvertSaveDataToUsableData()
        {
            ActivitiesC = Activities.ToList();

            foreach(ProcessActivity activity in ActivitiesC)
            {
                activity.ConvertSaveDataToUsableData();
            }
        }
        public void ConvertUsableDataToSaveData()
        {
            Activities = ActivitiesC.ToArray();

            foreach (ProcessActivity activity in Activities)
            {
                activity.ConvertUsableDataToSaveData();
            }
        }
    }
}
