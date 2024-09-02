using Serilog;
using System;
using System.Threading.Tasks;
using WorkLifeBalance.Models;
using static WorkLifeBalance.Services.TimeHandler;

namespace WorkLifeBalance.Services.Feature
{
    public class DataStorageFeature : FeatureBase
    {
        private static DataStorageFeature? _instance;
        public static DataStorageFeature Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataStorageFeature();
                }
                return _instance;
            }
        }

        public readonly float AppVersion = 2;
        public string AppName;
        public string AppDirectory;
        public string AppExePath;

        public bool IsAppSaving { get; private set; } = false;
        public bool IsAppLoading { get; private set; } = false;

        public bool IsClosingApp = false;
        public bool IsAppReady = false;

        public DayData TodayData = new();
        public AppSettings Settings = new();
        public AutoStateChangeData AutoChangeData = new();

        public delegate void DataEvent();
        public event DataEvent? OnLoading;
        public event DataEvent? OnLoaded;
        public event DataEvent? OnSaving;
        public event DataEvent? OnSaved;

        public DataStorageFeature()
        {
            AppName = "WorkLifeBalance";
            AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
            AppExePath = $"{AppDirectory}/{AppName}.exe";
        }
        public async Task SaveData()
        {
            if (IsAppSaving) return;

            IsAppSaving = true;

            OnSaving?.Invoke();

            Log.Information($"Saving Day");
            await DataBaseHandler.WriteDay(TodayData);
            Log.Information($"Saving Settings");
            await DataBaseHandler.WriteSettings(Settings);
            Log.Information($"Saving Activities");
            await DataBaseHandler.WriteAutoSateData(AutoChangeData);

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
            TodayData = await DataBaseHandler.ReadDay(TodayData.DateC.ToString("MMddyyyy"));
            Log.Information($"Loading Settings");
            Settings = await DataBaseHandler.ReadSettings();
            Log.Information($"Loading Activities");
            AutoChangeData = await DataBaseHandler.ReadAutoStateData(TodayData.DateC.ToString("MMddyyyy"));

            OnLoaded?.Invoke();

            IsAppLoading = false;
            Log.Information($"Load Complete!");
        }

        protected override TickEvent ReturnFeatureMethod()
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
