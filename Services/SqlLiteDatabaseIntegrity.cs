using WorkLifeBalance.Services.Feature;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Serilog;
using System;

namespace WorkLifeBalance.Services
{
    public class SqlLiteDatabaseIntegrity
    {
        private readonly SqlDataAccess sqlDataAccess;
        private readonly DataStorageFeature dataStorageFeature;
        private readonly Dictionary<string, Func<Task>> DatabaseUpdates;
        private string databasePath = "";

        public SqlLiteDatabaseIntegrity(SqlDataAccess sqlDataAccess, DataStorageFeature dataStorageFeature)
        {
            this.sqlDataAccess = sqlDataAccess;
            this.dataStorageFeature = dataStorageFeature;

            databasePath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\WorkLifeBalance\RecordedData.db";

            DatabaseUpdates = new()
            {
                { "2.0.0", Update2_0_0To2_0_1},
                { "Beta", UpdateBetaTo2_0_0V}
            };
        }


        public async Task CheckDatabaseIntegrity()
        {
            if (IsDatabasePresent())
            {
                string version = await GetDatabaseVersion();
                await UpdateOrCreateDatabase(version);
            }
            else
            {
                Log.Warning("Database file not found, genereting one");
                await CreateLatestDatabase();
            }
            Log.Information($"Database is up to date!");
        }

        private async Task UpdateOrCreateDatabase(string version)
        {
            //if the database doesn't have the latest version
            if (version != dataStorageFeature.Settings.Version)
            {
                //else check if the version exists in the update list
                if (DatabaseUpdates.TryGetValue(version, out Func<Task>? DBUpdateMethod))
                {
                    //if yes, execute the update, updating the database
                    await DBUpdateMethod();
                    //then we get the updated database version
                    string databaseVersion = await GetDatabaseVersion();
                    //if its not up to date, then we call this method again, to give it the next update
                    Log.Warning($"Database Updated to version {databaseVersion}");
                  
                    await UpdateOrCreateDatabase(databaseVersion);
                }
                else
                {
                    Log.Error($"Database corupted, re-genereting it");
                    //if we don't have an update for that version, it means the databse is really old or bugged
                    //so we delete it and call the update with the current versiom, which will just create the databse
                    DeleteDatabaseFile();
                    await CreateLatestDatabase();
                }
            }
        }

        private void DeleteDatabaseFile()
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }

        private async Task<string> GetDatabaseVersion()
        {
            string version = "Beta";

            string sql = "SELECT Version from Settings";

            try
            {
                var result = (await sqlDataAccess.ReadDataAsync<string, dynamic>(sql, new { })).FirstOrDefault();
                if(result != null)
                {
                    version = result;
                }
            }
            catch            
            {
                Log.Warning("Database Version collumn not found, indicatin Beta version database");
            }

            return version;
        }

        private async Task UpdateDatabaseVersion(string version)
        {
            string sql = "SELECT COUNT(1) FROM Settings";
            bool ExistVersionRow = (await sqlDataAccess.ExecuteAsync(sql, new { })) > 0;

            string updateVersionSQL = "";

            if(ExistVersionRow)
            {
                updateVersionSQL = "UPDATE Settings SET Version = @Version";
            }
            else
            {
                updateVersionSQL = "INSERT INTO Settings (Version) VALUES (@Version)";
            }

            await sqlDataAccess.ExecuteAsync<dynamic>(updateVersionSQL, new { Version = version });
        }

        private bool IsDatabasePresent()
        {
            return File.Exists(databasePath);
        }
        
        private async Task CreateLatestDatabase()
        {
            string createActivitySQL =
                """
                    CREATE TABLE "Activity" 
                    (
                	"Date"	TEXT NOT NULL,
                	"Process"	TEXT NOT NULL,
                	"TimeSpent"	TEXT NOT NULL);
                """;
            await sqlDataAccess.ExecuteAsync(createActivitySQL, new { });

            string createDaysSQL =
                """
                    CREATE TABLE "Days" (
                	"Date"	TEXT NOT NULL UNIQUE,
                	"WorkedAmmount"	TEXT NOT NULL DEFAULT '000000',
                    "IdleAmmount" TEXT NOT NULL DEFAULT '000000',
                	"RestedAmmount"	TEXT NOT NULL DEFAULT '000000',
                	PRIMARY KEY("Date"));
                """;
            await sqlDataAccess.ExecuteAsync(createDaysSQL, new { });

            string createSettingsSQL =
                $"""
                    CREATE TABLE "Settings" (
                	"LastTimeOpened"	TEXT,
                	"StartWithWindows"	INTEGER,
                	"SaveInterval"	INTEGER,
                	"AutoDetectInterval"	INTEGER,
                	"AutoDetectIdleInterval"	INTEGER,
                	"Version"	TEXT NOT NULL DEFAULT "{dataStorageFeature.Settings.Version}");
                """;
            await sqlDataAccess.ExecuteAsync(createSettingsSQL, new { });

            string createWorkingWindowsSQL =
                """
                    CREATE TABLE "WorkingWindows" (
                	"WorkingStateWindows"	TEXT NOT NULL UNIQUE);
                """;
            await sqlDataAccess.ExecuteAsync(createWorkingWindowsSQL, new { });
        }

        private async Task Update2_0_0To2_0_1()
        {
            await UpdateDatabaseVersion("2.0.1");
        }

        private async Task UpdateBetaTo2_0_0V()
        {
            string sqlCreateVersionTable =
                """
                    ALTER TABLE Settings
                    ADD COLUMN Version TEXT NOT NULL Default "2.0.0";
                """;
            string sqlCreateIdleAmmountTable =
                """
                    ALTER TABLE Days
                    ADD COLUMN IdleAmmount TEXT NOT NULL Default '000000';
                """;
            string sqlRemoveStartupCornerTable =
                """
                    ALTER TABLE Settings
                    DROP COLUMN StartUpCorner;
                """;
            string sqlRemoveDetectStateBoolTable =
                """
                    ALTER TABLE Settings
                    DROP COLUMN AutoDetectWorking;
                """;
            string sqlRemoveDetectIdleBoolTable =
                """
                    ALTER TABLE Settings
                    DROP COLUMN AutoDetectIdle;
                """;
            await sqlDataAccess.ExecuteAsync(sqlCreateVersionTable, new { });
            await sqlDataAccess.ExecuteAsync(sqlCreateIdleAmmountTable, new { });

            await sqlDataAccess.ExecuteAsync(sqlRemoveStartupCornerTable, new { });
            await sqlDataAccess.ExecuteAsync(sqlRemoveDetectStateBoolTable, new { });
            await sqlDataAccess.ExecuteAsync(sqlRemoveDetectIdleBoolTable, new { });
        }
    }
}
