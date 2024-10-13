using Serilog;
using System;
using System.Threading.Tasks;
using WorkLifeBalance.Models;

namespace WorkLifeBalance.Services.Feature
{
    public class DataStorageFeature : FeatureBase
    {
        public readonly string AppVersion = "2.0.0";
        public string AppName;
        public string AppDirectory;
        public string AppExePath;

        public bool IsAppSaving { get; private set; } = false;
        public bool IsAppLoading { get; private set; } = false;

        public bool IsClosingApp = false;
        public bool IsAppReady = false;

        public DayData TodayData = new();
        public AppSettingsData Settings = new();
        public AutoStateChangeData AutoChangeData = new();

        public event Action? OnLoading;
        public event Action? OnLoaded;
        public event Action? OnSaving;
        public event Action? OnSaved;
        private readonly DataBaseHandler dataBaseHandler;
        public DataStorageFeature(DataBaseHandler dataBaseHandler)
        {
            AppName = "WorkLifeBalance";
            AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
            AppExePath = $"{AppDirectory}/{AppName}.exe";
            this.dataBaseHandler = dataBaseHandler;
        }
        public async Task SaveData()
        {
            if (IsAppSaving) return;

            IsAppSaving = true;

            OnSaving?.Invoke();

            Log.Information($"Saving Day");
            await dataBaseHandler.WriteDay(TodayData);
            Log.Information($"Saving Settings");
            await dataBaseHandler.WriteSettings(Settings);
            Log.Information($"Saving Activities");
            await dataBaseHandler.WriteAutoSateData(AutoChangeData);

            OnSaved?.Invoke();

            IsAppSaving = false;

            Log.Information($"Save Complete!");
        }

        public async Task LoadData()
        {
            if (IsAppLoading) return;

            IsAppLoading = true;

            OnLoading?.Invoke();

            Log.Information($"Loading Day");
            TodayData = await dataBaseHandler.ReadDay(TodayData.DateC.ToString("MMddyyyy"));
            Log.Information($"Loading Settings");
            Settings = await dataBaseHandler.ReadSettings();
            Log.Information($"Loading Activities");
            AutoChangeData = await dataBaseHandler.ReadAutoStateData(TodayData.DateC.ToString("MMddyyyy"));

            OnLoaded?.Invoke();

            IsAppLoading = false;
            Log.Information($"Load Complete!");
        }

        protected override Action ReturnFeatureMethod()
        {
            return TriggerSaveData;
        }

        private bool IsSaveTriggered = false;
        private async void TriggerSaveData()
        {
            if (IsSaveTriggered) return;

            IsSaveTriggered = true;

            try
            {
                await Task.Delay(Settings.SaveInterval * 60000, CancelTokenS.Token);
                await SaveData();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"DataStorageFeature timer loop");
            }

            finally
            {
                IsSaveTriggered = false;
            }
        }
    }
}
