using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Numerics;
using System.Threading.Tasks;
using WorkLifeBalance.Models;

namespace WorkLifeBalance.ViewModels
{
    public partial class UpdatePageVM : SecondWindowPageVMBase
    {
        [ObservableProperty]
        public string version = "Error";

        [ObservableProperty]
        public string updateLog = "Error";

        private VersionData? Vdata;

        public UpdatePageVM()
        {
            windowPageName = "Update";
            RequiredWindowSize = new Vector2(400, 350);
        }

        public override Task OnPageOppeningAsync(object? args = null)
        {
            if (args is VersionData data)
            {
                Vdata = data;

                Version = Vdata.Version!;
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
                System.Diagnostics.Process.Start(Vdata.DownloadLink!);
            }
        }
    }
}
