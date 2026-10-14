using WorkLifeBalance.Shared.Data;

namespace WorkLifeBalance.Features.Statistics
{
    // Queries behind the data pages. Months are filtered with LIKE on the "MMddyyyy" date: "MM%yyyy".
    public class StatisticsStore
    {
        private readonly SqlDataAccess _dataAccess;

        public StatisticsStore(SqlDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public async Task<List<ProcessActivityData>> ReadProcessDayActivity(string date)
        {
            var activity = await _dataAccess.ReadDataAsync<ProcessActivityData, dynamic>(
                "SELECT Date, Process, TimeSpent FROM Activity WHERE Date LIKE @Date AND Process IS NOT NULL", new { Date = date });

            activity.ForEach(item => item.ConvertSaveDataToUsableData());
            return activity;
        }

        public async Task<List<PageActivityData>> ReadUrlDayActivity(string date)
        {
            var activity = await _dataAccess.ReadDataAsync<PageActivityData, dynamic>(
                "SELECT Date, Url, TimeSpent FROM Activity WHERE Date LIKE @Date AND Url IS NOT NULL", new { Date = date });

            activity.ForEach(item => item.ConvertSaveDataToUsableData());
            return activity;
        }

        public async Task<int> ReadCountInMonth(string month, string year) =>
            await _dataAccess.ExecuteAsync("SELECT COUNT(*) AS row_count FROM Days WHERE date LIKE @Pattern", new { Pattern = $"{month}%{year}" });

        // No month or year means every day
        public async Task<List<DayData>> ReadMonth(string month = "", string year = "")
        {
            var sql = string.IsNullOrEmpty(month) || string.IsNullOrWhiteSpace(year)
                ? "SELECT * FROM Days"
                : "SELECT * FROM Days WHERE Date LIKE @Pattern";

            var days = await _dataAccess.ReadDataAsync<DayData, dynamic>(sql, new { Pattern = $"{month}%{year}" });
            days.ForEach(day => day.ConvertSaveDataToUsableData());
            return days;
        }

        // The day with the highest value in the column; the column name goes in the sql because it can't be a parameter
        public async Task<DayData> GetMaxValue(string column, string month = "", string year = "")
        {
            DayData? day;
            if (string.IsNullOrEmpty(month) || string.IsNullOrEmpty(year))
            {
                var sql = $"SELECT * FROM Days WHERE CAST({column} as INT) = (SELECT MAX(CAST({column} as INT)) FROM Days)";
                day = (await _dataAccess.ReadDataAsync<DayData, dynamic>(sql, new { })).FirstOrDefault();
            }
            else
            {
                var sql = $"SELECT * FROM Days WHERE CAST({column} as INT) = (SELECT MAX(CAST({column} as INT)) FROM Days WHERE Date LIKE @Template)";
                day = (await _dataAccess.ReadDataAsync<DayData, dynamic>(sql, new { Template = $"{month}%{year}" })).FirstOrDefault();
            }

            day ??= new DayData();
            day.ConvertSaveDataToUsableData();
            return day;
        }

        public async Task<int> GetAvgSecondsTimeOnly(string timeColumn, string month = "", string year = "")
        {
            var sql = $"""
                WITH ConvertedTimes AS
                (
                    SELECT
                    (CAST(SUBSTR({timeColumn}, 1, 2) AS INTEGER) * 3600 +
                    CAST(SUBSTR({timeColumn}, 3, 2) AS INTEGER) * 60 +
                    CAST(SUBSTR({timeColumn}, 5, 2) AS INTEGER)) AS TotalSeconds, date FROM Days
                )
                SELECT COALESCE(AVG(TotalSeconds), 0) AS AvgSeconds
                FROM ConvertedTimes WHERE date LIKE @Template
                """;

            return (await _dataAccess.ReadDataAsync<int, dynamic>(sql, new { Template = $"{month}%{year}" })).FirstOrDefault();
        }
    }
}
