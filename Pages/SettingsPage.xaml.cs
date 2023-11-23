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
using WorkLifeBalance.Handlers.Feature;

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : SecondWindowPageBase
    {
        int Saveinterval = 5;
        int AutoDetectInterval = 1;
        int AutoDetectIdleInterval = 1;

        bool AutoDetectWork = false;
        bool AutoDetectIdle = false;
        bool StartWithWin = false;

        public SettingsPage(object? args) : base(args)
        {
            InitializeComponent();
            RequiredWindowSize = new Vector2(250,320);
            pageNme = "Settings";

            ApplySettings();
        }

        //updates ui based on loaded settings
        private void ApplySettings()
        {
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

            VersionT.Text = $"Version: {DataHandler.Instance.AppVersion}";

            AutosaveT.Text = DataHandler.Instance.Settings.SaveInterval.ToString();

            AutoDetectT.Text = DataHandler.Instance.Settings.AutoDetectInterval.ToString();

            AutoDetectIdleT.Text = DataHandler.Instance.Settings.AutoDetectIdleInterval.ToString();

            StartWithWin = DataHandler.Instance.Settings.StartWithWindowsC;
            StartWithWInBtn.IsChecked = StartWithWin;

            AutoDetectWork = DataHandler.Instance.Settings.AutoDetectWorkingC;
            AutoDetectWorkingBtn.IsChecked = AutoDetectWork;

            AutoDetectIdle = DataHandler.Instance.Settings.AutoDetectIdleC;
            AutoDetectIdleBtn.IsChecked = AutoDetectIdle;

            if (DataHandler.Instance.Settings.AutoDetectWorkingC)
            {
                ExpandAutoDetectArea();
            }
            if (DataHandler.Instance.Settings.AutoDetectIdleC)
            {
                ExpandDetectMouseIdleArea();
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
            if (string.IsNullOrEmpty(AutoDetectT.Text) || !int.TryParse(AutoDetectT.Text, out _))
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
            StartWithWin = (bool)StartWithWInBtn.IsChecked;
        }

        //Each page has a close page so the second window can await the page specific cleanups stuff
        //here we wait set the changed values,apply changes and wait for the save to db
        public override async Task ClosePageAsync()
        {
            DataHandler.Instance.Settings.SaveInterval = Saveinterval;

            DataHandler.Instance.Settings.AutoDetectInterval = AutoDetectInterval;

            DataHandler.Instance.Settings.AutoDetectIdleInterval = AutoDetectIdleInterval;

            DataHandler.Instance.Settings.AutoDetectIdleC = AutoDetectIdle;

            DataHandler.Instance.Settings.AutoDetectWorkingC = AutoDetectWork;

            DataHandler.Instance.Settings.StartWithWindowsC = StartWithWin;

            await DataHandler.Instance.SaveData();

            ApplyStartToWindows();

            MainWindow.instance.CheckAutoDetectWorking();
        }

        private void ApplyStartToWindows()
        {
            string startupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), $"{DataHandler.Instance.AppName}.lnk");

            if (DataHandler.Instance.Settings.StartWithWindowsC)
            {
                CreateShortcut(startupFolderPath);
            }
            else
            {
                DeleteShortcut(startupFolderPath);
            }
        }

        private void CreateShortcut(string startupfolder)
        {
            if (!File.Exists(startupfolder))
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(startupfolder);
                shortcut.TargetPath = DataHandler.Instance.AppExePath;
                shortcut.WorkingDirectory = DataHandler.Instance.AppDirectory;
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
                TimeHandler.Instance.Subscribe(StateChangerHandler.Instance.AddFeature());
                ExpandAutoDetectArea();
            }
            else
            {
                TimeHandler.Instance.UnSubscribe(StateChangerHandler.Instance.AddFeature());
                ContractAutoDetectArea();
            }
        }
        private void ExpandAutoDetectArea()
        {
            AutoToggleWorkingPanel.Height = 80;
            AutoDetectWork = true;
        }
        private void ContractAutoDetectArea()
        {
            AutoToggleWorkingPanel.Height = 0;
            AutoDetectWork = false;
        }

        private void ToggleDetectMouseIdle(object sender, RoutedEventArgs e)
        {
            if (AutoDetectIdleBtn.IsChecked == true)
            {
                ExpandDetectMouseIdleArea();
            }
            else
            {
                ContractDetectMouseIdleArea();
                TimeHandler.Instance.UnSubscribe(MouseIdleHandler.Instance.RemoveFeature());
            }
        }

        private void ChangeDetectMouseIdleDelay(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(AutoDetectIdleT.Text) || !int.TryParse(AutoDetectIdleT.Text, out _))
            {
                AutoDetectIdleT.Text = 1.ToString();
                AutoDetectIdleInterval = 1;
                return;
            }

            AutoDetectIdleInterval = int.Parse(AutoDetectIdleT.Text);
        }

        private void ExpandDetectMouseIdleArea()
        {
            AutoDetectIdlePanel.Height = 55;
            AutoDetectIdle = true;
        }
        private void ContractDetectMouseIdleArea()
        {
            AutoDetectIdlePanel.Height = 0;
            AutoDetectIdle = false;
        }
    }
}
