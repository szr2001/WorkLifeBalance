using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Threading;
using System.Linq;
using Serilog;
using System;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace WorkLifeBalance.Services
{
    public class SqlDataAccess
    {
        private readonly IConfiguration configuration;
        private readonly SemaphoreSlim _semaphore = new(1);
        //use config to read the connection string
        private string ConnectionString = "";

        public SqlDataAccess(IConfiguration configuration)
        {
            this.configuration = configuration;

            string? overridedDirectory = configuration.GetValue<string>("OverrideDbDirectory");

            if (string.IsNullOrEmpty(overridedDirectory))
            {
                ConnectionString = @$"Data Source={Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\WorkLifeBalance\RecordedData.db;Version=3;";
            }
            else
            {
                ConnectionString = @$"Data Source={overridedDirectory}\RecordedData.db;Version=3;";
            }
        }

        public async Task<int> ExecuteAsync<T>(string sql, T parameters)
        {
            await _semaphore.WaitAsync();
            try
            {
                using (SQLiteConnection connection = new(ConnectionString))
                {
                    await connection.OpenAsync();

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var rows = await connection.ExecuteScalarAsync<int>(sql, parameters);
                            await transaction.CommitAsync();
                            return rows;
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Execute SQL error with sql: {sql} Error: {ex}");
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<int> WriteDataAsync<T>(string sql, T parameters)
        {
            await _semaphore.WaitAsync();
            try
            {
                using (SQLiteConnection connection = new(ConnectionString))
                {
                    await connection.OpenAsync();
                    
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var rows =  await connection.ExecuteAsync(sql, parameters);
                            await transaction.CommitAsync();

                            return rows;
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Write data to database error with sql {sql}, parameters {parameters}: Error {ex}");
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<T>> ReadDataAsync<T, U>(string sql, U parameters)
        {
            await _semaphore.WaitAsync();
            try
            {
                using (SQLiteConnection connection = new(ConnectionString))
                {
                    await connection.OpenAsync();
                    try
                    {
                        var rows = await connection.QueryAsync<T>(sql, parameters);
                        return rows.ToList();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Read data from database error with sql {sql}, parameters {parameters}: Error {ex}");
                        throw;
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
