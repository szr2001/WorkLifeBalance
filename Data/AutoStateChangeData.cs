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

        public void ConvertSaveDataToUsableData()
        {
            foreach(ProcessActivity activity in Activities)
            {
                activity.ConvertSaveDataToUsableData();
            }
        }
        public void ConvertUsableDataToSaveData()
        {
            foreach (ProcessActivity activity in Activities)
            {
                activity.ConvertUsableDataToSaveData();
            }
        }
    }
}
