using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Navigation;

namespace WorkLifeBalance.Features.CloseApp
{
    public partial class CloseWarningViewModel : SecondWindowPageBase
    {
        private readonly DataStorageFeature _dataStorage;

        public CloseWarningViewModel(DataStorageFeature dataStorage)
        {
            _dataStorage = dataStorage;
            PageWidth = 320;
            PageHeight = 160;
            PageName = "Close Warning";
        }

        // Saves before closing so the last minutes are not lost
        [RelayCommand]
        private void CloseApp()
        {
            if (_dataStorage.IsClosingApp)
                return;

            _dataStorage.IsClosingApp = true;
            Log.Information("------------------App Shuting Down------------------");

            _ = Task.Run(async () =>
            {
                await _dataStorage.SaveData();
                await Log.CloseAndFlushAsync();
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
            });
        }
    }
}
