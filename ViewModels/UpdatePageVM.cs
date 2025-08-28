using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Threading.Tasks;
using WorkLifeBalance.Models;
using WorkLifeBalance.Services;

namespace WorkLifeBalance.ViewModels
{
    public partial class UpdatePageVM : SecondWindowPageVMBase
    {
        [ObservableProperty]
        private string version = "Error";

        [ObservableProperty]
        private string updateLog = "Error";

        private VersionData? Vdata;
        private readonly LowLevelHandler lowLevelHandler;
        public UpdatePageVM(LowLevelHandler lowLevelHandler)
        {
            this.lowLevelHandler = lowLevelHandler;
            PageName = "Update Available";
            PageHeight = 400;
            PageWidth = 350;
        }

        public override Task OnPageOpeningAsync(object? args = null)
        {
            if (args is VersionData data)
            {
                Vdata = data;

                Version = $"New Version: {Vdata.Version!}";
                UpdateLog = Vdata.UpdateLog!;
            }
            else 
            {
                Log.Error("UpdatePageVm oppened with wrong arguments, args != VersionData");
            }

            return Task.CompletedTask;
        }
        
        public override Task OnPageClosingAsync() => Task.CompletedTask;

        [RelayCommand]
        private void GoToDownload()
        {
            if(Vdata != null)
            {
                lowLevelHandler.OpenLink(Vdata.DownloadLink!);
                App.Current.Shutdown();
            }
        }
    }
}
