using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.Services
{
    public partial class MainWindowDetailsService : ObservableObject, IWindowService<MainWindowDetailsPageBase>
    {
        //needs a way to stop multiple features requesting a MainWindowDetails page.
        private INavigationService navigationService;

        [ObservableProperty]
        private MainWindowDetailsPageBase? loadedPage;

        private MainWindowDetailsPageBase? activeMainWindowPage;
        
        public Action? OnPageLoaded { get; set; }
    
        partial void OnLoadedPageChanged(MainWindowDetailsPageBase? oldValue, MainWindowDetailsPageBase? newValue)
        {
            OnPageLoaded?.Invoke();
        }
        
        public MainWindowDetailsService(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public async Task Close()
        {
            await ClearPage();
        }

        public async Task OpenWith<TVm>(object? args) where TVm : PageViewModelBase
        {
            await Task.Run(ClearPage);

            activeMainWindowPage = (MainWindowDetailsPageBase)navigationService.NavigateTo<TVm>();

            await Task.Run(async () =>
            {
                await activeMainWindowPage.OnPageOpeningAsync(args);
                App.Current.Dispatcher.Invoke(() =>
                {
                    LoadedPage = activeMainWindowPage;
                });
            });
        }

        private async Task ClearPage()
        {
            if (LoadedPage != null)
            {
                await LoadedPage.OnPageClosingAsync();
                LoadedPage = null;
            }
        }
    }
}
