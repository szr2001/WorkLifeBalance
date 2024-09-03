using Serilog;
using System;
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
        private AppTimer? mainTimer;
        private LowLevelHandler? lowLevelHandler;
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

            //app features, each handles a specific part of the app
            //and can be subscribed to the AppTimer
            dataStorageFeature = new();
            activityTrackerFeature = new();
            idleCheckerFeature = new();
            stateCheckerFeature = new();
            timeTrackerFeature = new();

            dataBaseHandler = new DataBaseHandler();

            mainTimer = new();

            SecondWindowVM secondWindowVM = new();
            MainMenuVM mainMenuVM = new(mainTimer, lowLevelHandler, dataStorageFeature, timeTrackerFeature);

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
            mainTimer!.Subscribe(timeTrackerFeature!.AddFeature());
            mainTimer.Subscribe(dataStorageFeature.AddFeature());
            mainTimer.Subscribe(activityTrackerFeature!.AddFeature());

            //check settings to see if you need to add some features
            if (dataStorageFeature.Settings.AutoDetectWorkingC)
            {
                mainTimer.Subscribe(stateCheckerFeature!.AddFeature());
            }

            //starts the main timer
            mainTimer.StartTick();

            //check if auto detect is enabled so you update ui
            SetAppState(AppState.Resting);
            ApplyAutoDetectWorking();

            //asign the todays date
            DateT.Text = $"Today: {DataStorageFeature.Instance.TodayData.DateC.ToString("MM/dd/yyyy")}";
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
