using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using WorkLifeBalance.Models;

namespace WorkLifeBalance.ViewModels
{
    public partial class UpdatePageVM : SecondWindowPageVMBase
    {
        [ObservableProperty]
        private string version = "Error";

        [ObservableProperty]
        private string updateLog = "Error";

        private VersionData? Vdata;

        public UpdatePageVM()
        {
            windowPageName = "Update Available";
            RequiredWindowSize = new Vector2(400, 350);
        }

        public override Task OnPageOppeningAsync(object? args = null)
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

        [RelayCommand]
        private void GoToDownload()
        {
            if(Vdata != null)
            {
                Process.Start(new ProcessStartInfo(Vdata.DownloadLink!) { UseShellExecute = true });
                App.Current.Shutdown();
            }
        }
    }
}
