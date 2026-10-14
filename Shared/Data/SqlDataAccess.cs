using System.Data.SQLite;
using Dapper;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace WorkLifeBalance.Shared.Data
{
    public class SqlDataAccess
    {
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly string _connectionString;

        public SqlDataAccess(IConfiguration configuration)
        {
            var overriddenDirectory = configuration.GetValue<string>("OverrideDbDirectory");
            DatabaseDirectory = string.IsNullOrEmpty(overriddenDirectory)
                ? $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\WorkLifeBalance"
                : overriddenDirectory;
            DatabasePath = $@"{DatabaseDirectory}\RecordedData.db";
            _connectionString = $"Data Source={DatabasePath};Version=3;";
        }

        public string DatabaseDirectory { get; }
        public string DatabasePath { get; }

        public async Task<int> ExecuteAsync<T>(string sql, T parameters)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction();
                try
                {
                    var result = await connection.ExecuteScalarAsync<int>(sql, parameters);
                    await transaction.CommitAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Execute SQL error with sql: {Sql}", sql);
                    await transaction.RollbackAsync();
                    throw;
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
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction();
                try
                {
                    var rows = await connection.ExecuteAsync(sql, parameters);
                    await transaction.CommitAsync();
                    return rows;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Write data to database error with sql {Sql}, parameters {Parameters}", sql, parameters);
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<T>> ReadDataAsync<T, TParameters>(string sql, TParameters parameters)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();
                try
                {
                    return (await connection.QueryAsync<T>(sql, parameters)).ToList();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Read data from database error with sql {Sql}, parameters {Parameters}", sql, parameters);
                    throw;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
