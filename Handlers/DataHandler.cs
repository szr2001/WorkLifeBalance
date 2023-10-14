using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkLifeBalance.Handlers
{
    public class DataHandler
    {
        public static DataHandler Instance;
        public delegate void DataEvent();

        public event DataEvent? OnLoading;
        public event DataEvent? OnLoaded;
        public event DataEvent? OnSaving;
        public event DataEvent? OnSaved;

        public DataHandler(DataEvent? onloaded = null, DataEvent? onsaved = null, DataEvent? onsaving = null, DataEvent? onloading = null)
        {
            if(Instance == null)
            {
                Instance = this;
            }

            OnLoading += onloading;
            OnLoaded += onloaded;
            OnSaving += onsaving;
            OnSaved += onsaved;
        }

        public async Task SaveData()
        {
            OnSaving?.Invoke();


            OnSaved?.Invoke();
        }

        public async Task LoadData()
        {
            OnLoading?.Invoke();


            OnLoaded?.Invoke();
        }
    }
}
