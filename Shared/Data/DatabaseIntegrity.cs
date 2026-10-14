using System.IO;
using Serilog;
using WorkLifeBalance.Shared.Native;

namespace WorkLifeBalance.Shared.Data
{
    public class DatabaseIntegrity
    {
        private readonly SqlDataAccess _dataAccess;
        private readonly DataStorageFeature _dataStorage;
        private readonly StartupTask _startupTask;
        private readonly Dictionary<string, Func<Task>> _migrations;

        public DatabaseIntegrity(SqlDataAccess dataAccess, DataStorageFeature dataStorage, StartupTask startupTask)
        {
            _dataAccess = dataAccess;
            _dataStorage = dataStorage;
            _startupTask = startupTask;

            _migrations = new()
            {
                ["2.0.7"] = Update2_0_7To2_0_8,
                ["2.0.6"] = Update2_0_6To2_0_7,
                ["2.0.5"] = () => UpdateDatabaseVersion("2.0.6"),
                ["2.0.4"] = () => UpdateDatabaseVersion("2.0.5"),
                ["2.0.3"] = () => UpdateDatabaseVersion("2.0.4"),
                ["2.0.2"] = () => UpdateDatabaseVersion("2.0.3"),
                ["2.0.1"] = () => UpdateDatabaseVersion("2.0.2"),
                ["2.0.0"] = () => UpdateDatabaseVersion("2.0.1"),
                ["Beta"] = UpdateBetaTo2_0_0,
            };
        }

        public async Task CheckDatabaseIntegrity()
        {
            if (IsDatabasePresent())
            {
                await UpdateOrCreateDatabase(await GetDatabaseVersion());
            }
            else
            {
                Log.Warning("Database file not found, generating one");
                await CreateLatestDatabase();
            }

            Log.Information("Database is up to date!");
        }

        private async Task UpdateOrCreateDatabase(string version)
        {
            if (version == _dataStorage.Settings.Version)
            {
                _startupTask.Regenerate();
                return;
            }

            if (_migrations.TryGetValue(version, out var migrate))
            {
                await migrate();
                var databaseVersion = await GetDatabaseVersion();
                Log.Warning("Database updated to version {Version}", databaseVersion);
                await UpdateOrCreateDatabase(databaseVersion);
            }
            else
            {
                Log.Error("Database corrupted, marking it as corrupted and generating a new one");
                MarkDatabaseAsCorrupted();
                await CreateLatestDatabase();
            }
        }

