using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkLifeBalance.Data
{
    [Serializable]
    public class ProcessActivity
    {
        public string Date { get; set; } = "06062023";
        public string Process { get; set; } = "da";
        public string TimeSpent { get; set; } = "000000";

        public DateOnly DateC { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public TimeOnly TimeSpentC { get; set; } = new TimeOnly(0, 0, 0);

        public void ConvertSaveDataToUsableData()
        {
            if (string.IsNullOrEmpty(Date))
            {
                return;
            }

            DateC = new DateOnly
                (
                    int.Parse(Date.Substring(4, 4)),
                    int.Parse(Date.Substring(0, 2)),
                    int.Parse(Date.Substring(2, 2))
                );

            TimeSpentC = new TimeOnly
                (
                    int.Parse(TimeSpent.Substring(0, 2)),
                    int.Parse(TimeSpent.Substring(2, 2)),
                    int.Parse(TimeSpent.Substring(4, 2))
                );
        }
        public void ConvertUsableDataToSaveData()
        {
            Date = DateC.ToString("MMddyyyy");

            TimeSpent = TimeSpentC.ToString("HHmmss");
        }
    }
}
