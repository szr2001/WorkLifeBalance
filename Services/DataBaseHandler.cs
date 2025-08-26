using System.Collections.Generic;
using System.Threading.Tasks;
using WorkLifeBalance.Models;
using System.Linq;
using WorkLifeBalance.Models.Base;

namespace WorkLifeBalance.Services
{
    public class DataBaseHandler
    {
        private readonly SqlDataAccess dataAccess;

        public DataBaseHandler(SqlDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public async Task WriteAutoSateData(AutoStateChangeData autod)
        {
            autod.ConvertUsableDataToSaveData();

            //I'm too dumb to figure out the sql for less calls
            string DeleteOldWorkingWindowsSQL = @"DELETE FROM WorkingWindows";

            await dataAccess.ExecuteAsync(DeleteOldWorkingWindowsSQL, new { });

            string DeleteOldWorkingPagesSSql = @"DELETE FROM WorkingUrls";

            await dataAccess.ExecuteAsync(DeleteOldWorkingPagesSSql, new { });
            
            string InsertNewWorkingWindowsSQL = @"INSERT INTO WorkingWindows (WorkingStateWindows)
                  VALUES (@WorkingWindow)";

            foreach(string window in autod.WorkingStateWindows)
            {
                await dataAccess.WriteDataAsync(InsertNewWorkingWindowsSQL, new { WorkingWindow = window });
            }
            
            string insertNewWorkingPagesSql = @"INSERT INTO WorkingUrls (WorkingStateUrl)
                  VALUES (@WorkingUrl)";

            foreach (string url in autod.WorkingStateUrls)
            {
                await dataAccess.WriteDataAsync(insertNewWorkingPagesSql, new { WorkingUrl = url });
            }

            await InsertActivity(autod.ProcessActivities, "Process");
            await InsertActivity(autod.PageActivities, "Url");
        }

        public async Task<AutoStateChangeData> ReadAutoStateData(string date)
        {
            AutoStateChangeData retrivedSettings = new();

            string sql = @$"SELECT Date, Process, TimeSpent FROM Activity
                            WHERE Date = @Date";

            retrivedSettings.ProcessActivities = (await dataAccess.ReadDataAsync<ProcessActivityData, dynamic>(sql, new { Date = date })).ToArray();

            sql = @$"SELECT Date, Url, TimeSpent FROM Activity
                            WHERE Date = @Date";
            
            retrivedSettings.PageActivities = (await dataAccess.ReadDataAsync<PageActivityData, dynamic>(sql, new { Date = date })).ToArray();
            
            sql = @$"SELECT WorkingStateWindows FROM WorkingWindows";

            retrivedSettings.WorkingStateWindows = (await dataAccess.ReadDataAsync<string, dynamic>(sql, new { })).ToArray();

            sql = @"SELECT WorkingStateUrl FROM WorkingUrls";
            
            retrivedSettings.WorkingStateUrls = (await dataAccess.ReadDataAsync<string, dynamic>(sql, new { })).ToArray();
            
            retrivedSettings.ConvertSaveDataToUsableData();
            
            return retrivedSettings;
        }

        public async Task<List<ProcessActivityData>> ReadProcessDayActivity(string date)
        {
            List<ProcessActivityData> processActivity = new();

            string sql = @$"SELECT Date, Process, TimeSpent from Activity 
                            WHERE Date Like @Date AND Process IS NOT NULL";

            processActivity = (await dataAccess.ReadDataAsync<ProcessActivityData, dynamic>(sql, new { Date = date })).ToList();

            foreach (ProcessActivityData day in processActivity)
            {
                day.ConvertSaveDataToUsableData();
            }

            return processActivity;
        }
        
        public async Task<List<PageActivityData>> ReadUrlDayActivity(string date)
        {
            List<PageActivityData> pageActivity = new();

            string sql = @$"SELECT Date, Url, TimeSpent from Activity 
                            WHERE Date Like @Date AND Url IS NOT NULL";

            pageActivity = (await dataAccess.ReadDataAsync<PageActivityData, dynamic>(sql, new { Date = date })).ToList();

            foreach (PageActivityData day in pageActivity)
            {
                day.ConvertSaveDataToUsableData();
            }

            return pageActivity;
        }

        public async Task WriteSettings(AppSettingsData sett)
        {
            sett.ConvertUsableDataToSaveData();

            string sql = @"UPDATE Settings 
                        SET LastTimeOpened = @LastTimeOpened,
                        StartWithWindows = @StartWithWindows,
                        SaveInterval = @SaveInterval,
                        AutoDetectInterval = @AutoDetectInterval,
                        AutoDetectIdleInterval = @AutoDetectIdleInterval,
                        MinimizeToTray = @MinimizeToTray
                        LIMIT 1";

            await dataAccess.WriteDataAsync(sql, sett);
        }

        public async Task<AppSettingsData> ReadSettings()
        {
            AppSettingsData? retrivedSettings;

            string sql = @$"SELECT * FROM Settings
                            LIMIT 1";

            retrivedSettings = (await dataAccess.ReadDataAsync<AppSettingsData, dynamic>(sql, new { })).FirstOrDefault();

            retrivedSettings ??= new();

            retrivedSettings.ConvertSaveDataToUsableData();

            return retrivedSettings;
        }

        public async Task WriteDay(DayData day)
        {
            day.ConvertUsableDataToSaveData();
            
            string sql = @"INSERT OR REPLACE INTO Days (Date,WorkedAmmount,RestedAmmount,IdleAmmount)
                         VALUES (@Date,@WorkedAmmount,@RestedAmmount,@IdleAmmount)";

            await dataAccess.WriteDataAsync(sql, day);
        }

        public async Task<DayData> ReadDay(string date)
        {
            DayData? retrivedDay;

            string sql = @$"SELECT * FROM Days
                          WHERE Date = @Date";
            retrivedDay = (await dataAccess.ReadDataAsync<DayData, dynamic>(sql, new { Date = date })).FirstOrDefault();

            retrivedDay ??= new();

            retrivedDay.ConvertSaveDataToUsableData();

            return retrivedDay;
        }

        public async Task<int> ReadCountInMonth(string month, string year)
        {
            int affectedRows = 0;
            string sql = @$"SELECT COUNT(*) AS row_count
                            FROM Days WHERE date LIKE @Pattern";
            affectedRows = await dataAccess.ExecuteAsync(sql, new { Pattern = $"{month}%{year}" });

            return affectedRows;
        }

        public async Task<List<DayData>> ReadMonth(string Month = "", string year = "")
        {
            List<DayData> ReturnDays = new();

            string sql;
            if (string.IsNullOrEmpty(Month) || string.IsNullOrWhiteSpace(year))
            {
                sql = @$"SELECT * from Days";
            }
            else
            {
                sql = @$"SELECT * from Days 
                        WHERE Date Like @Pattern";
            }

            ReturnDays = (await dataAccess.ReadDataAsync<DayData, dynamic>(sql, new { Pattern = $"{Month}%{year}" })).ToList();

            foreach (DayData day in ReturnDays)
            {
                day.ConvertSaveDataToUsableData();
            }

            return ReturnDays;
        }

        public async Task<DayData> GetMaxValue(string collumnData, string Month = "", string year = "")
        {
            DayData? retrivedDay = null;

            string sql;

            if (string.IsNullOrEmpty(Month) || string.IsNullOrEmpty(year))
            {
                //pass the value directly because it brokes if I use it as a parameter
                sql = @$"SELECT * FROM Days 
                      WHERE CAST({collumnData} as INT) = 
                      (SELECT MAX(CAST({collumnData} as INT)) FROM Days)";
                retrivedDay = (await dataAccess.ReadDataAsync<DayData, dynamic>(sql, new { })).FirstOrDefault();
            }
            else
            {
                sql = @$"SELECT * FROM Days 
                        WHERE CAST({collumnData} as INT) = 
                        (SELECT MAX(CAST({collumnData} as INT)) FROM Days
                        WHERE Date LIKE @Template)";


                retrivedDay = (await dataAccess.ReadDataAsync<DayData, dynamic>(sql, new { Template = $"{Month}%{year}" })).FirstOrDefault();
            }

            retrivedDay ??= new();

            retrivedDay.ConvertSaveDataToUsableData();

            return retrivedDay;
        }

        public async Task<int> GetAvgSecondsTimeOnly(string timeOnlyCollumn, string Month = "", string year = "")
        {
            int avgAmount;

            string sql = @$"WITH ConvertedTimes AS 
                    (
                        SELECT 
                        (CAST(SUBSTR({timeOnlyCollumn}, 1, 2) AS INTEGER) * 3600 +
                        CAST(SUBSTR({timeOnlyCollumn}, 3, 2) AS INTEGER) * 60 +
                        CAST(SUBSTR({timeOnlyCollumn}, 5, 2) AS INTEGER)) AS TotalSeconds, date FROM Days
                    )
                    SELECT COALESCE(AVG(TotalSeconds), 0) AS AvgSeconds
                    FROM ConvertedTimes WHERE date LIKE @Template";

            avgAmount = (await dataAccess.ReadDataAsync<int, dynamic>(sql, new { Template = $"{Month}%{year}" })).FirstOrDefault();

            return avgAmount;
        }

        public async Task<ProcessActivityData> GetMostActiveActivity(string activity, string Month = "", string year = "")
        {
            ProcessActivityData? retrivedDay = null;

            string sql;
            if (string.IsNullOrEmpty(Month) || string.IsNullOrEmpty(year))
            {
                sql = @$"SELECT * FROM Days 
                        WHERE @Activity = 
                        (SELECT MAX(@Activity) FROM Days)";
            }
            else
            {

                sql = @$"SELECT * FROM Days 
                        WHERE @Activity = 
                        (SELECT MAX(@Activity) FROM Days
                        WHERE Date Like @Pattern)";
            }
            retrivedDay = (await dataAccess.ReadDataAsync<ProcessActivityData, dynamic>(sql, new { Activity = activity, Pattern = $"{Month}%{year}" })).FirstOrDefault();

            retrivedDay ??= new();

            retrivedDay.ConvertSaveDataToUsableData();

            return retrivedDay;
        }

        private async Task InsertActivity<T>(T[] items, string column) where T : ActivityDataBase
        {
            
            string updateActivitySql = @$"UPDATE Activity
                                SET TimeSpent = @TimeSpent
                                WHERE Date = @Date AND {column} = @{column}";

            string insertActivitySql = @$"INSERT INTO Activity (Date,{column},TimeSpent)
                               VALUES (@Date,@{column},@TimeSpent)";
            
            foreach (T activity in items)
            {
                int affectedRows = await dataAccess.WriteDataAsync(updateActivitySql, activity);

                if (affectedRows == 0)
                {
                    await dataAccess.WriteDataAsync(insertActivitySql, activity);
                }
            }
        }
    }
}