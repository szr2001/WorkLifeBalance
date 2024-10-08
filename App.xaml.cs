using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Diagnostics;
using System.Windows;
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

        private DataStorageFeature? dataStorageFeature;
        private ActivityTrackerFeature? activityTrackerFeature;
        private IdleCheckerFeature? idleCheckerFeature;
        private StateCheckerFeature? stateCheckerFeature;
        private TimeTrackerFeature? timeTrackerFeature;
        private DataBaseHandler? dataBaseHandler;
        private AppTimer? appTimer;
        private LowLevelHandler? lowLevelHandler;
        private readonly ServiceProvider _servicesProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<MainWindow>();

            _servicesProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            lowLevelHandler = new();
            if (!lowLevelHandler.IsRunningAsAdmin())
            {
                RestartApplicationWithAdmin();
                return;
            }

            if (Debug)
            {
                lowLevelHandler.EnableConsole();
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

            dataBaseHandler = new DataBaseHandler();

            //app features, each handles a specific part of the app
            //and can be subscribed to the AppTimer
            dataStorageFeature = new(dataBaseHandler);
            activityTrackerFeature = new(lowLevelHandler, dataStorageFeature);
            idleCheckerFeature = new();
            stateCheckerFeature = new();
            appTimer = new(dataStorageFeature);
            timeTrackerFeature = new(appTimer, dataStorageFeature);



            SecondWindowVM secondWindowVM = new();
            MainMenuVM mainMenuVM = new(appTimer, lowLevelHandler, dataStorageFeature, timeTrackerFeature);

            SecondWindow secondWindow = new(secondWindowVM);
            MainWindow mainWindow = new(mainMenuVM);

            dataStorageFeature.OnLoaded += InitializeApp;
            mainWindow.Show();

            _ = dataStorageFeature.LoadData();
        }

        //initialize app called when data was loaded
        private void InitializeApp()
        {
            //set app ready so timers can start
            dataStorageFeature!.IsAppReady = true;

            //subscribe features to the main timer
            appTimer!.Subscribe(timeTrackerFeature!.AddFeature());
            appTimer.Subscribe(dataStorageFeature.AddFeature());
            appTimer.Subscribe(activityTrackerFeature!.AddFeature());

            //check settings to see if you need to add some features
            if (dataStorageFeature.Settings.AutoDetectWorkingC)
            {
                appTimer.Subscribe(stateCheckerFeature!.AddFeature());
            }

            //starts the main timer
            appTimer.StartTick();

            //SetAppState(AppState.Resting);
            //ApplyAutoDetectWorking();

            Log.Information("------------------App Initialized------------------");
        }

        private void RestartApplicationWithAdmin()
        {
            var psi = new ProcessStartInfo
            {
                FileName = dataStorageFeature?.AppExePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            Process.Start(psi);
            Current.Shutdown();
        }
    }
}
