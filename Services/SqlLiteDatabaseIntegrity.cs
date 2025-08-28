﻿using WorkLifeBalance.Services.Feature;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Serilog;
using System;
using Microsoft.Extensions.Configuration;

namespace WorkLifeBalance.Services
{
    public class SqlLiteDatabaseIntegrity
    {
        private readonly SqlDataAccess sqlDataAccess;
        private readonly DataStorageFeature dataStorageFeature;
        private readonly LowLevelHandler lowLevelHandler;
        private readonly IConfiguration configuration;
        private readonly Dictionary<string, Func<Task>> DatabaseUpdates;
        private string dbdirectoryPath = "";
        private string dbPath = "";

        public SqlLiteDatabaseIntegrity(SqlDataAccess sqlDataAccess, DataStorageFeature dataStorageFeature,
            LowLevelHandler lowLevelHandler, IConfiguration configuration)
        {
            this.sqlDataAccess = sqlDataAccess;
            this.dataStorageFeature = dataStorageFeature;

            string? overridedDirectory = configuration.GetValue<string>("OverrideDbDirectory");

            if (string.IsNullOrEmpty(overridedDirectory))
            {
                dbdirectoryPath =
                    @$"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\WorkLifeBalance";
            }
            else
            {
                dbdirectoryPath = overridedDirectory;
            }

            dbPath = @$"{dbdirectoryPath}\RecordedData.db";

            DatabaseUpdates = new()
            {
                { "2.0.7", Update2_0_7To2_0_8 },
                { "2.0.6", Update2_0_6To2_0_7 },
                { "2.0.5", Update2_0_5To2_0_6 },
                { "2.0.4", Update2_0_4To2_0_5 },
                { "2.0.3", Update2_0_3To2_0_4 },
                { "2.0.2", Update2_0_2To2_0_3 },
                { "2.0.1", Update2_0_1To2_0_2 },
                { "2.0.0", Update2_0_0To2_0_1 },
                { "Beta", UpdateBetaTo2_0_0V }
            };
            this.lowLevelHandler = lowLevelHandler;
            this.configuration = configuration;
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
                    Log.Error($"Database corupted, marking it as Corupted and genereting a new one");
                    //if we don't have an update for that version, it means the databse is really old or bugged
                    //so we mark it as such and call the update with the current versiom, which will just create the databse
                    MarkDbAsCorupted();
                    await CreateLatestDatabase();
                }

                return;
            }

            lowLevelHandler.RegenerateStartupShortcut();
        }

        private void MarkDbAsCorupted()
        {
            try
            {
                if (File.Exists(dbPath))
                {
                    File.Copy(dbPath,
                        @$"{dbdirectoryPath}\RecordedData_{DateTime.UtcNow:yyyy_MM_dd_HH_mm_ss}_Corrupted.db", false);

                    File.Delete(dbPath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private async Task<string> GetDatabaseVersion()
        {
            string version = "Beta";

            string sql = "SELECT Version from Settings";

            try
            {
                var result = (await sqlDataAccess.ReadDataAsync<string, dynamic>(sql, new { })).FirstOrDefault();
                if (result != null)
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

            if (ExistVersionRow)
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
            if (!Directory.Exists(dbdirectoryPath))
            {
                Log.Warning($"{dbdirectoryPath} does not exist, creating it");
                Directory.CreateDirectory(dbdirectoryPath);
            }

            return File.Exists(dbPath);
        }

        private async Task CreateLatestDatabase()
        {
            string createActivitySQL =
                """
                    CREATE TABLE "Activity" 
                    (
                	"Date"	TEXT NOT NULL,
                	"Process"	TEXT CHECK((Process IS NOT NULL AND Url IS NULL) OR (Url IS NOT NULL AND Process IS NULL)),
                	"URL" TEXT,
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
                     "MinimizeToTray"	INTEGER,
                     "Version"	TEXT);
                 """;
            await sqlDataAccess.ExecuteAsync(createSettingsSQL, new { });

            string createWorkingWindowsSQL =
                """
                    CREATE TABLE "WorkingWindows" (
                	"WorkingStateWindows"	TEXT NOT NULL UNIQUE);
                """;
            await sqlDataAccess.ExecuteAsync(createWorkingWindowsSQL, new { });

            string createWorkingPageSQL =
                """
                    CREATE TABLE "WorkingUrls" (
                	"WorkingStateUrl"	TEXT NOT NULL UNIQUE);
                """;
            await sqlDataAccess.ExecuteAsync(createWorkingPageSQL, new { });
            await UpdateDatabaseVersion(dataStorageFeature.Settings.Version);
        }

        private async Task Update2_0_7To2_0_8()
        {
            string sqlCreateUrlColumnForActivity =
                """
                CREATE TABLE "Activity_new" (
                	"Date"	TEXT NOT NULL,
                	"TimeSpent"	TEXT NOT NULL, 
                	"Url" TEXT NULL,
                	"Process" TEXT CHECK((Process IS NOT NULL AND Url IS NULL) OR (Url IS NOT NULL AND Process IS NULL))
                	);
                INSERT INTO Activity_new SELECT * FROM Activity;
                DROP TABLE Activity;
                ALTER TABLE Activity_new RENAME TO Activity;
                CREATE TABLE "WorkingUrls" ("WorkingStateUrl" TEXT NOT NULL UNIQUE);
                """;
            await sqlDataAccess.ExecuteAsync(sqlCreateUrlColumnForActivity, new { });
            await UpdateDatabaseVersion("2.0.8");
        }

        private async Task Update2_0_6To2_0_7()
        {
            string sqlCreateMinimizeToTrayTable =
                """
                    ALTER TABLE Settings
                    ADD COLUMN MinimizeToTray INT NOT NULL DEFAULT 0;
                """;
            await sqlDataAccess.ExecuteAsync(sqlCreateMinimizeToTrayTable, new { });
            await UpdateDatabaseVersion("2.0.7");
        }

        private async Task Update2_0_5To2_0_6()
        {
            await UpdateDatabaseVersion("2.0.6");
        }

        private async Task Update2_0_4To2_0_5()
        {
            await UpdateDatabaseVersion("2.0.5");
        }

        private async Task Update2_0_3To2_0_4()
        {
            await UpdateDatabaseVersion("2.0.4");
        }

        private async Task Update2_0_2To2_0_3()
        {
            await UpdateDatabaseVersion("2.0.3");
        }

        private async Task Update2_0_1To2_0_2()
        {
            await UpdateDatabaseVersion("2.0.2");
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