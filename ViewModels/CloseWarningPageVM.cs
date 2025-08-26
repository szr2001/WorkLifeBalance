using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Threading.Tasks;
using System.Windows;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.ViewModels
{
    public partial class CloseWarningPageVM : SecondWindowPageVMBase
    {
        private readonly DataStorageFeature dataStorageFeature;
        public CloseWarningPageVM(DataStorageFeature dataStorageFeature)
        {
            PageHeight = 160;
            PageWidth = 280;
            PageName = "Close Warning";
            this.dataStorageFeature = dataStorageFeature;
        }

        [RelayCommand]
        private void CloseApp()
        {
            if (dataStorageFeature.IsClosingApp) return;

            dataStorageFeature.IsClosingApp = true;

            Log.Information("------------------App Shuting Down------------------");

            _ = Task.Run(async () =>
            {
                await dataStorageFeature.SaveData();
                await Log.CloseAndFlushAsync();

                App.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            });
        }

        public override Task OnPageClosingAsync() => Task.CompletedTask;

        public override Task OnPageOpeningAsync(object? args = null) => Task.CompletedTask;
    }
}
