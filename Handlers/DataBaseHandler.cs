using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkLifeBalance.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WorkLifeBalance.HandlerClasses
{
    public static class DataBaseHandler // add Backup and LoadBackup
    {
        //semaphore to ensure only one method can write to the database at once
        private static SemaphoreSlim _semaphore = new(1);
        //connection string for the db
        private static readonly string ConnectionString = @$"Data Source={Directory.GetCurrentDirectory()}\RecordedData.db;Version=3;";

        public static async Task WriteSettings(WLBSettings sett)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //open connection
                    await connection.OpenAsync();
                    //if no one writes continue
                    await Task.Run(async () =>
                    {
                        using (SQLiteTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                string sql = @"UPDATE Settings 
                                              SET LastTimeOpened = @LastTimeOpened,
                                              StartWithWindows = @StartWithWindows,
                                              StartUpCorner = @StartUpCorner,
                                              SaveInterval = @SaveInterval
                                              LIMIT 1";

                                connection.Execute(sql, sett);
                                await transaction.CommitAsync();
                            }
                            catch (Exception ex)
                            {
                                //rols back if failed
                                await transaction.RollbackAsync();
                                MainWindow.MainDispatcher.Invoke(() =>
                                {
                                    MainWindow.ShowErrorBox("Failed to write to database", $"This can be caused by a missing database file: {ex.Message}");
                                });
                            }
                        }
                    });

                    //close connection
                    await connection.CloseAsync();
                }
            }
            finally
            {
                //free semaphore so other methods can run
                _semaphore.Release();
            }
        }

        public static async Task<WLBSettings> ReadSettings()
        {
            WLBSettings retrivedSettings = new();
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

                        retrivedSettings = connection.QueryFirstOrDefault<WLBSettings>(sql);
                    }
                    catch (Exception ex)
                    {
                        MainWindow.MainDispatcher.Invoke(() =>
                        {
                            MainWindow.ShowErrorBox("Failed to write to database", $"This can be caused by a missing database file: {ex.Message}");
                        });
                    }
                    //close connection
                    await connection.CloseAsync();
                }
            }
            finally
            {
                //release sempahore so other methods can run
                _semaphore.Release();
            }

            //returns count
            return retrivedSettings;
        }

        public static async Task WriteDay(DayData day)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //open connection
                    await connection.OpenAsync();
                    //if no one writes continue
                    await Task.Run(async () =>
                    {
                        using (SQLiteTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                //write data
                                string sql = @"INSERT OR REPLACE INTO Days (Date,WorkedAmmount,RestedAmmount,StudiedAmmount)
                                             VALUES (@Date,@WorkedAmmount,@RestedAmmount,@StudiedAmmount)";

                                await connection.ExecuteAsync(sql, day);
                                await transaction.CommitAsync();
                            }
                            catch (Exception ex)
                            {
                                //rols back if failed
                                await transaction.RollbackAsync();
                                MainWindow.MainDispatcher.Invoke(() => 
                                {
                                    MainWindow.ShowErrorBox("Failed to write to database",$"This can be caused by a missing database file: {ex.Message}");
                                });
                            }
                        }
                    });

                    //close connection
                    await connection.CloseAsync();
                }
            }
            finally
            {
                //free semaphore so other methods can run
                _semaphore.Release();
            }
        }

        public static async Task<DayData> ReadDay(string Date)
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
                                      WHERE Date = '{Date}'";
                        retrivedDay = connection.QueryFirstOrDefault<DayData>(sql);
                    }
                    catch (Exception ex)
                    {
                        //close app if failed
                        MainWindow.MainDispatcher.Invoke(() =>
                        {
                            MainWindow.ShowErrorBox("Failed to read from database", $"This can be caused by a missing database file: {ex.Message}");
                        });
                    }
                    //close connection
                    await connection.CloseAsync();
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

            return retrivedDay;
        }
    }
}