using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WorkLifeBalance.Data;
using WorkLifeBalance.HandlerClasses;

namespace WorkLifeBalance.Handlers
{
    public class DataHandler
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

        public bool IsAppSaving { get; private set; } = false;
        public bool IsAppLoading { get; private set; } = false;

        public bool IsClosingApp = false;
        public bool IsAppReady = false;

        DateOnly TodayDate = DateOnly.FromDateTime(DateTime.Now);

        public TimmerState AppTimmerState = TimmerState.Resting;

        public DayData TodayData = new();
        public AppSettings Settings = new();
        public AutoStateChangeData AutoChangeData = new();

        public delegate void DataEvent();
        public event DataEvent? OnLoading;
        public event DataEvent? OnLoaded;
        public event DataEvent? OnSaving;
        public event DataEvent? OnSaved;

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

            TodayData = await DataBaseHandler.ReadDay(TodayDate.ToString("MMddyyyy"));
            Settings = await DataBaseHandler.ReadSettings();
            AutoChangeData = await DataBaseHandler.ReadAutoStateData(TodayDate.ToString("MMddyyyy"));

            OnLoaded?.Invoke();

            IsAppLoading = false;
        }

        private bool IsSaveTriggered = false;
        public async void TriggerSaveData()
        {
            if (IsSaveTriggered) return;

            IsSaveTriggered = true;

            await Task.Delay(Settings.SaveInterval * 60000);
            await SaveData();

            IsSaveTriggered = false;
        }
    }
}
