using System.Collections.Generic;
using System.Threading.Tasks;
using WorkLifeBalance.Models;
using System.Linq;
using System;
using Dapper;

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

            string InsertNewWorkingWindowsSQL = @"INSERT INTO WorkingWindows (WorkingStateWindows)
                  VALUES (@WorkingWindow)";

            foreach(string window in autod.WorkingStateWindows)
            {
                await dataAccess.WriteDataAsync(InsertNewWorkingWindowsSQL, new { WorkingWindow = window });
            }

            string UpdateActivitySql = @"UPDATE Activity
                                SET TimeSpent = @TimeSpent
                                WHERE Date = @Date AND Process = @Process";

            string InsertActivitySql = @"INSERT INTO Activity (Date,Process,TimeSpent)
                               VALUES (@Date,@Process,@TimeSpent)";

            foreach (ProcessActivityData activity in autod.Activities)
            {
                int affectedRows = await dataAccess.WriteDataAsync(UpdateActivitySql, activity);

                if (affectedRows == 0)
                {
                    await dataAccess.WriteDataAsync(InsertActivitySql, activity);
                }
            }

        }

        public async Task<AutoStateChangeData> ReadAutoStateData(string date)
        {
            AutoStateChangeData retrivedSettings = new();

            string sql = @$"SELECT * FROM Activity
                            WHERE Date = @Date";

            retrivedSettings.Activities = (await dataAccess.ReadDataAsync<ProcessActivityData, dynamic>(sql, new { Date = date })).ToArray();

            sql = @$"SELECT * FROM WorkingWindows";

            retrivedSettings.WorkingStateWindows = (await dataAccess.ReadDataAsync<string, dynamic>(sql, new { })).ToArray();

            retrivedSettings.ConvertSaveDataToUsableData();
            
            return retrivedSettings;
        }

        public async Task<List<ProcessActivityData>> ReadDayActivity(string date)
        {
            List<ProcessActivityData> ReturnActivity = new();

            string sql = @$"SELECT * from Activity 
                            WHERE Date Like @Date";


            ReturnActivity = (await dataAccess.ReadDataAsync<ProcessActivityData, dynamic>(sql, new { Date = date })).ToList();

            foreach (ProcessActivityData day in ReturnActivity)
            {
                day.ConvertSaveDataToUsableData();
            }

            return ReturnActivity;
        }

        public async Task WriteSettings(AppSettingsData sett)
        {
            sett.ConvertUsableDataToSaveData();

            string sql = @"UPDATE Settings 
                        SET LastTimeOpened = @LastTimeOpened,
                        StartWithWindows = @StartWithWindows,
                        SaveInterval = @SaveInterval,
                        AutoDetectInterval = @AutoDetectInterval,
                        AutoDetectIdleInterval = @AutoDetectIdleInterval
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

    }
}