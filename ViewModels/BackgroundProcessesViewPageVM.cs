using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using WorkLifeBalance.Helpers;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Models.Messages;
using WorkLifeBalance.Services;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.ViewModels
{
    public partial class BackgroundProcessesViewPageVM : SecondWindowPageVMBase, IRecipient<UrlsMessage>
    {
        public ObservableCollection<string> DetectedWindows { get; set; } = new();
        public ObservableCollection<string> SelectedProcesses { get; set; } = new();
        public ObservableCollection<string> SelectedPages { get; set; } = new();
        public ObservableCollection<string> DetectedTabs { get; set; } = new();

        [ObservableProperty]
        private string activeWindow = "";

        [ObservableProperty] 
        private string activePage = "";
        
        private DataStorageFeature dataStorageFeature;
        private LowLevelHandler lowLevelHandler;
        private ActivityTrackerFeature activityTrackerFeature;
        private IWindowService<SecondWindowPageVMBase> secondWindowService;
        private readonly IWindowService<PopupWindowPageVMBase> popupService;

        public BackgroundProcessesViewPageVM(DataStorageFeature dataStorageFeature, LowLevelHandler lowLevelHandler,
            ActivityTrackerFeature activityTrackerFeature, IWindowService<SecondWindowPageVMBase> secondWindowService,
            IWindowService<PopupWindowPageVMBase> popupService)
        {
            PageHeight = 960;
            PageWidth = 700;
            PageName = "Customize Work Apps";
            this.dataStorageFeature = dataStorageFeature;
            this.lowLevelHandler = lowLevelHandler;
            this.activityTrackerFeature = activityTrackerFeature;
            this.secondWindowService = secondWindowService;
            this.popupService = popupService;
        }

        private void UpdateActiveWindowUi(string newwindow)
        {
            ActiveWindow = newwindow;
        }

        private void UpdateActivePageUi(string page)
        {
            if (UrlHelper.TryGetHost(page, out string? host))
            {
                ActivePage = host!;
            }
        }
        
        private void InitializeProcessNames()
        {
            if (!WeakReferenceMessenger.Default.IsRegistered<UrlsMessage>(this))
            {
                WeakReferenceMessenger.Default.Register(this);
            }
            SelectedProcesses = new ObservableCollection<string>(dataStorageFeature.AutoChangeData.WorkingStateWindows);
            SelectedPages = new ObservableCollection<string>(dataStorageFeature.AutoChangeData.WorkingStateUrls);
            
            List<string> allProcesses = lowLevelHandler.GetBackgroundApplicationsName();
            List<string> allTabs = lowLevelHandler.GetActiveBackgroundTabs()
                .Select(x => UrlHelper.TryGetHost(x, out string? host) ? host! : null)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList()!;
            
            DetectedWindows = new ObservableCollection<string>(allProcesses.Except(SelectedProcesses));
            DetectedTabs = new ObservableCollection<string>(allTabs.Except(SelectedPages));
        }

        public override Task OnPageOpeningAsync(object? args = null)
        {
            activityTrackerFeature.OnWindowChange += UpdateActiveWindowUi;
            activityTrackerFeature.OnPageChange += UpdateActivePageUi;
            InitializeProcessNames();
            return Task.CompletedTask;
        }

        public override async Task OnPageClosingAsync()
        {
            activityTrackerFeature.OnWindowChange -= UpdateActiveWindowUi;
            activityTrackerFeature.OnPageChange -= UpdateActivePageUi;
            if(WeakReferenceMessenger.Default.IsRegistered<UrlsMessage>(this))
                WeakReferenceMessenger.Default.Unregister<UrlsMessage>(this);
            dataStorageFeature.AutoChangeData.WorkingStateWindows = SelectedProcesses.ToArray();
            dataStorageFeature.AutoChangeData.WorkingStateUrls = SelectedPages.ToArray();
            
            await popupService.Close();
            await dataStorageFeature.SaveData();
        }
        
        [RelayCommand]
        private void ReturnToPreviousPage()
        {
            secondWindowService.OpenWith<OptionsPageVM>();
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
        private void DeselectProcess(string itemName)
        {
            SelectedProcesses.Remove(itemName);
            DetectedWindows.Add(itemName); 
        }

        [RelayCommand]
        private void DeselectPage(string itemName)
        {
            SelectedPages.Remove(itemName);
            DetectedTabs.Add(itemName); 
        }
        
        [RelayCommand]
        private async Task OpenAddPageWindow()
        {
            IEnumerable<string> pages = SelectedPages.Concat(DetectedTabs);
            string urls = string.Join("|", pages);
            await popupService.OpenWith<AddUrlPageVM>(urls);
        }
        
        public void Receive(UrlsMessage message)
        {
            Task.Run(async () =>
            {
                string[] inputUrls = message.Value.Split('|');
                string[] validUrls = PrepareAndValidateInputUrls(inputUrls);
                HashSet<string> uniqueUrls = new HashSet<string>(validUrls.Union(DetectedTabs).Except(SelectedPages));

                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    DetectedTabs.Clear();
                    foreach (var url in uniqueUrls)
                    {
                        DetectedTabs.Add(url);
                    }
                });

            });
        }

        private static string[] PrepareAndValidateInputUrls(string[] inputUrls)
        {
            HashSet<string> result = new HashSet<string>();

            foreach (string url in inputUrls)
            {
                if (string.IsNullOrEmpty(url))
                {
                    continue;
                }

                if (UrlHelper.TryGetHost(url, out string? host))
                {
                    result.Add(host!);
                }
            }

            return result.ToArray();
        }
    }
}
