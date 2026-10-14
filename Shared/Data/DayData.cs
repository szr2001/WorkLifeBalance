using Serilog;

namespace WorkLifeBalance.Shared.Data
{
    public class DayData
    {
        public string Date { get; set; } = "";
        public string WorkedAmmount { get; set; } = "";
        public string RestedAmmount { get; set; } = "";
        public string IdleAmmount { get; set; } = "";

        public DateOnly DateC { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public TimeOnly WorkedAmmountC { get; set; } = new(0, 0, 0);
        public TimeOnly RestedAmmountC { get; set; } = new(0, 0, 0);
        public TimeOnly IdleAmmountC { get; set; } = new(0, 0, 0);

        public void ConvertSaveDataToUsableData()
        {
            if (string.IsNullOrEmpty(Date))
                return;

            try
            {
                DateC = StoredFormat.ParseDate(Date);
                WorkedAmmountC = StoredFormat.ParseTime(WorkedAmmount);
                RestedAmmountC = StoredFormat.ParseTime(RestedAmmount);
                IdleAmmountC = StoredFormat.ParseTime(IdleAmmount);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DayData: failed to convert data to usable data");
            }
        }

        public void ConvertUsableDataToSaveData()
        {
            Date = DateC.ToString(StoredFormat.Date);
            WorkedAmmount = WorkedAmmountC.ToString(StoredFormat.Time);
            RestedAmmount = RestedAmmountC.ToString(StoredFormat.Time);
            IdleAmmount = IdleAmmountC.ToString(StoredFormat.Time);
        }
    }
}
