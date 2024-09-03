using System.Windows.Controls;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        int Saveinterval = 5;
        int AutoDetectInterval = 1;
        int AutoDetectIdleInterval = 1;

        bool AutoDetectWork = false;
        bool AutoDetectIdle = false;
        bool StartWithWin = false;
        private SettingsPageVM settingsPageVM;
        public SettingsPage(SettingsPageVM settingsPageVM)
        {
            InitializeComponent();
            this.settingsPageVM = settingsPageVM;
            DataContext = settingsPageVM;

            //ApplySettings();
        }

        ////updates ui based on loaded settings
        //private void ApplySettings()
        //{
        //    switch (DataStorageFeature.Instance.Settings.StartUpCornerC)
        //    {
        //        case AnchorCorner.TopLeft:
        //            TopLeftBtn.IsChecked = true;
        //            break;
        //        case AnchorCorner.TopRight:
        //            TopRightBtn.IsChecked = true;
        //            break;
        //        case AnchorCorner.BootomLeft:
        //            BottomLeftBtn.IsChecked = true;
        //            break;
        //        case AnchorCorner.BottomRight:
        //            BottomRightBtn.IsChecked = true;
        //            break;
        //    }

        //    VersionT.Text = $"Version: {DataStorageFeature.Instance.AppVersion}";

        //    AutosaveT.Text = DataStorageFeature.Instance.Settings.SaveInterval.ToString();

        //    AutoDetectT.Text = DataStorageFeature.Instance.Settings.AutoDetectInterval.ToString();

        //    AutoDetectIdleT.Text = DataStorageFeature.Instance.Settings.AutoDetectIdleInterval.ToString();

        //    StartWithWin = DataStorageFeature.Instance.Settings.StartWithWindowsC;
        //    StartWithWInBtn.IsChecked = StartWithWin;

        //    AutoDetectWork = DataStorageFeature.Instance.Settings.AutoDetectWorkingC;
        //    AutoDetectWorkingBtn.IsChecked = AutoDetectWork;

        //    AutoDetectIdle = DataStorageFeature.Instance.Settings.AutoDetectIdleC;
        //    AutoDetectIdleBtn.IsChecked = AutoDetectIdle;

        //    if (DataStorageFeature.Instance.Settings.AutoDetectWorkingC)
        //    {
        //        ExpandAutoDetectArea();
        //    }
        //    if (DataStorageFeature.Instance.Settings.AutoDetectIdleC)
        //    {
        //        ExpandDetectMouseIdleArea();
        //    }
        //}

        //private void ChangeAutosaveDelay(object sender, TextChangedEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(AutosaveT.Text) || !int.TryParse(AutosaveT.Text, out _))
        //    {
        //        AutosaveT.Text = 5.ToString();
        //        Saveinterval = 5;
        //    }
        //    else
        //    {
        //        Saveinterval = int.Parse(AutosaveT.Text);
        //    }
        //    Log.Information($"AutoSaveInterval set to {Saveinterval}");
        //}
        //private void ChangeAutoDetectDelay(object sender, TextChangedEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(AutoDetectT.Text) || !int.TryParse(AutoDetectT.Text, out _))
        //    {
        //        AutoDetectT.Text = 5.ToString();
        //        AutoDetectInterval = 5;
        //    }
        //    else
        //    {
        //        AutoDetectInterval = int.Parse(AutoDetectT.Text);
        //    }

        //    Log.Information($"AutoDetectStateInterval set to {AutoDetectInterval}");
        //}
        //private void SetBotLeftStartup(object sender, RoutedEventArgs e)
        //{
        //    BottomRightBtn.IsChecked = false;
        //    TopRightBtn.IsChecked = false;
        //    TopLeftBtn.IsChecked = false;
        //    DataStorageFeature.Instance.Settings.StartUpCornerC = AnchorCorner.BootomLeft;
        //    Log.Information($"StartupPosition set to {DataStorageFeature.Instance.Settings.StartUpCornerC}");
        //}

        //private void SetBotRightStartup(object sender, RoutedEventArgs e)
        //{
        //    BottomLeftBtn.IsChecked = false;
        //    TopRightBtn.IsChecked = false;
        //    TopLeftBtn.IsChecked = false;
        //    DataStorageFeature.Instance.Settings.StartUpCornerC = AnchorCorner.BottomRight;
        //    Log.Information($"StartupPosition set to {DataStorageFeature.Instance.Settings.StartUpCornerC}");
        //}

        //private void SetUpRightStartup(object sender, RoutedEventArgs e)
        //{
        //    BottomLeftBtn.IsChecked = false;
        //    BottomRightBtn.IsChecked = false;
        //    TopLeftBtn.IsChecked = false;
        //    DataStorageFeature.Instance.Settings.StartUpCornerC = AnchorCorner.TopRight;
        //    Log.Information($"StartupPosition set to {DataStorageFeature.Instance.Settings.StartUpCornerC}");
        //}

        //private void SetUpLeftStartup(object sender, RoutedEventArgs e)
        //{
        //    BottomLeftBtn.IsChecked = false;
        //    BottomRightBtn.IsChecked = false;
        //    TopRightBtn.IsChecked = false;
        //    DataStorageFeature.Instance.Settings.StartUpCornerC = AnchorCorner.TopLeft;
        //    Log.Information($"StartupPosition set to {DataStorageFeature.Instance.Settings.StartUpCornerC}");
        //}

        //private void SetStartWithWin(object sender, RoutedEventArgs e)
        //{
        //    StartWithWin = (bool)StartWithWInBtn.IsChecked;
        //    Log.Information($"StartWithWin set to {StartWithWin}");

        //}

        ////Each page has a close page so the second window can await the page specific cleanups stuff
        ////here we wait set the changed values,apply changes and wait for the save to db
        //public override async Task ClosePageAsync()
        //{
        //    DataStorageFeature.Instance.Settings.SaveInterval = Saveinterval;

        //    DataStorageFeature.Instance.Settings.AutoDetectInterval = AutoDetectInterval;

        //    DataStorageFeature.Instance.Settings.AutoDetectIdleInterval = AutoDetectIdleInterval;

        //    DataStorageFeature.Instance.Settings.AutoDetectIdleC = AutoDetectIdle;

        //    DataStorageFeature.Instance.Settings.AutoDetectWorkingC = AutoDetectWork;

        //    DataStorageFeature.Instance.Settings.StartWithWindowsC = StartWithWin;

        //    await DataStorageFeature.Instance.SaveData();

        //    ApplyStartToWindows();

        //    MainWindow.instance.ApplyAutoDetectWorking();
        //    Log.Information("Applied Settings");
        //}

        //private void ApplyStartToWindows()
        //{
        //    string startupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), $"{DataStorageFeature.Instance.AppName}.lnk");

        //    if (DataStorageFeature.Instance.Settings.StartWithWindowsC)
        //    {
        //        CreateShortcut(startupFolderPath);
        //    }
        //    else
        //    {
        //        DeleteShortcut(startupFolderPath);
        //    }
        //}

        //private void CreateShortcut(string startupfolder)
        //{
        //    if (!File.Exists(startupfolder))
        //    {
        //        WshShell shell = new WshShell();
        //        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(startupfolder);
        //        shortcut.TargetPath = DataStorageFeature.Instance.AppExePath;
        //        shortcut.WorkingDirectory = DataStorageFeature.Instance.AppDirectory;
        //        shortcut.Save();
        //    }
        //}

        //private void DeleteShortcut(string startupfolder)
        //{
        //    if (File.Exists(startupfolder))
        //    {
        //        File.Delete(startupfolder);
        //    }
        //}

        //private void ConfigureAutoDetectBtn(object sender, RoutedEventArgs e)
        //{
        //    SecondWindow.RequestSecondWindow(SecondWindowType.BackgroundProcesses);
        //}

        //private void ToggleAutoDetectWorking(object sender, RoutedEventArgs e)
        //{
        //    if (AutoDetectWorkingBtn.IsChecked == true)
        //    {
        //        ExpandAutoDetectArea();
        //    }
        //    else
        //    {
        //        ContractAutoDetectArea();
        //    }
        //    Log.Information($"AutoDetectWorking changed to {AutoDetectWorkingBtn.IsChecked}");
        //}
        //private void ExpandAutoDetectArea()
        //{
        //    AutoToggleWorkingPanel.Height = 80;
        //    AutoDetectWork = true;
        //}
        //private void ContractAutoDetectArea()
        //{
        //    AutoToggleWorkingPanel.Height = 0;
        //    AutoDetectWork = false;
        //}

        //private void ToggleDetectMouseIdle(object sender, RoutedEventArgs e)
        //{
        //    if (AutoDetectIdleBtn.IsChecked == true)
        //    {
        //        ExpandDetectMouseIdleArea();
        //    }
        //    else
        //    {
        //        ContractDetectMouseIdleArea();
        //        AppTimer.UnSubscribe(IdleCheckerFeature.Instance.RemoveFeature());
        //    }
        //    Log.Information($"AutoDetectIdle changed to {AutoDetectIdleBtn.IsChecked}");
        //}

        //private void ChangeDetectMouseIdleDelay(object sender, TextChangedEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(AutoDetectIdleT.Text) || !int.TryParse(AutoDetectIdleT.Text, out _))
        //    {
        //        AutoDetectIdleT.Text = 1.ToString();
        //        AutoDetectIdleInterval = 1;
        //        return;
        //    }

        //    AutoDetectIdleInterval = int.Parse(AutoDetectIdleT.Text);
        //    Log.Information($"AutoDetectIdleInterval set to {AutoDetectIdleInterval}");
        //}

        //private void ExpandDetectMouseIdleArea()
        //{
        //    AutoDetectIdlePanel.Height = 55;
        //    AutoDetectIdle = true;
        //}
        //private void ContractDetectMouseIdleArea()
        //{
        //    AutoDetectIdlePanel.Height = 0;
        //    AutoDetectIdle = false;
        //}
    }
}
