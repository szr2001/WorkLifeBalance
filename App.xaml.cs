using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WorkLifeBalance.Features.CloseApp;
using WorkLifeBalance.Features.ForceState;
using WorkLifeBalance.Features.ForceWork;
using WorkLifeBalance.Features.Options;
using WorkLifeBalance.Features.Settings;
using WorkLifeBalance.Features.Statistics;
using WorkLifeBalance.Features.Tracking;
using WorkLifeBalance.Features.Updates;
using WorkLifeBalance.Features.WorkApps;
using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Native;
using WorkLifeBalance.Shared.Navigation;
using WorkLifeBalance.Shared.Scheduling;
using WorkLifeBalance.Shared.Sound;
using WorkLifeBalance.Shell;

namespace WorkLifeBalance
{
    public partial class App : Application
    {
        private readonly ServiceProvider _services;
        private readonly IConfiguration _configuration;

        public App()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            ConfigureServices(services);
            _services = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

#if DEBUG
            _services.GetRequiredService<LowLevelHandler>().EnableConsole();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
#else
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
#endif

            _ = InitializeApp();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_configuration);

            // Shared
            services.AddSingleton<SqlDataAccess>();
            services.AddSingleton<AppDataStore>();
            services.AddSingleton<DatabaseIntegrity>();
            services.AddSingleton<DataStorageFeature>();
            services.AddSingleton<AppTimer>();
            services.AddSingleton<IFeaturesService, FeaturesService>();
            services.AddSingleton<LowLevelHandler>();
            services.AddSingleton<StartupTask>();
            services.AddSingleton<ISoundService, SoundService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<SecondWindowService>();
            services.AddSingleton<IWindowService<SecondWindowPageBase>>(provider => provider.GetRequiredService<SecondWindowService>());
            services.AddSingleton<IWindowService<PopupWindowPageBase>, PopupWindowService>();
            services.AddSingleton<IWindowService<MainWindowDetailsPageBase>, MainWindowDetailsService>();
            services.AddSingleton<LoadingViewModel>();

            // Pages and features are resolved by type (NavigationService, FeaturesService)
            services.AddSingleton<Func<Type, ViewModelBase>>(provider => type => (ViewModelBase)provider.GetRequiredService(type));
            services.AddSingleton<Func<Type, FeatureBase>>(provider => type => (FeatureBase)provider.GetRequiredService(type));

            // Shell
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<SecondWindow>();
            services.AddSingleton<SecondWindowViewModel>();
            services.AddSingleton<PopupWindow>();
            services.AddSingleton<PopupWindowViewModel>();

            // Features
            services.AddSingleton<AppStateHandler>();
            services.AddSingleton<TimeTrackerFeature>();
            services.AddSingleton<ActivityTrackerFeature>();
            services.AddSingleton<StateCheckerFeature>();
            services.AddSingleton<IdleCheckerFeature>();

            services.AddSingleton<ForceStateFeature>();
            services.AddSingleton<ForceStatePanelViewModel>();

            services.AddSingleton<ForceWorkFeature>();
            services.AddSingleton<ForceWorkViewModel>();
            services.AddSingleton<ForceWorkPanelViewModel>();

            services.AddSingleton<WorkAppsViewModel>();
            services.AddSingleton<AddUrlViewModel>();

            services.AddSingleton<StatisticsStore>();
            services.AddSingleton<StatisticsViewModel>();
            services.AddSingleton<DaysViewModel>();
            services.AddSingleton<DayDetailsViewModel>();

            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<OptionsViewModel>();
            services.AddSingleton<IUpdateCheckerService, UpdateCheckerService>();
            services.AddSingleton<UpdateViewModel>();
            services.AddSingleton<CloseWarningViewModel>();
        }

        private async Task InitializeApp()
        {
            // request the windows now so they are there to subscribe to the events
            _services.GetRequiredService<SecondWindow>();
            _services.GetRequiredService<PopupWindow>();

            var dataStorage = _services.GetRequiredService<DataStorageFeature>();

            await _services.GetRequiredService<IUpdateCheckerService>().CheckForUpdate();
            await _services.GetRequiredService<DatabaseIntegrity>().CheckDatabaseIntegrity();
            await dataStorage.LoadData();

            var appTimer = _services.GetRequiredService<AppTimer>();

            // set app ready so timers can start
            dataStorage.IsAppReady = true;

            var featuresService = _services.GetRequiredService<IFeaturesService>();
            featuresService.AddFeature<DataStorageFeature>();
            featuresService.AddFeature<TimeTrackerFeature>();
            featuresService.AddFeature<ActivityTrackerFeature>();
            featuresService.AddFeature<IdleCheckerFeature>();
            featuresService.AddFeature<StateCheckerFeature>();

            appTimer.StartTick();

            _services.GetRequiredService<MainWindow>().Show();
            Log.Information("------------------App Initialized------------------");
        }
    }
}
