using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Diagnostics;
using System.Windows;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private bool Debug = true;
        private readonly ServiceProvider _servicesProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();

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
            services.AddSingleton<DataBaseHandler>();
            services.AddSingleton<LowLevelHandler>();
            services.AddSingleton<AppTimer>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISecondWindowService, SecondWindowService>();
            services.AddSingleton<Func<Type, ViewModelBase>>(serviceProvider => viewModelType => (ViewModelBase)serviceProvider.GetRequiredService(viewModelType));

            services.AddSingleton<BackgroundProcessesViewPageVM>();
            services.AddSingleton<MainMenuVM>();
            services.AddSingleton<OptionsPageVM>();
            services.AddSingleton<SecondWindowVM>();
            services.AddSingleton<SettingsPageVM>();
            services.AddSingleton<ViewDataPageVM>();
            services.AddSingleton<ViewDayDetailsPageVM>();
            services.AddSingleton<ViewDaysPageVM>();

            _servicesProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LowLevelHandler lowHandler = _servicesProvider.GetRequiredService<LowLevelHandler>();
            DataStorageFeature dataStorageFeature = _servicesProvider.GetRequiredService<DataStorageFeature>();

            if (lowHandler.IsRunningAsAdmin())
            {
                RestartApplicationWithAdmin();
                return;
            }

            if (Debug)
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

            dataStorageFeature.OnLoaded += InitializeApp;

            var mainWindow = _servicesProvider.GetRequiredService<MainWindow>(); 
            mainWindow.Show();

            _ = dataStorageFeature.LoadData();
        }

        //initialize app called when data was loaded
        private void InitializeApp()
        {
            DataStorageFeature dataStorageFeature = _servicesProvider.GetRequiredService<DataStorageFeature>();
            AppTimer appTimer= _servicesProvider.GetRequiredService<AppTimer>();
            TimeTrackerFeature timeTrackerFeature = _servicesProvider.GetRequiredService<TimeTrackerFeature>();
            ActivityTrackerFeature activityTrackerFeature = _servicesProvider.GetRequiredService<ActivityTrackerFeature>();
            StateCheckerFeature stateCheckerFeature = _servicesProvider.GetRequiredService<StateCheckerFeature>();
            
            //set app ready so timers can start
            dataStorageFeature.IsAppReady = true;

            //subscribe features to the main timer
            appTimer.Subscribe(timeTrackerFeature.AddFeature());
            appTimer.Subscribe(dataStorageFeature.AddFeature());
            appTimer.Subscribe(activityTrackerFeature.AddFeature());

            //check settings to see if you need to add some features
            if (dataStorageFeature.Settings.AutoDetectWorkingC)
            {
                appTimer.Subscribe(stateCheckerFeature.AddFeature());
            }

            //starts the main timer
            appTimer.StartTick();

            //SetAppState(AppState.Resting);
            //ApplyAutoDetectWorking();

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
