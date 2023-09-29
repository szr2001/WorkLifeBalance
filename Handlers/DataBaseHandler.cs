using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WorkLifeBalance.HandlerClasses
{
    public static class DataBaseHandler // add Backup and LoadBackup
    {
        //semaphore to ensure only one method can write to the database at once
        private static SemaphoreSlim _semaphore = new(1);
        //connection string for the db
        private static readonly string ConnectionString = "Data Source=.\\RecordedData.db;Version=3;";

        public static async Task WriteSettings(WLBSettings res)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            try
            {
                Console.WriteLine("Writing To FavResidences Table");
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
                                string sql = @"INSERT INTO FavResidences (Url)
                                             VALUES (@Url)";

                                connection.Execute(sql, res);
                                await transaction.CommitAsync();
                            }
                            catch (Exception e)
                            {
                                //rols back if failed
                                Console.WriteLine($"Error writing to FavResidences Table: {e.Message}");
                                await transaction.RollbackAsync();
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

        public static async Task<WLBSettings> ReadDay(WLBSettings res)
        {
            WLBSettings retrivedSettings = new();
            await _semaphore.WaitAsync();
            try
            {
                Console.WriteLine("Writing To FavResidences Table");
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
                                string sql = @"INSERT INTO FavResidences (Url)
                                             VALUES (@Url)";

                                connection.Execute(sql, res);
                                await transaction.CommitAsync();
                            }
                            catch (Exception e)
                            {
                                //rols back if failed
                                Console.WriteLine($"Error writing to FavResidences Table: {e.Message}");
                                await transaction.RollbackAsync();
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
            return retrivedSettings;
        }

        public static async Task WriteDay(DayData res)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            try
            {
                Console.WriteLine("Writing To FavResidences Table");
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
                                string sql = @"INSERT INTO FavResidences (Url)
                                             VALUES (@Url)";

                                connection.Execute(sql, res);
                                await transaction.CommitAsync();
                            }
                            catch (Exception e)
                            {
                                //rols back if failed
                                Console.WriteLine($"Error writing to FavResidences Table: {e.Message}");
                                await transaction.RollbackAsync();
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

        public static async Task<DayData> ReadDay(DayData res)
        {
            DayData retrivedDay = new();
            await _semaphore.WaitAsync();
            try
            {
                Console.WriteLine("Writing To FavResidences Table");
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
                                string sql = @"INSERT INTO FavResidences (Url)
                                             VALUES (@Url)";

                                connection.Execute(sql, res);
                                await transaction.CommitAsync();
                            }
                            catch (Exception e)
                            {
                                //rols back if failed
                                Console.WriteLine($"Error writing to FavResidences Table: {e.Message}");
                                await transaction.RollbackAsync();
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
            return retrivedDay;
        }
    }
}