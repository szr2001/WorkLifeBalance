using System.Collections.Generic;
using System.Threading.Tasks;
using WorkLifeBalance.Models;
using System.Linq;
using System;

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

            await dataAccess.Execute(DeleteOldWorkingWindowsSQL, new { });

            string InsertNewWorkingWindowsSQL = @"INSERT INTO WorkingWindows (WorkingStateWindows)
                  VALUES (@WorkingWindow)";

            foreach(string window in autod.WorkingStateWindows)
            {
                await dataAccess.WriteData(InsertNewWorkingWindowsSQL, new { WorkingWindow = window });
            }

            string UpdateActivitySql = @"UPDATE Activity
                                SET TimeSpent = @TimeSpent
                                WHERE Date = @Date AND Process = @Process";

            string InsertActivitySql = @"INSERT INTO Activity (Date,Process,TimeSpent)
                               VALUES (@Date,@Process,@TimeSpent)";

            foreach (ProcessActivityData activity in autod.Activities)
            {
                int affectedRows = await dataAccess.WriteData(UpdateActivitySql, activity);

                if (affectedRows == 0)
                {
                    await dataAccess.WriteData(InsertActivitySql, activity);
                }
            }

        }

        public async Task<AutoStateChangeData> ReadAutoStateData(string date)
        {
            AutoStateChangeData retrivedSettings = new();

            string sql = @$"SELECT * FROM Activity
                            WHERE Date = @Date";

            retrivedSettings.Activities = (await dataAccess.ReadData<ProcessActivityData, dynamic>(sql, new { Date = date })).ToArray();

            sql = @$"SELECT * FROM WorkingWindows";

            retrivedSettings.WorkingStateWindows = (await dataAccess.ReadData<string, dynamic>(sql, new { })).ToArray();

            retrivedSettings.ConvertSaveDataToUsableData();
            
            return retrivedSettings;
        }

        public async Task<List<ProcessActivityData>> ReadDayActivity(string date)
        {
            List<ProcessActivityData> ReturnActivity = new();

            string sql = @$"SELECT * from Activity 
                            WHERE Date Like @Date";


            ReturnActivity = (await dataAccess.ReadData<ProcessActivityData, dynamic>(sql, new { Date = date })).ToList();

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
                        AutoDetectWorking = @AutoDetectWorking,
                        AutoDetectIdle = @AutoDetectIdle,
                        StartUpCorner = @StartUpCorner,
                        SaveInterval = @SaveInterval,
                        AutoDetectInterval = @AutoDetectInterval,
                        AutoDetectIdleInterval = @AutoDetectIdleInterval
                        LIMIT 1";

            await dataAccess.WriteData(sql, sett);
        }

        public async Task<AppSettingsData> ReadSettings()
        {
            AppSettingsData? retrivedSettings;

            string sql = @$"SELECT * FROM Settings
                            LIMIT 1";

            retrivedSettings = (await dataAccess.ReadData<AppSettingsData, dynamic>(sql, new { })).FirstOrDefault();

            if(retrivedSettings == null)
            {
                retrivedSettings = new();
            }

            retrivedSettings.ConvertSaveDataToUsableData();

            return retrivedSettings;
        }

        public async Task WriteDay(DayData day)
        {
            day.ConvertUsableDataToSaveData();
            
            string sql = @"INSERT OR REPLACE INTO Days (Date,WorkedAmmount,RestedAmmount)
                         VALUES (@Date,@WorkedAmmount,@RestedAmmount)";

            await dataAccess.WriteData(sql, day);
        }

        public async Task<DayData> ReadDay(string date)
        {
            DayData? retrivedDay;

            string sql = @$"SELECT * FROM Days
                          WHERE Date = @Date";
            retrivedDay = (await dataAccess.ReadData<DayData, dynamic>(sql, new { Date = date })).FirstOrDefault();

            if(retrivedDay == null)
            {
                retrivedDay = new();
            }

            retrivedDay.ConvertSaveDataToUsableData();

            return retrivedDay;
        }

        public async Task<int> ReadCountInMonth(string month)
        {
            int returncount = 0;

            string sql = @$"SELECT COUNT(*) AS row_count
                            FROM Days WHERE date LIKE @Pattern";
            returncount = await dataAccess.Execute(sql, new { Pattern = $"{month}%%" });

            return returncount;
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
                //problem with Pattern
                sql = @$"SELECT * from Days 
                        WHERE Date Like @Pattern";
            }

            ReturnDays = (await dataAccess.ReadData<DayData, dynamic>(sql, new { Pattern = $"{Month}%{year}" })).ToList();

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
                sql = @$"SELECT * FROM Days 
                         WHERE CAST(@CollumnData as INT) = 
                        (SELECT MAX(CAST(@CollumnData as INT)) FROM Days)";
                retrivedDay = (await dataAccess.ReadData<DayData, dynamic>(sql, new { CollumnData = collumnData })).FirstOrDefault();
            }
            else
            {
                sql = @$"SELECT * FROM Days 
                             WHERE CAST(@CollumnData as INT) = 
                             (SELECT MAX(CAST(@CollumnData as INT)) FROM Days
                             WHERE Date LIKE @Pattern)";
                retrivedDay = (await dataAccess.ReadData<DayData, dynamic>(sql, new { Pattern = $"{Month}%{year}%", CollumnData = collumnData})).FirstOrDefault();
            }


            if (retrivedDay == null)
            {
                retrivedDay = new();
            }

            retrivedDay.ConvertSaveDataToUsableData();

            return retrivedDay;
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
            retrivedDay = (await dataAccess.ReadData<ProcessActivityData, dynamic>(sql, new { Activity = activity, Pattern = $"{Month}%{year}" })).FirstOrDefault();

            if(retrivedDay == null)
            {
                retrivedDay = new();
            }

            retrivedDay.ConvertSaveDataToUsableData();

            return retrivedDay;
        }

    }
}