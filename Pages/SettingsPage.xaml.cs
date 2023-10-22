using IWshRuntimeLibrary;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using WorkLifeBalance.Data;
using File = System.IO.File;
using WorkLifeBalance.Windows;
using Path = System.IO.Path;
using WorkLifeBalance.Handlers;
using System.Threading.Tasks;

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : SecondWindowPageBase
    {
        int Saveinterval = 5;
        int AutoDetectInterval = 5;
        public SettingsPage(object? args) : base(args)
        {
            InitializeComponent();
            RequiredWindowSize = new Vector2(250,320);
            pageNme = "Settings";

            switch (DataHandler.Instance.Settings.StartUpCornerC)
            {
                case AnchorCorner.TopLeft:
                    TopLeftBtn.IsChecked = true;
                    break;
                case AnchorCorner.TopRight:
                    TopRightBtn.IsChecked = true;
                    break;
                case AnchorCorner.BootomLeft:
                    BottomLeftBtn.IsChecked = true;
                    break;
                case AnchorCorner.BottomRight:
                    BottomRightBtn.IsChecked = true;
                    break;
            }

            AutosaveT.Text = DataHandler.Instance.Settings.SaveInterval.ToString();

            AutoDetectT.Text = DataHandler.Instance.Settings.AutoDetectInterval.ToString();

            StartWithWInBtn.IsChecked = DataHandler.Instance.Settings.StartWithWindowsC;

            AutoDetectWorkingBtn.IsChecked = DataHandler.Instance.Settings.AutoDetectWorkingC;

            if (DataHandler.Instance.Settings.AutoDetectWorkingC)
            {
                ExpandAutoDetectArea();
            }
        }

        private void ChangeAutosaveDelay(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(AutosaveT.Text) || !int.TryParse(AutosaveT.Text , out _) )
            {
                AutosaveT.Text = 5.ToString();
                Saveinterval = 5;
                return;
            }
            
            Saveinterval = int.Parse(AutosaveT.Text);
        }
        private void ChangeAutoDetectDelay(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(AutoDetectT.Text) || int.TryParse(AutoDetectT.Text, out _))
            {
                AutoDetectT.Text = 5.ToString();
                AutoDetectInterval = 5;
                return;
            }

            AutoDetectInterval = int.Parse(AutoDetectT.Text);
        }
        private void SetBotLeftStartup(object sender, RoutedEventArgs e)
        {
            BottomRightBtn.IsChecked = false;
            TopRightBtn.IsChecked = false;
            TopLeftBtn.IsChecked = false;
            DataHandler.Instance.Settings.StartUpCornerC = AnchorCorner.BootomLeft;
        }

        private void SetBotRightStartup(object sender, RoutedEventArgs e)
        {
            BottomLeftBtn.IsChecked = false;
            TopRightBtn.IsChecked = false;
            TopLeftBtn.IsChecked = false;
            DataHandler.Instance.Settings.StartUpCornerC = AnchorCorner.BottomRight;
        }

        private void SetUpRightStartup(object sender, RoutedEventArgs e)
        {
            BottomLeftBtn.IsChecked = false;
            BottomRightBtn.IsChecked = false;
            TopLeftBtn.IsChecked = false;
            DataHandler.Instance.Settings.StartUpCornerC = AnchorCorner.TopRight;
        }

        private void SetUpLeftStartup(object sender, RoutedEventArgs e)
        {
            BottomLeftBtn.IsChecked = false;
            BottomRightBtn.IsChecked = false;
            TopRightBtn.IsChecked = false;
            DataHandler.Instance.Settings.StartUpCornerC = AnchorCorner.TopLeft;
        }

        private void SetStartWithWin(object sender, RoutedEventArgs e)
        {
            DataHandler.Instance.Settings.StartWithWindowsC = (bool)StartWithWInBtn.IsChecked;
        }

        public override async Task ClosePageAsync()
        {
            ApplyStartToWindows();

            DataHandler.Instance.Settings.SaveInterval = Saveinterval;

            DataHandler.Instance.Settings.AutoDetectInterval = AutoDetectInterval;

            await DataHandler.Instance.SaveData();
        }

        private void ApplyStartToWindows()
        {
            string appName = "WorkLifeBalance";
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string appPath = Path.Combine(appDirectory, $"{appName}.exe");

            string startupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), $"{appName}.lnk");

            if (DataHandler.Instance.Settings.StartWithWindowsC)
            {
                CreateShortcut(appPath,appDirectory, startupFolderPath);
            }
            else
            {
                DeleteShortcut(startupFolderPath);
            }
        }

        private void CreateShortcut(string appPath,string appdirectory, string startupfolder)
        {
            // Create a shortcut if it doesn't exist
            if (!File.Exists(startupfolder))
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(startupfolder);
                shortcut.TargetPath = appPath;
                shortcut.WorkingDirectory = appdirectory;
                shortcut.Save();
            }
        }

        private void DeleteShortcut(string startupfolder)
        {
            if (File.Exists(startupfolder))
            {
                File.Delete(startupfolder);
            }
        }

        private void ConfigureAutoDetectBtn(object sender, RoutedEventArgs e)
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.BackgroundProcesses);
        }

        private void ToggleAutoDetectWorking(object sender, RoutedEventArgs e)
        {
            if (AutoDetectWorkingBtn.IsChecked == true)
            {
                TimeHandler.Instance.OnTimerTick += AutomaticStateChangerHandler.Instance.TriggerWorkDetect;
                ExpandAutoDetectArea();
            }
            else
            {
                TimeHandler.Instance.OnTimerTick -= AutomaticStateChangerHandler.Instance.TriggerWorkDetect;
                ContractAutoDetectArea();
            }
        }
        private void ExpandAutoDetectArea()
        {
            AutoToggleWorkingPanel.Height = 80;
            DataHandler.Instance.Settings.AutoDetectWorkingC = true;
            MainWindow.instance.CheckAutoDetectWorking();
        }
        private void ContractAutoDetectArea()
        {
            AutoToggleWorkingPanel.Height = 0;
            DataHandler.Instance.Settings.AutoDetectWorkingC = false;
            MainWindow.instance.CheckAutoDetectWorking();
        }
    }
}
