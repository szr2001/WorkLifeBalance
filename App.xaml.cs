using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels;
using WorkLifeBalance.Services;
using System.Diagnostics;
using System.Windows;
using System.IO;
using Serilog;
using System;
using System.Threading.Tasks;

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
            services.AddSingleton(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainMenuVM>()
            });
            services.AddSingleton(provider => new SecondWindow
            {
                DataContext = provider.GetService<SecondWindowVM>()
            });
            services.AddSingleton<DataStorageFeature>();
            services.AddSingleton<ActivityTrackerFeature>();
            services.AddSingleton<IdleCheckerFeature>();
            services.AddSingleton<StateCheckerFeature>();
            services.AddSingleton<TimeTrackerFeature>();
            services.AddSingleton<SqlDataAccess>();
            services.AddSingleton(_configuration);
            services.AddSingleton<DataBaseHandler>();
            services.AddSingleton<LowLevelHandler>();
            services.AddSingleton<AppStateHandler>();
            services.AddSingleton<SqlLiteDatabaseIntegrity>();
            services.AddSingleton<AppTimer>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISecondWindowService, SecondWindowService>();
            services.AddSingleton<IFeaturesServices, FeaturesService>();

            //factory method for ViewModelBase.
            services.AddSingleton<Func<Type, ViewModelBase>>(serviceProvider => viewModelType => (ViewModelBase)serviceProvider.GetRequiredService(viewModelType));
            //factory method for Features.
            services.AddSingleton<Func<Type, FeatureBase>>(serviceProvider => featureBase => (FeatureBase)serviceProvider.GetRequiredService(featureBase));

            services.AddSingleton<BackgroundProcessesViewPageVM>();
            services.AddSingleton<MainMenuVM>();
            services.AddSingleton<OptionsPageVM>();
            services.AddSingleton<SecondWindowVM>();
            services.AddSingleton<SettingsPageVM>();
            services.AddSingleton<ViewDataPageVM>();
            services.AddSingleton<ViewDayDetailsPageVM>();
            services.AddSingleton<ViewDaysPageVM>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LowLevelHandler lowHandler = _servicesProvider.GetRequiredService<LowLevelHandler>();

            //move show a popup and then if the user pressses ok, restart, if not, close app
            //if (!lowHandler.IsRunningAsAdmin())
            //{
            //    RestartApplicationWithAdmin();
            //    return;
            //}

            bool isDebug = _configuration.GetValue<bool>("Debug");
            if (isDebug)
            {
                lowHandler.EnableConsole();
                Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            }
            _ = InitializeApp();
        }

        private async Task InitializeApp()
        {
            DataStorageFeature dataStorageFeature = _servicesProvider.GetRequiredService<DataStorageFeature>();

            SqlLiteDatabaseIntegrity sqlLiteDatabaseCreator = _servicesProvider.GetRequiredService<SqlLiteDatabaseIntegrity>();

            await sqlLiteDatabaseCreator.CheckDatabaseIntegrity();

            await dataStorageFeature.LoadData();
            
            AppTimer appTimer = _servicesProvider.GetRequiredService<AppTimer>();
            
            //set app ready so timers can start
            dataStorageFeature.IsAppReady = true;

            IFeaturesServices featuresService = _servicesProvider.GetRequiredService<IFeaturesServices>();
            featuresService.AddFeature<DataStorageFeature>();
            featuresService.AddFeature<TimeTrackerFeature>();
            featuresService.AddFeature<ActivityTrackerFeature>();

            //check settings to see if you need to add some features
            if (dataStorageFeature.Settings.AutoDetectWorkingC)
            {
                featuresService.AddFeature<StateCheckerFeature>();
            }

            //starts the main timer
            appTimer.StartTick();

            var mainWindow = _servicesProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            Log.Information("------------------App Initialized------------------");
        }

        private void RestartApplicationWithAdmin()
        {
            var DataStorageFeature = _servicesProvider.GetRequiredService<DataStorageFeature>();
            var psi = new ProcessStartInfo
            {
                FileName = DataStorageFeature.AppExePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            Process.Start(psi);
            Current.Shutdown();
        }
    }
}
