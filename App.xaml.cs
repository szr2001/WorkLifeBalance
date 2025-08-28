using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels;
using WorkLifeBalance.Services;
using System.Windows;
using System.IO;
using Serilog;
using System;
using System.Threading.Tasks;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _servicesProvider;
        private readonly IConfiguration _configuration;

        public App()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();

            ConfigureServices(services);

            _servicesProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IWindowService<SecondWindowPageVMBase>, SecondWindowService>();
            services.AddSingleton<IWindowService<PopupWindowPageVMBase>, PopupWindowService>();
            services.AddSingleton<IFeaturesServices, FeaturesService>();
            services.AddSingleton<IUpdateCheckerService, UpdateCheckerService>();
            services.AddSingleton<IWindowService<MainWindowDetailsPageBase>, MainWindowDetailsService>();
            services.AddSingleton<ISoundService, SoundService>();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<SecondWindow>();
            services.AddSingleton<PopupWindow>();
            
            services.AddSingleton<DataStorageFeature>();
            services.AddSingleton<ActivityTrackerFeature>();
            services.AddSingleton<IdleCheckerFeature>();
            services.AddSingleton<StateCheckerFeature>();
            services.AddSingleton<ForceStateFeature>();
            services.AddSingleton<TimeTrackerFeature>();
            services.AddSingleton<ForceWorkFeature>();

            services.AddSingleton<SqlDataAccess>();
            services.AddSingleton(_configuration);
            services.AddSingleton<DataBaseHandler>();
            services.AddSingleton<LowLevelHandler>();
            services.AddSingleton<AppStateHandler>();
            services.AddSingleton<SqlLiteDatabaseIntegrity>();
            services.AddSingleton<AppTimer>();

            //factory method for ViewModelBase.
            services.AddSingleton<Func<Type, ViewModelBase>>(serviceProvider =>
                viewModelType => (ViewModelBase)serviceProvider.GetRequiredService(viewModelType));
            //factory method for Features.
            services.AddSingleton<Func<Type, FeatureBase>>(serviceProvider =>
                featureBase => (FeatureBase)serviceProvider.GetRequiredService(featureBase));

            services.AddSingleton<BackgroundProcessesViewPageVM>();
            services.AddSingleton<MainWindowVM>();
            services.AddSingleton<ForceWorkPageVM>();
            services.AddSingleton<OptionsPageVM>();
            services.AddSingleton<SecondWindowBaseVM>();
            services.AddSingleton<PopupWindowBaseVM>();
            services.AddSingleton<CloseWarningPageVM>();
            services.AddSingleton<SettingsPageVM>();
            services.AddSingleton<ViewDataPageVM>();
            services.AddSingleton<UpdatePageVM>();
            services.AddSingleton<LoadingPageVM>();
            services.AddSingleton<ViewDayDetailsPageVM>();
            services.AddSingleton<ViewDaysPageVM>();
            services.AddSingleton<AddUrlPageVM>();

            services.AddSingleton<ForceWorkMainMenuDetailsPageVM>();
            services.AddSingleton<ForceStateMainMenuDetailsPageVM>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LowLevelHandler lowHandler = _servicesProvider.GetRequiredService<LowLevelHandler>();

#if DEBUG
            lowHandler.EnableConsole();
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

        private async Task InitializeApp()
        {
            //request the secondWindow so we will have it there to subscribe to events
            _servicesProvider.GetRequiredService<SecondWindow>();
            _servicesProvider.GetRequiredService<PopupWindow>();
            
            DataStorageFeature dataStorageFeature = _servicesProvider.GetRequiredService<DataStorageFeature>();

            SqlLiteDatabaseIntegrity sqlLiteDatabaseIntegrity =
                _servicesProvider.GetRequiredService<SqlLiteDatabaseIntegrity>();

            IUpdateCheckerService updateCheckerService = _servicesProvider.GetRequiredService<IUpdateCheckerService>();

            await updateCheckerService.CheckForUpdate();

            await sqlLiteDatabaseIntegrity.CheckDatabaseIntegrity();

            await dataStorageFeature.LoadData();

            AppTimer appTimer = _servicesProvider.GetRequiredService<AppTimer>();

            //set app ready so timers can start
            dataStorageFeature.IsAppReady = true;

            IFeaturesServices featuresService = _servicesProvider.GetRequiredService<IFeaturesServices>();
            featuresService.AddFeature<DataStorageFeature>();
            featuresService.AddFeature<TimeTrackerFeature>();
            featuresService.AddFeature<ActivityTrackerFeature>();
            featuresService.AddFeature<IdleCheckerFeature>();
            featuresService.AddFeature<StateCheckerFeature>();

            //starts the main timer
            appTimer.StartTick();

            _servicesProvider.GetRequiredService<MainWindow>().Show();

            Log.Information("------------------App Initialized------------------");
        }
    }
}