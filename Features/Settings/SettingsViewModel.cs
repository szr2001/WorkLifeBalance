using CommunityToolkit.Mvvm.ComponentModel;
using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Native;
using WorkLifeBalance.Shared.Navigation;

namespace WorkLifeBalance.Features.Settings
{
    // The changes are applied and saved when the page closes
    public partial class SettingsViewModel : SecondWindowPageBase
    {
        private readonly DataStorageFeature _dataStorage;
        private readonly StartupTask _startupTask;

        [ObservableProperty]
        private string _version = "";

        [ObservableProperty]
        private int _autoSaveInterval = 5;

        [ObservableProperty]
        private int _autoDetectInterval = 1;

        [ObservableProperty]
        private int _autoDetectIdleInterval = 1;

        [ObservableProperty]
        private bool _startWithWin;

        [ObservableProperty]
        private bool _minimizeToTray;

        public SettingsViewModel(DataStorageFeature dataStorage, StartupTask startupTask)
        {
            _dataStorage = dataStorage;
            _startupTask = startupTask;
            PageWidth = 320;
            PageHeight = 290;
            PageName = "Settings";
            InitializeData();
        }

        private AppSettingsData Settings => _dataStorage.Settings;

        public override async Task OnPageClosingAsync()
        {
            Settings.SaveInterval = AutoSaveInterval;
            Settings.AutoDetectInterval = AutoDetectInterval;
            Settings.AutoDetectIdleInterval = AutoDetectIdleInterval;
            Settings.StartWithWindowsC = StartWithWin;
            Settings.MinimizeToTrayC = MinimizeToTray;
            await _dataStorage.SaveData();

            if (Settings.StartWithWindowsC)
                _startupTask.Create();
            else
                _startupTask.Delete();

            Settings.OnSettingsChanged();
        }

        private void InitializeData()
        {
            Version = $"Version: {Settings.Version}";
            AutoSaveInterval = Settings.SaveInterval;
            AutoDetectInterval = Settings.AutoDetectInterval;
            AutoDetectIdleInterval = Settings.AutoDetectIdleInterval;
            StartWithWin = Settings.StartWithWindowsC;
            MinimizeToTray = Settings.MinimizeToTrayC;
        }
    }
}
