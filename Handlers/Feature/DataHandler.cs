using System;
using System.Threading.Tasks;
using WorkLifeBalance.Data;
using WorkLifeBalance.Handlers;
using static WorkLifeBalance.Handlers.TimeHandler;

namespace WorkLifeBalance.Handlers.Feature
{
    public class DataHandler : FeatureBase
    {
        private static DataHandler? _instance;
        public static DataHandler Instance 
        {
            get 
            {
                if(_instance == null)
                {
                    _instance = new DataHandler();
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

        public DataHandler()
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

            Console.WriteLine("Saving day");
            await DataBaseHandler.WriteDay(TodayData);
            Console.WriteLine("Saving settings");
            await DataBaseHandler.WriteSettings(Settings);
            Console.WriteLine("Saving autostate");
            await DataBaseHandler.WriteAutoSateData(AutoChangeData);

            OnSaved?.Invoke();

            IsAppSaving = false;

            Console.WriteLine("Save Complete!");
        }

        public async Task LoadData()
        {
            if(IsAppLoading) return;

            IsAppLoading = true;

            OnLoading?.Invoke();

            //TodayData.DateC might not exist yet. if it gives an error try creating a new TodayDate.Now
            TodayData = await DataBaseHandler.ReadDay(TodayData.DateC.ToString("MMddyyyy"));
            Settings = await DataBaseHandler.ReadSettings();
            AutoChangeData = await DataBaseHandler.ReadAutoStateData(TodayData.DateC.ToString("MMddyyyy"));

            OnLoaded?.Invoke();

            IsAppLoading = false;
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
                await Task.Delay(Settings.SaveInterval * 60000,CancelTokenS.Token);
                await SaveData();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsSaveTriggered = false;
            }
        }
    }
}
