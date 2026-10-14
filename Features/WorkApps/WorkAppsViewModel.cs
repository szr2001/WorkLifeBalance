using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WorkLifeBalance.Features.Tracking;
using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Native;
using WorkLifeBalance.Shared.Navigation;

namespace WorkLifeBalance.Features.WorkApps
{
    // Picks the processes and pages that count as "working"
    public partial class WorkAppsViewModel : SecondWindowPageBase, IRecipient<UrlsMessage>
    {
        private readonly DataStorageFeature _dataStorage;
        private readonly LowLevelHandler _lowLevelHandler;
        private readonly ActivityTrackerFeature _activityTracker;
        private readonly IWindowService<SecondWindowPageBase> _secondWindowService;
        private readonly IWindowService<PopupWindowPageBase> _popupService;

        [ObservableProperty]
        private string _activeWindow = "";

        [ObservableProperty]
        private string _activePage = "";

        public WorkAppsViewModel(DataStorageFeature dataStorage, LowLevelHandler lowLevelHandler, ActivityTrackerFeature activityTracker, IWindowService<SecondWindowPageBase> secondWindowService, IWindowService<PopupWindowPageBase> popupService)
        {
            _dataStorage = dataStorage;
            _lowLevelHandler = lowLevelHandler;
            _activityTracker = activityTracker;
            _secondWindowService = secondWindowService;
            _popupService = popupService;
            PageWidth = 700;
            PageHeight = 540;
            PageName = "Customize Work Apps";
        }

        public ObservableCollection<string> DetectedWindows { get; set; } = new();
        public ObservableCollection<string> SelectedProcesses { get; set; } = new();
        public ObservableCollection<string> SelectedPages { get; set; } = new();
        public ObservableCollection<string> DetectedTabs { get; set; } = new();

        public override Task OnPageOpeningAsync(object? args = null)
        {
            _activityTracker.OnWindowChange += UpdateActiveWindowUi;
            _activityTracker.OnPageChange += UpdateActivePageUi;
            InitializeProcessNames();
            return Task.CompletedTask;
        }

        public override async Task OnPageClosingAsync()
        {
            _activityTracker.OnWindowChange -= UpdateActiveWindowUi;
            _activityTracker.OnPageChange -= UpdateActivePageUi;
            if (WeakReferenceMessenger.Default.IsRegistered<UrlsMessage>(this))
                WeakReferenceMessenger.Default.Unregister<UrlsMessage>(this);

            _dataStorage.AutoChangeData.WorkingStateWindows = SelectedProcesses.ToArray();
            _dataStorage.AutoChangeData.WorkingStateUrls = SelectedPages.ToArray();
            await _popupService.Close();
            await _dataStorage.SaveData();
        }

        public void Receive(UrlsMessage message)
        {
            Task.Run(async () =>
            {
                var validUrls = PrepareAndValidateInputUrls(message.Value.Split('|'));
                var uniqueUrls = new HashSet<string>(validUrls.Union(DetectedTabs).Except(SelectedPages));
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    DetectedTabs.Clear();
                    foreach (var url in uniqueUrls)
                        DetectedTabs.Add(url);
                });
            });
        }

        private void UpdateActiveWindowUi(string window) => ActiveWindow = window;

        private void UpdateActivePageUi(string page)
        {
            if (UrlHelper.TryGetHost(page, out var host))
                ActivePage = host!;
        }

        private void InitializeProcessNames()
        {
            if (!WeakReferenceMessenger.Default.IsRegistered<UrlsMessage>(this))
                WeakReferenceMessenger.Default.Register(this);

            SelectedProcesses = new ObservableCollection<string>(_dataStorage.AutoChangeData.WorkingStateWindows);
            SelectedPages = new ObservableCollection<string>(_dataStorage.AutoChangeData.WorkingStateUrls);

            var allProcesses = _lowLevelHandler.GetBackgroundApplicationsName();
            var allTabs = _lowLevelHandler.GetActiveBackgroundTabs()
                .Select(url => UrlHelper.TryGetHost(url, out var host) ? host : null)
                .Where(host => !string.IsNullOrEmpty(host))
                .Select(host => host!);

            DetectedWindows = new ObservableCollection<string>(allProcesses.Except(SelectedProcesses));
            DetectedTabs = new ObservableCollection<string>(allTabs.Except(SelectedPages));
        }

        private static string[] PrepareAndValidateInputUrls(string[] inputUrls)
        {
            var hosts = new HashSet<string>();
            foreach (var url in inputUrls)
            {
                if (!string.IsNullOrEmpty(url) && UrlHelper.TryGetHost(url, out var host))
                    hosts.Add(host!);
            }

            return hosts.ToArray();
        }

        [RelayCommand]
        private void SelectProcess(string processName)
        {
            DetectedWindows.Remove(processName);
            SelectedProcesses.Add(processName);
        }

        [RelayCommand]
        private void SelectTab(string url)
        {
            DetectedTabs.Remove(url);
            SelectedPages.Add(url);
        }

        [RelayCommand]
        private void DeselectProcess(string processName)
        {
            SelectedProcesses.Remove(processName);
            DetectedWindows.Add(processName);
        }

        [RelayCommand]
        private void DeselectPage(string url)
        {
            SelectedPages.Remove(url);
            DetectedTabs.Add(url);
        }

        [RelayCommand]
        private async Task OpenAddPageWindow()
        {
            var urls = string.Join("|", SelectedPages.Concat(DetectedTabs));
            await _popupService.OpenWith<AddUrlViewModel>(urls);
        }
    }
}
