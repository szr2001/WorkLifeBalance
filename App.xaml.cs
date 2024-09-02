using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LowLevelHandler lowLevelHandler = new();
            CheckAdministratorPerms(lowLevelHandler);

            if (Debug)
            {
                lowLevelHandler.EnableConsole();
            }

            DataBaseHandler dataBaseHandler = new DataBaseHandler();
            
            TimeHandler mainTimer = new();

            SecondWindowVM secondWindowVM = new();
            MainMenuVM mainMenuVM = new(mainTimer, lowLevelHandler);

            SecondWindow secondWindow = new(secondWindowVM);
            MainWindow mainWindow = new(mainMenuVM);
            mainWindow.Show();
        }


        private void CheckAdministratorPerms(LowLevelHandler lowLevelHandler)
        {
            if (lowLevelHandler.IsRunningAsAdmin())
            {
                RestartApplicationWithAdmin();
            }
        }

        private void RestartApplicationWithAdmin()
        {
            var psi = new ProcessStartInfo
            {
                FileName = DataStorageFeature.Instance.AppExePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(psi);
                Current.Shutdown();
            }
            catch (Exception ex)
            {
                WorkLifeBalance.MainWindow.ShowErrorBox("The application must be run as Administrator", $"Restart as Administrator Failed: {ex.Message}");
            }
        }
    }
}
