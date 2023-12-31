﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkLifeBalance.Data;

namespace WorkLifeBalance.Handlers
{
    public static class DataBaseHandler
    {
        //semaphore to ensure only one method can write to the database at once
        private static readonly SemaphoreSlim _semaphore = new(1);
        //connection string for the db
        private static readonly string ConnectionString = @$"Data Source={Directory.GetCurrentDirectory()}\RecordedData.db;Version=3;";

        public static async Task WriteAutoSateData(AutoStateChangeData autod)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();

            autod.ConvertUsableDataToSaveData();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //open connection
                    await connection.OpenAsync();
                    //if no one writes continue
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string sql = @"DELETE FROM WorkingWindows";

                            await connection.ExecuteAsync(sql);

                            sql = @"INSERT INTO WorkingWindows (WorkingStateWindows)
                                    VALUES (@WindowValue)";

                            await connection.ExecuteAsync(sql, autod.WorkingStateWindows.Select(value => new { WindowValue = value }));

                            //problem
                            string UpdateSql = @"UPDATE Activity
                                                 SET TimeSpent = @TimeSpent
                                                 WHERE Date = @Date AND Process = @Process";

                            string AddSql = @"INSERT INTO Activity (Date,Process,TimeSpent)
                                              VALUES (@Date,@Process,@TimeSpent)";

                            foreach (ProcessActivity activity in autod.Activities)
                            {
                                int affectedRows = await connection.ExecuteAsync(UpdateSql, activity);

                                if(affectedRows == 0)
                                {
                                    await connection.ExecuteAsync(AddSql, activity);
                                }
                            }