        private void MarkDatabaseAsCorrupted()
        {
            try
            {
                if (!File.Exists(_dataAccess.DatabasePath))
                    return;

                File.Copy(_dataAccess.DatabasePath, $@"{_dataAccess.DatabaseDirectory}\RecordedData_{DateTime.UtcNow:yyyy_MM_dd_HH_mm_ss}_Corrupted.db", false);
                File.Delete(_dataAccess.DatabasePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to mark the database as corrupted");
            }
        }

        private async Task<string> GetDatabaseVersion()
        {
            try
            {
                return (await _dataAccess.ReadDataAsync<string, dynamic>("SELECT Version from Settings", new { })).FirstOrDefault() ?? "Beta";
            }
            catch
            {
                Log.Warning("Database Version column not found, indicating a Beta version database");
                return "Beta";
            }
        }

        private async Task UpdateDatabaseVersion(string version)
        {
            var hasSettingsRow = await _dataAccess.ExecuteAsync("SELECT COUNT(1) FROM Settings", new { }) > 0;
            var sql = hasSettingsRow ? "UPDATE Settings SET Version = @Version" : "INSERT INTO Settings (Version) VALUES (@Version)";
            await _dataAccess.ExecuteAsync<dynamic>(sql, new { Version = version });
        }

        private bool IsDatabasePresent()
        {
            if (!Directory.Exists(_dataAccess.DatabaseDirectory))
            {
                Log.Warning("{Directory} does not exist, creating it", _dataAccess.DatabaseDirectory);
                Directory.CreateDirectory(_dataAccess.DatabaseDirectory);
            }

            return File.Exists(_dataAccess.DatabasePath);
        }

        private async Task CreateLatestDatabase()
        {
            await _dataAccess.ExecuteAsync("""
                CREATE TABLE "Activity" (
                    "Date" TEXT NOT NULL,
                    "Process" TEXT CHECK((Process IS NOT NULL AND Url IS NULL) OR (Url IS NOT NULL AND Process IS NULL)),
                    "URL" TEXT,
                    "TimeSpent" TEXT NOT NULL);
                """, new { });

            await _dataAccess.ExecuteAsync("""
                CREATE TABLE "Days" (
                    "Date" TEXT NOT NULL UNIQUE,
                    "WorkedAmmount" TEXT NOT NULL DEFAULT '000000',
                    "IdleAmmount" TEXT NOT NULL DEFAULT '000000',
                    "RestedAmmount" TEXT NOT NULL DEFAULT '000000',
                    PRIMARY KEY("Date"));
                """, new { });

            await _dataAccess.ExecuteAsync("""
                CREATE TABLE "Settings" (
                    "LastTimeOpened" TEXT,
                    "StartWithWindows" INTEGER,
                    "SaveInterval" INTEGER,
                    "AutoDetectInterval" INTEGER,
                    "AutoDetectIdleInterval" INTEGER,
                    "MinimizeToTray" INTEGER,
                    "Version" TEXT);
                """, new { });

            await _dataAccess.ExecuteAsync("""
                CREATE TABLE "WorkingWindows" (
                    "WorkingStateWindows" TEXT NOT NULL UNIQUE);
                """, new { });

            await _dataAccess.ExecuteAsync("""
                CREATE TABLE "WorkingUrls" (
                    "WorkingStateUrl" TEXT NOT NULL UNIQUE);
                """, new { });

            await UpdateDatabaseVersion(_dataStorage.Settings.Version);
        }

        private async Task Update2_0_7To2_0_8()
        {
            await _dataAccess.ExecuteAsync("""
                CREATE TABLE "Activity_new" (
                    "Date" TEXT NOT NULL,
                    "TimeSpent" TEXT NOT NULL,
                    "Url" TEXT NULL,
                    "Process" TEXT CHECK((Process IS NOT NULL AND Url IS NULL) OR (Url IS NOT NULL AND Process IS NULL)));
                INSERT INTO Activity_new SELECT * FROM Activity;
                DROP TABLE Activity;
                ALTER TABLE Activity_new RENAME TO Activity;
                CREATE TABLE "WorkingUrls" ("WorkingStateUrl" TEXT NOT NULL UNIQUE);
                """, new { });
            await UpdateDatabaseVersion("2.0.8");
        }

        private async Task Update2_0_6To2_0_7()
        {
            await _dataAccess.ExecuteAsync("ALTER TABLE Settings ADD COLUMN MinimizeToTray INT NOT NULL DEFAULT 0;", new { });
            await UpdateDatabaseVersion("2.0.7");
        }

        private async Task UpdateBetaTo2_0_0()
        {
            await _dataAccess.ExecuteAsync("""ALTER TABLE Settings ADD COLUMN Version TEXT NOT NULL Default "2.0.0";""", new { });
            await _dataAccess.ExecuteAsync("ALTER TABLE Days ADD COLUMN IdleAmmount TEXT NOT NULL Default '000000';", new { });
            await _dataAccess.ExecuteAsync("ALTER TABLE Settings DROP COLUMN StartUpCorner;", new { });
            await _dataAccess.ExecuteAsync("ALTER TABLE Settings DROP COLUMN AutoDetectWorking;", new { });
            await _dataAccess.ExecuteAsync("ALTER TABLE Settings DROP COLUMN AutoDetectIdle;", new { });
        }
    }
}
