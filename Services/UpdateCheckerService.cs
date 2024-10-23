using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using Newtonsoft.Json;
using WorkLifeBalance.Models;
using System.Net.Http;
using Serilog;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Services
{
    public class UpdateCheckerService : IUpdateCheckerService
    {
        private readonly ISecondWindowService secondWindowService;
        private readonly IConfiguration configuration;
        private readonly DataStorageFeature dataStorageFeature;

        private readonly string UpdateAdress = "";
        public UpdateCheckerService(ISecondWindowService secondWindowService, IConfiguration configuration, DataStorageFeature dataStorageFeature)
        {
            this.secondWindowService = secondWindowService;
            this.dataStorageFeature = dataStorageFeature;
            this.configuration = configuration;

            UpdateAdress = configuration.GetValue<string>("UpdateDataAdress")!;
        }

        public async Task CheckForUpdate()
        {
            if (string.IsNullOrEmpty(UpdateAdress))
            {
                Log.Error("UpdateDataAdress is empty inside the appsettings.json");
                return;
            }

            VersionData? vdata = await DownloadLatestVersionData();

            if(vdata == null)
            {
                Log.Error("Retrieved VersionData is null, problems with fetching update data");
                return;
            }

            if(vdata.Version != dataStorageFeature.Settings.Version)
            {
                Log.Warning($"New Update Available! Current Version: {dataStorageFeature.Settings.Version}, Latest Version: {vdata.Version}");
                await secondWindowService.OpenWindowWith<UpdatePageVM>(vdata);
                return;
            }

            Log.Information($"App is up to date! Current Version: {dataStorageFeature.Settings.Version}, Latest Version: {vdata.Version}");
        }

        private async Task<VersionData?> DownloadLatestVersionData()
        {
            using (HttpClient client = new HttpClient())
            {
                try 
                {
                    string responseContent = await client.GetStringAsync(UpdateAdress);

                    VersionData? gistData = JsonConvert.DeserializeObject<VersionData>(responseContent);
                    return gistData;
                }
                catch (Exception ex)
                {
                    Log.Error($"Error fetching Latest Version data: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