                            await transaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            //rols back if failed
                            await transaction.RollbackAsync();
                            MainWindow.instance.MainDispatcher.Invoke(() =>
                            {
                                MainWindow.ShowErrorBox("Failed to write to database", $"This can be caused by a missing database file: {ex.Message}");
                            });
                        }
                        finally
                        {
                            await connection.CloseAsync();
                        }
                    }
                }
            }
            finally
            {
                //free semaphore so other methods can run
                _semaphore.Release();
            }
        }

        public static async Task<AutoStateChangeData> ReadAutoStateData(string date)
        {
            AutoStateChangeData retrivedSettings = new();
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes to db continue

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //start connection
                    await connection.OpenAsync();

                    //gets total number
                    try
                    {
                        string sql = @$"SELECT * FROM Activity
                                        WHERE Date = @Date";

                        retrivedSettings.Activities = (await connection.QueryAsync<ProcessActivity>(sql, new { Date = date })).ToArray();

                        sql = @$"SELECT * FROM WorkingWindows";

                        retrivedSettings.WorkingStateWindows = (await connection.QueryAsync<string>(sql)).ToArray();
                    }
                    catch (Exception ex)
                    {
                        MainWindow.instance.MainDispatcher.Invoke(() =>
                        {
                            MainWindow.ShowErrorBox("Failed to write to database", $"This can be caused by a missing database file: {ex.Message}");
                        });
                    }
                    finally
                    {
                        await connection.CloseAsync();
                    }
                }
            }
            finally
            {
                //release sempahore so other methods can run
                _semaphore.Release();
            }

            if (retrivedSettings == null)
            {
                retrivedSettings = new();
            }

            retrivedSettings.ConvertSaveDataToUsableData();
            //returns count
            return retrivedSettings;
        }

        public static async Task<List<ProcessActivity>> ReadDayActivity(string date)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes continue

            //list of filtered residences
            List<ProcessActivity> ReturnActivity = new();

            try
            {
                //write to console it started reading database
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //open connection
                    await connection.OpenAsync();

                    try
                    {
                        //depending on passed arguments chose between 2 sql statements
                        string sql = @$"SELECT * from Activity 
                                     WHERE Date Like @Date";


                        //wait for return, and pass the return to a Residence class
                        ReturnActivity = (await connection.QueryAsync<ProcessActivity>(sql, new { Date = date })).ToList();

                    }
                    catch (Exception ex)
                    {
                        //close app if failed
                        MainWindow.instance.MainDispatcher.Invoke(() =>
                        {
                            MainWindow.ShowErrorBox("Failed to read from database", $"This can be caused by a missing database file: {ex.Message}");
                        });
                    }
                    finally
                    {
                        await connection.CloseAsync();
                    }
                }
            }
            finally
            {
                //release semaphore so other methods could run
                _semaphore.Release();
            }
            foreach (ProcessActivity day in ReturnActivity)
            {
                day.ConvertSaveDataToUsableData();
            }
            //return filtered res
            return ReturnActivity;
        }

        public static async Task WriteSettings(AppSettings sett)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();

            sett.ConvertUsableDataToSaveData();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //open connection
                    await connection.OpenAsync();
                    //if no one writes continue
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string sql =    @"UPDATE Settings 
                                            SET LastTimeOpened = @LastTimeOpened,
                                            StartWithWindows = @StartWithWindows,
                                            AutoDetectWorking = @AutoDetectWorking,
                                            AutoDetectIdle = @AutoDetectIdle,
                                            StartUpCorner = @StartUpCorner,
                                            SaveInterval = @SaveInterval,
                                            AutoDetectInterval = @AutoDetectInterval,
                                            AutoDetectIdleInterval = @AutoDetectIdleInterval
                                            LIMIT 1";

                            connection.Execute(sql, sett);
                            await transaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            //rols back if failed
                            await transaction.RollbackAsync();
                            MainWindow.instance.MainDispatcher.Invoke(() =>
                            {
                                MainWindow.ShowErrorBox("Failed to write to database", $"This can be caused by a missing database file: {ex.Message}");
                            });
                        }
                        finally
                        {
                            await connection.CloseAsync();
                        }
                    }
                }
            }
            finally
            {
                //free semaphore so other methods can run
                _semaphore.Release();
            }
        }

        public static async Task<AppSettings> ReadSettings()
        {
            AppSettings retrivedSettings = new();
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes to db continue

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //start connection
                    await connection.OpenAsync();

                    //gets total number
                    try
                    {
                        string sql = @$"SELECT * FROM Settings
                                        LIMIT 1";

                        retrivedSettings = connection.QueryFirstOrDefault<AppSettings>(sql);
                    }
                    catch (Exception ex)
                    {
                        MainWindow.instance.MainDispatcher.Invoke(() =>
                        {
                            MainWindow.ShowErrorBox("Failed to write to database", $"This can be caused by a missing database file: {ex.Message}");
                        });
                    }
                    finally
                    {
                        await connection.CloseAsync();
                    }
                }
            }
            finally
            {
                //release sempahore so other methods can run
                _semaphore.Release();
            }

            if(retrivedSettings == null)
            {
                retrivedSettings = new();
            }

            retrivedSettings.ConvertSaveDataToUsableData();
            //returns count
            return retrivedSettings;
        }

        public static async Task WriteDay(DayData day)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();

            day.ConvertUsableDataToSaveData();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //open connection
                    await connection.OpenAsync();
                    //if no one writes continue
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            //write data
                            string sql = @"INSERT OR REPLACE INTO Days (Date,WorkedAmmount,RestedAmmount)
                                             VALUES (@Date,@WorkedAmmount,@RestedAmmount)";

                            await connection.ExecuteAsync(sql, day);
                            await transaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            //rols back if failed
                            await transaction.RollbackAsync();
                            MainWindow.instance.MainDispatcher.Invoke(() =>
                            {
                                MainWindow.ShowErrorBox("Failed to write to database", $"This can be caused by a missing database file: {ex.Message}");
                            });
                        }
                        finally
                        {
                            await connection.CloseAsync();
                        }
                    }
                }
            }
            finally
            {
                //free semaphore so other methods can run
                _semaphore.Release();
            }
        }

        public static async Task<DayData> ReadDay(string date)
        {
            DayData? retrivedDay = null;
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes to db continue

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //start connection
                    await connection.OpenAsync();

                    //execute the sql to get the day
                    try
                    {
                        string sql = @$"SELECT * FROM Days
                                      WHERE Date = @Date";
                        retrivedDay = connection.QueryFirstOrDefault<DayData>(sql,new {Date = date });
                    }
                    catch (Exception ex)
                    {
                        //close app if failed
                        MainWindow.instance.MainDispatcher.Invoke(() =>
                        {
                            MainWindow.ShowErrorBox("Failed to read from database", $"This can be caused by a missing database file: {ex.Message}");
                        });
                    }
                    finally
                    {
                        await connection.CloseAsync();
                    }
                }
            }
            finally
            {
                //release sempahore so other methods can run
                _semaphore.Release();
            }

            if(retrivedDay == null)
            {
                retrivedDay = new();
            }
            
            retrivedDay.ConvertSaveDataToUsableData();

            return retrivedDay;
        }

        public static async Task<int> ReadCountInMonth(string month)
        {
            int returncount = 0;
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes to db continue

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //start connection
                    await connection.OpenAsync();

                    //execute the sql to get the day
                    try
                    {
                        string sql = @$"SELECT COUNT(*) AS row_count
                                        FROM Days WHERE date LIKE @Pattern";
                        returncount = await connection.QueryFirstOrDefaultAsync<int>(sql,new {Pattern = $"{month}%%" });
                    }
                    catch (Exception ex)
                    {
                        //close app if failed
                        MainWindow.instance.MainDispatcher.Invoke(() =>
                        {
                            MainWindow.ShowErrorBox("Failed to read from database", $"This can be caused by a missing database file: {ex.Message}");
                        });
                    }
                    finally
                    {
                        await connection.CloseAsync();
                    }
                }
            }
            finally
            {
                //release sempahore so other methods can run
                _semaphore.Release();
            }
            return returncount;
        }

        public static async Task<List<DayData>> ReadMonth(string Month = "",string year = "")
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes continue

            //list of filtered residences
            List<DayData> ReturnDays = new();

            try
            {
                //write to console it started reading database
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //open connection
                    await connection.OpenAsync();

                    try
                    {
                        //depending on passed arguments chose between 2 sql statements
                        string sql;
                        if(string.IsNullOrEmpty(Month) || string.IsNullOrWhiteSpace(year))
                        {
                            sql = @$"SELECT * from Days";
                        }
                        else
                        {
                            //problem with Pattern
                            sql = @$"SELECT * from Days 
                                     WHERE Date Like @Pattern";
                        }


                        //wait for return, and pass the return to a Residence class
                        ReturnDays = (await connection.QueryAsync<DayData>(sql, new { Pattern = $"{Month}%{year}" })).ToList();

                    }
                    catch (Exception ex)
                    {
                        //close app if failed
                        MainWindow.instance.MainDispatcher.Invoke(() =>
                        {
                            MainWindow.ShowErrorBox("Failed to read from database", $"This can be caused by a missing database file: {ex.Message}");
                        });
                    }
                    finally
                    {
                        await connection.CloseAsync();
                    }
                }
            }
            finally
            {
                //release semaphore so other methods could run
                _semaphore.Release();
            }
            foreach(DayData day in ReturnDays)
            {
                day.ConvertSaveDataToUsableData();
            }
            //return filtered res
            return ReturnDays;
        }

        public static async Task<DayData> GetMaxValue(string value,string Month = "", string year = "")
        {
            DayData? retrivedDay = null;
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes to db continue

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //start connection
                    await connection.OpenAsync();

                    //execute the sql to get the day
                    try
                    {
                        string sql;
                        if (string.IsNullOrEmpty(Month) || string.IsNullOrEmpty(year))
                        {
                            sql = @$"SELECT * FROM Days 
                             WHERE CAST({value} as INT) = 
                            (SELECT MAX(CAST({value} as INT)) FROM Days)";
                        }
                        else
                        {
                            sql = @$"SELECT * FROM Days 
                             WHERE CAST({value} as INT) = 
                             (SELECT MAX(CAST({value} as INT)) FROM Days
                             WHERE Date LIKE '{Month}%{year}%')";
                        }

                        //Removed Parameters, it breaks receiving data.

                        //DynamicParameters parm = new();
                        //parm.Add("Collum",value,System.Data.DbType.String);
                        //parm.Add("template",$"{Month}%{year}",System.Data.DbType.String);

                        retrivedDay = connection.QueryFirstOrDefault<DayData>(sql);

                    }
                    catch (Exception ex)
                    {
                        //close app if failed
                        MainWindow.instance.MainDispatcher.Invoke(() =>
                        {
                            MainWindow.ShowErrorBox("Failed to read from database", $"This can be caused by a missing database file: {ex.Message}");
                        });
                    }
                    finally
                    {
                        await connection.CloseAsync();
                    }
                }
            }
            finally
            {
                //release sempahore so other methods can run
                _semaphore.Release();
            }

            if (retrivedDay == null)
            {
                retrivedDay = new();
            }

            retrivedDay.ConvertSaveDataToUsableData();

            return retrivedDay;
        }

        public static async Task<ProcessActivity> GetMostActiveActivity(string value, string Month = "", string year = "")
        {
            ProcessActivity? retrivedDay = null;
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes to db continue

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //start connection
                    await connection.OpenAsync();

                    //execute the sql to get the day
                    try
                    {
                        string sql;
                        if (string.IsNullOrEmpty(Month) || string.IsNullOrEmpty(year))
                        {
                            sql = @$"SELECT * FROM Days 
                                     WHERE @Value = 
                                    (SELECT MAX(@Value) FROM Days)";
                        }
                        else
                        {

                            sql = @$"SELECT * FROM Days 
                                     WHERE @Value = 
                                     (SELECT MAX(@Value) FROM Days
                                     WHERE Date Like @Pattern)";
                        }
                        retrivedDay = connection.QueryFirstOrDefault<ProcessActivity>(sql, new { Value = value, Pattern = $"{Month}%{year}" });
                    }
                    catch (Exception ex)
                    {
                        //close app if failed
                        MainWindow.instance.MainDispatcher.Invoke(() =>
                        {
                            MainWindow.ShowErrorBox("Failed to read from database", $"This can be caused by a missing database file: {ex.Message}");
                        });
                    }
                    finally
                    {
                        await connection.CloseAsync();
                    }
                }
            }
            finally
            {
                //release sempahore so other methods can run
                _semaphore.Release();
            }

            retrivedDay.ConvertSaveDataToUsableData();

            return retrivedDay;
        }

    }
}