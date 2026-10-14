namespace WorkLifeBalance.Shared.Data
{
    public class AppDataStore
    {
        private readonly SqlDataAccess _dataAccess;

        public AppDataStore(SqlDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public async Task WriteAutoStateData(AutoStateChangeData data)
        {
            data.ConvertUsableDataToSaveData();

            await _dataAccess.ExecuteAsync("DELETE FROM WorkingWindows", new { });
            await _dataAccess.ExecuteAsync("DELETE FROM WorkingUrls", new { });

            foreach (var window in data.WorkingStateWindows)
                await _dataAccess.WriteDataAsync("INSERT INTO WorkingWindows (WorkingStateWindows) VALUES (@WorkingWindow)", new { WorkingWindow = window });

            foreach (var url in data.WorkingStateUrls)
                await _dataAccess.WriteDataAsync("INSERT INTO WorkingUrls (WorkingStateUrl) VALUES (@WorkingUrl)", new { WorkingUrl = url });

            await WriteActivity(data.ProcessActivities, "Process");
            await WriteActivity(data.PageActivities, "Url");
        }

        public async Task<AutoStateChangeData> ReadAutoStateData(string date)
        {
            var data = new AutoStateChangeData
            {
                ProcessActivities = (await _dataAccess.ReadDataAsync<ProcessActivityData, dynamic>(
                    "SELECT Date, Process, TimeSpent FROM Activity WHERE Date = @Date AND Process IS NOT NULL", new { Date = date })).ToArray(),
                PageActivities = (await _dataAccess.ReadDataAsync<PageActivityData, dynamic>(
                    "SELECT Date, Url, TimeSpent FROM Activity WHERE Date = @Date AND Url IS NOT NULL", new { Date = date })).ToArray(),
                WorkingStateWindows = (await _dataAccess.ReadDataAsync<string, dynamic>(
                    "SELECT WorkingStateWindows FROM WorkingWindows", new { })).ToArray(),
                WorkingStateUrls = (await _dataAccess.ReadDataAsync<string, dynamic>(
                    "SELECT WorkingStateUrl FROM WorkingUrls", new { })).ToArray(),
            };

            data.ConvertSaveDataToUsableData();
            return data;
        }

        public async Task WriteSettings(AppSettingsData settings)
        {
            settings.ConvertUsableDataToSaveData();

            const string sql = """
                UPDATE Settings
                SET LastTimeOpened = @LastTimeOpened,
                        StartWithWindows = @StartWithWindows,
                        SaveInterval = @SaveInterval,
                        AutoDetectInterval = @AutoDetectInterval,
                        AutoDetectIdleInterval = @AutoDetectIdleInterval,
                        MinimizeToTray = @MinimizeToTray
                LIMIT 1
                """;
            await _dataAccess.WriteDataAsync(sql, settings);
        }

        public async Task<AppSettingsData> ReadSettings()
        {
            var settings = (await _dataAccess.ReadDataAsync<AppSettingsData, dynamic>("SELECT * FROM Settings LIMIT 1", new { })).FirstOrDefault()
                ?? new AppSettingsData();

            settings.ConvertSaveDataToUsableData();
            return settings;
        }

        public async Task WriteDay(DayData day)
        {
            day.ConvertUsableDataToSaveData();

            const string sql = """
                INSERT OR REPLACE INTO Days (Date, WorkedAmmount, RestedAmmount, IdleAmmount)
                VALUES (@Date, @WorkedAmmount, @RestedAmmount, @IdleAmmount)
                """;
            await _dataAccess.WriteDataAsync(sql, day);
        }

        public async Task<DayData> ReadDay(string date)
        {
            var day = (await _dataAccess.ReadDataAsync<DayData, dynamic>("SELECT * FROM Days WHERE Date = @Date", new { Date = date })).FirstOrDefault()
                ?? new DayData();

            day.ConvertSaveDataToUsableData();
            return day;
        }

        private async Task WriteActivity<T>(T[] activities, string column) where T : ActivityDataBase
        {
            var updateSql = $"UPDATE Activity SET TimeSpent = @TimeSpent WHERE Date = @Date AND {column} = @{column}";
            var insertSql = $"INSERT INTO Activity (Date, {column}, TimeSpent) VALUES (@Date, @{column}, @TimeSpent)";

            foreach (var activity in activities)
            {
                if (await _dataAccess.WriteDataAsync(updateSql, activity) == 0)
                    await _dataAccess.WriteDataAsync(insertSql, activity);
            }
        }
    }
}
