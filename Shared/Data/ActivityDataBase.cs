using Serilog;

namespace WorkLifeBalance.Shared.Data
{
    public abstract class ActivityDataBase
    {
        public abstract string Date { get; set; }
        public abstract string TimeSpent { get; set; }

        public DateOnly DateC { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public TimeOnly TimeSpentC { get; set; } = new(0, 0, 0);

        public virtual void ConvertSaveDataToUsableData()
        {
            if (string.IsNullOrEmpty(Date))
                return;

            try
            {
                DateC = StoredFormat.ParseDate(Date);
                TimeSpentC = StoredFormat.ParseTime(TimeSpent);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Activity: failed to convert data to usable data");
            }
        }

        public virtual void ConvertUsableDataToSaveData()
        {
            Date = DateC.ToString(StoredFormat.Date);
            TimeSpent = TimeSpentC.ToString(StoredFormat.Time);
        }
    }
}
