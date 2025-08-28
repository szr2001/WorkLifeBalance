using System;

namespace WorkLifeBalance.Models.Base;

public abstract class ActivityDataBase
{
    public abstract string Date { get; set; } 
    public abstract string TimeSpent { get; set; }

    public DateOnly DateC { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public TimeOnly TimeSpentC { get; set; } = new TimeOnly(0, 0, 0);

    public virtual void ConvertSaveDataToUsableData()
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

            TimeSpentC = new TimeOnly
            (
                int.Parse(TimeSpent.Substring(0, 2)),
                int.Parse(TimeSpent.Substring(2, 2)),
                int.Parse(TimeSpent.Substring(4, 2))
            );
        }
        catch (Exception ex)
        {
            //MainWindow.ShowErrorBox("ProcessActivity Error", "Failed to convert data to usable data", ex);
        }
    }
    public virtual void ConvertUsableDataToSaveData()
    {
        try
        {
            Date = DateC.ToString("MMddyyyy");

            TimeSpent = TimeSpentC.ToString("HHmmss");
        }
        catch (Exception ex)
        {
            //MainWindow.ShowErrorBox("ProcessActivity Error", "Failed to convert usable data to save data", ex);
        }
    }
}