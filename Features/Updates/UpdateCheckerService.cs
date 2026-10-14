using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Navigation;

namespace WorkLifeBalance.Features.Updates
{
    // Compares the version in the gist (UpdateDataAdress in appsettings.json) with ours and shows the update page
    public class UpdateCheckerService : IUpdateCheckerService
    {
        private readonly IWindowService<SecondWindowPageBase> _secondWindowService;
        private readonly DataStorageFeature _dataStorage;
        private readonly string _updateAddress;

        public UpdateCheckerService(IWindowService<SecondWindowPageBase> secondWindowService, IConfiguration configuration, DataStorageFeature dataStorage)
        {
            _secondWindowService = secondWindowService;
            _dataStorage = dataStorage;
            _updateAddress = configuration.GetValue<string>("UpdateDataAdress") ?? "";
        }

        public async Task CheckForUpdate()
        {
            if (string.IsNullOrEmpty(_updateAddress))
            {
                Log.Error("UpdateDataAdress is empty inside the appsettings.json");
                return;
            }

            var latest = await DownloadLatestVersionData();
            if (latest == null)
            {
                Log.Error("Retrieved VersionData is null, problems with fetching update data");
                return;
            }

            var current = _dataStorage.Settings.Version;
            if (latest.Version != current)
            {
                Log.Warning("New Update Available! Current Version: {Current}, Latest Version: {Latest}", current, latest.Version);
                await _secondWindowService.OpenWith<UpdateViewModel>(latest);
                return;
            }

            Log.Information("App is up to date! Current Version: {Current}, Latest Version: {Latest}", current, latest.Version);
        }

        private async Task<VersionData?> DownloadLatestVersionData()
        {
            using var client = new HttpClient();
            try
            {
                return JsonConvert.DeserializeObject<VersionData>(await client.GetStringAsync(_updateAddress));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching Latest Version data");
                return null;
            }
        }
    }
}
