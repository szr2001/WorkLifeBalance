using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Controls;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.ViewModels
{
    public partial class BackgroundProcessesViewPageVM : SecondWindowPageVMBase
    {
        public ObservableCollection<string> DetectedWindows { get; set; } = new();
        public ObservableCollection<string> SelectedWindows { get; set; } = new();

        [ObservableProperty]
        private string activeWindow = "";

        private DataStorageFeature dataStorageFeature;
        private LowLevelHandler lowLevelHandler;
        private ActivityTrackerFeature activityTrackerFeature;
        private ISecondWindowService secondWindowService;
        public BackgroundProcessesViewPageVM(DataStorageFeature dataStorageFeature, LowLevelHandler lowLevelHandler, ActivityTrackerFeature activityTrackerFeature, ISecondWindowService secondWindowService)
        {
            PageHeight = 570;
            PageWidth = 700;
            PageName = "Customize Work Apps";
            this.dataStorageFeature = dataStorageFeature;
            this.lowLevelHandler = lowLevelHandler;
            this.activityTrackerFeature = activityTrackerFeature;
            this.secondWindowService = secondWindowService;
        }

        private void UpdateActiveWindowUi(string newwindow)
        {
            ActiveWindow = newwindow;
        }

        private void InitializeProcessNames()
        {
            SelectedWindows = new ObservableCollection<string>(dataStorageFeature.AutoChangeData.WorkingStateWindows);
            List<string> allProcesses = lowLevelHandler.GetBackgroundApplicationsName();
            DetectedWindows = new ObservableCollection<string>(allProcesses.Except(SelectedWindows));
        }

        public override Task OnPageOppeningAsync(object? args = null)
        {
            activityTrackerFeature.OnWindowChange += UpdateActiveWindowUi;
            InitializeProcessNames();
            return Task.CompletedTask;
        }

        public override async Task OnPageClosingAsync()
        {
            activityTrackerFeature.OnWindowChange -= UpdateActiveWindowUi;
            dataStorageFeature.AutoChangeData.WorkingStateWindows = SelectedWindows.ToArray();
            await dataStorageFeature.SaveData();
        }

        [RelayCommand]
        private void ReturnToPreviousPage()
        {
            secondWindowService.OpenWindowWith<OptionsPageVM>();
        }

        [RelayCommand]
        private void SelectProcess(string processName)
        {
            DetectedWindows.Remove(processName);
            SelectedWindows.Add(processName);
        }

        [RelayCommand]
        private void DeselectProcess(string processName)
        {
            SelectedWindows.Remove(processName);
            DetectedWindows.Add(processName);
        }
    }
}
