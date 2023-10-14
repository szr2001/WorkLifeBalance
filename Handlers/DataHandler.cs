using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public bool IsClosingApp = false;
        public bool IsAppReady = false;
        public DayData? TodayData = null;
        public WLBSettings AppSettings = new();

        public delegate void DataEvent();
        public event DataEvent? OnLoading;
        public event DataEvent? OnLoaded;
        public event DataEvent? OnSaving;
        public event DataEvent? OnSaved;

        public async Task SaveData()
        {
            OnSaving?.Invoke();

            await DataBaseHandler.WriteDay(TodayData);
            await DataBaseHandler.WriteSettings(AppSettings);

            OnSaved?.Invoke();
        }

        public async Task LoadData()
        {
            OnLoading?.Invoke();

            TodayData = await DataBaseHandler.ReadDay(DateOnly.FromDateTime(DateTime.Now).ToString("MMddyyyy"));
            AppSettings = await DataBaseHandler.ReadSettings();

            OnLoaded?.Invoke();
        }
    }
}
