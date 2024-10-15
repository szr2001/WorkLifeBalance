using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.Services
{
    public class SqlLiteDatabaseIntegrity
    {
        private readonly SqlDataAccess sqlDataAccess;
        private readonly DataStorageFeature dataStorageFeature;
        private readonly Dictionary<string, Func<Task>> DatabaseUpdates;
        private string databasePath = "";
        private string connectionString = "";

        public SqlLiteDatabaseIntegrity(SqlDataAccess sqlDataAccess, DataStorageFeature dataStorageFeature)
        {
            this.sqlDataAccess = sqlDataAccess;
            this.dataStorageFeature = dataStorageFeature;
            DatabaseUpdates = new()
            {
                { "2.0.0", Create2_0_0V},
                { "Beta", UpdateBetaTo2_0_0V}
            };
        }

        public async Task CheckDatabaseIntegrity()
        {
            databasePath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\WorkLifeBalance\RecordedData.db";
            connectionString = @$"Data Source={databasePath};Version=3;";

            if (IsDatabasePresent())
            {
                string version = await GetDatabaseVersion();
                await UpdateOrCreateDatabase(version);
            }
            else
            {
                Log.Warning("Database file not found, genereting one");
                await DatabaseUpdates[dataStorageFeature.AppVersion]();
            }
        }

        private async Task UpdateOrCreateDatabase(string version)
        {
            //if the database has the correct version, just return
            if (version == dataStorageFeature.AppVersion)
            {
                Log.Information("Database up to date");
                return;
            }
            //else check if the version exists in the update list
            if (DatabaseUpdates.ContainsKey(version))
            {
                //if yes, execute the update, updating the database
                await DatabaseUpdates[version]();
                //then we get the updated database version
                string databaseVersion = await GetDatabaseVersion();
                //if its not up to date, then we call this method again, to give it the next update
                Log.Warning($"Database Updated to version {databaseVersion}");
                if(databaseVersion != dataStorageFeature.AppVersion)
                {
                    //_ = UpdateOrCreateDatabase(databaseVersion);
                }
            }
            else
            {
                Log.Error($"Database corupted, re-genereting it");
                //if we don't have an update for that version, it means the databse is really old or bugged
                //so we delete it and call the update with the current versiom, which will just create the databse
                DeleteDatabaseFile();
                await DatabaseUpdates[dataStorageFeature.AppVersion]();
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
            string updateVersionSQL = "UPDATE Settings SET Version = @Version";
            await sqlDataAccess.ExecuteAsync<dynamic>(updateVersionSQL, new { Version = version });
        }

        private bool IsDatabasePresent()
        {
            return File.Exists(databasePath);
        }

        private async Task UpdateBetaTo2_0_0V()
        {
            string sqlCreateVersionTable =
                """
                    ALTER TABLE Settings
                    ADD COLUMN Version string;
                """;
            await sqlDataAccess.ExecuteAsync(sqlCreateVersionTable, new { });

            await UpdateDatabaseVersion("2.0.0");
        }

        private async Task Create2_0_0V()
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
                	"WorkedAmmount"	TEXT NOT NULL,
                	"RestedAmmount"	TEXT NOT NULL,
                	PRIMARY KEY("Date"));
                """;
            await sqlDataAccess.ExecuteAsync(createDaysSQL, new { });

            string createSettingsSQL =
                """
                    CREATE TABLE "Settings" (
                	"LastTimeOpened"	TEXT,
                	"StartWithWindows"	INTEGER,
                	"AutoDetectWorking"	INTEGER,
                	"AutoDetectIdle"	INTEGER,
                	"StartUpCorner"	INTEGER,
                	"SaveInterval"	INTEGER,
                	"AutoDetectInterval"	INTEGER,
                	"AutoDetectIdleInterval"	INTEGER,
                	"Version"	TEXT);
                """;
            await sqlDataAccess.ExecuteAsync(createSettingsSQL, new { });

            string createWorkingWindowsSQL =
                """
                    CREATE TABLE "WorkingWindows" (
                	    "WorkingStateWindows"	TEXT NOT NULL UNIQUE
                    );
                """;
            await sqlDataAccess.ExecuteAsync(createWorkingWindowsSQL, new { });


            await UpdateDatabaseVersion("2.0.0");
        }
    }
}
