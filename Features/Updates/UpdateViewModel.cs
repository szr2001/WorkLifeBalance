using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using WorkLifeBalance.Shared.Native;
using WorkLifeBalance.Shared.Navigation;

namespace WorkLifeBalance.Features.Updates
{
    public partial class UpdateViewModel : SecondWindowPageBase
    {
        private readonly LowLevelHandler _lowLevelHandler;

        [ObservableProperty]
        private string _version = "Error";

        [ObservableProperty]
        private string _updateLog = "Error";

        private VersionData? _versionData;

        public UpdateViewModel(LowLevelHandler lowLevelHandler)
        {
            _lowLevelHandler = lowLevelHandler;
            PageWidth = 340;
            PageHeight = 400;
            PageName = "Update Available";
        }

        public override Task OnPageOpeningAsync(object? args = null)
        {
            if (args is VersionData data)
            {
                _versionData = data;
                Version = $"New Version: {data.Version}";
                UpdateLog = data.UpdateLog!;
            }
            else
            {
                Log.Error("UpdateViewModel opened with wrong arguments, args != VersionData");
            }

            return Task.CompletedTask;
        }

        // Opens the download page and closes the app so the new version can be installed
        [RelayCommand]
        private void GoToDownload()
        {
            if (_versionData == null)
                return;

            _lowLevelHandler.OpenLink(_versionData.DownloadLink!);
            Application.Current.Shutdown();
        }
    }
}
