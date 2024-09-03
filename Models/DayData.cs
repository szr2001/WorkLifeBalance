using System;

namespace WorkLifeBalance.Models
{
    [Serializable]
    public class DayData
    {
        public string Date { get; set; } = "";
        public string WorkedAmmount { get; set; } = "";
        public string RestedAmmount { get; set; } = "";

        public DateOnly DateC { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public TimeOnly WorkedAmmountC { get; set; } = new TimeOnly(0, 0, 0);
        public TimeOnly RestedAmmountC { get; set; } = new TimeOnly(0, 0, 0);

        public void ConvertSaveDataToUsableData()
        {
            try
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

                WorkedAmmountC = new TimeOnly
                    (
                        int.Parse(WorkedAmmount.Substring(0, 2)),
                        int.Parse(WorkedAmmount.Substring(2, 2)),
                        int.Parse(WorkedAmmount.Substring(4, 2))
                    );

                RestedAmmountC = new TimeOnly
                    (
                        int.Parse(RestedAmmount.Substring(0, 2)),
                        int.Parse(RestedAmmount.Substring(2, 2)),
                        int.Parse(RestedAmmount.Substring(4, 2))
                    );
            }
            catch (Exception ex)
            {
                //MainWindow.ShowErrorBox("DayData Error", "Failed to convert data to usable data", ex);
            }
        }
        public void ConvertUsableDataToSaveData()
        {
            try
            {
                Date = DateC.ToString("MMddyyyy");

                WorkedAmmount = WorkedAmmountC.ToString("HHmmss");

                RestedAmmount = RestedAmmountC.ToString("HHmmss");
            }
            catch (Exception ex)
            {
                //MainWindow.ShowErrorBox("DayData Error", "Failed to convert usable data to save data", ex);
            }
        }
    }
}
