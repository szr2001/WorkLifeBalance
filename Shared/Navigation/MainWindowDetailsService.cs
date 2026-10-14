using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WorkLifeBalance.Shared.Navigation
{
    public partial class MainWindowDetailsService : ObservableObject, IWindowService<MainWindowDetailsPageBase>
    {
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private MainWindowDetailsPageBase? _loadedPage;

        public MainWindowDetailsService(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public Action? OnPageLoaded { get; set; }

        public async Task Close() => await ClearPage();

        public async Task OpenWith<TViewModel>(object? args = null) where TViewModel : PageViewModelBase
        {
            await Task.Run(ClearPage);

            var page = (MainWindowDetailsPageBase)_navigationService.NavigateTo<TViewModel>();
            await Task.Run(async () =>
            {
                await page.OnPageOpeningAsync(args);
                Application.Current.Dispatcher.Invoke(() => LoadedPage = page);
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

        partial void OnLoadedPageChanged(MainWindowDetailsPageBase? value) => OnPageLoaded?.Invoke();
    }
}
