using Serilog;
using WorkLifeBalance.Shared.Scheduling;

namespace WorkLifeBalance.Shared.Data
{
    public class DataStorageFeature : FeatureBase
    {
        private readonly AppDataStore _store;

        public DataStorageFeature(AppDataStore store)
        {
            _store = store;
        }

        public event Action? OnLoading;
        public event Action? OnLoaded;
        public event Action? OnSaving;
        public event Action? OnSaved;

        public bool IsAppSaving { get; private set; }
        public bool IsAppLoading { get; private set; }
        public bool IsClosingApp { get; set; }
        public bool IsAppReady { get; set; }

        public DayData TodayData { get; set; } = new();
        public AppSettingsData Settings { get; set; } = new();
        public AutoStateChangeData AutoChangeData { get; set; } = new();

        public async Task SaveData()
        {
            if (IsAppSaving)
                return;

            Log.Information("Saving...");
            await CheckIsNewDay();

            IsAppSaving = true;
            OnSaving?.Invoke();

            await _store.WriteDay(TodayData);
            await _store.WriteSettings(Settings);
            await _store.WriteAutoStateData(AutoChangeData);

            OnSaved?.Invoke();
            IsAppSaving = false;
            Log.Information("Save Complete!");
        }

        public async Task LoadData()
        {
            if (IsAppLoading)
                return;

            IsAppLoading = true;
            OnLoading?.Invoke();

            var today = TodayData.DateC.ToString(StoredFormat.Date);
            Log.Information("Loading Day");
            TodayData = await _store.ReadDay(today);
            Log.Information("Loading Settings");
            Settings = await _store.ReadSettings();
            Log.Information("Loading Activities");
            AutoChangeData = await _store.ReadAutoStateData(today);

            OnLoaded?.Invoke();
            IsAppLoading = false;
            Log.Information("Load Complete!");
        }

        protected override Func<Task> ReturnFeatureMethod() => TriggerSaveData;

        private Task CheckIsNewDay()
        {
            // TODO: compare TodayData with DateTime.Now and, when the day changed, save it and start a new DayData
            return Task.CompletedTask;
        }

        private async Task TriggerSaveData()
        {
            if (IsFeatureRunning)
                return;

            IsFeatureRunning = true;
            try
            {
                await Task.Delay(Settings.SaveInterval * 60000, CancelTokenSource.Token);
                await SaveData();
            }
            catch (TaskCanceledException taskCancel)
            {
                Log.Information("DataStorage: {Message}", taskCancel.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DataStorage");
            }
            finally
            {
                IsFeatureRunning = false;
            }
        }
    }
}
