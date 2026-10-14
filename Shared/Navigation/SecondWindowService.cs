using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using WorkLifeBalance.Shared.Data;

namespace WorkLifeBalance.Shared.Navigation
{
    // Keeps the pages opened in the second window so the user can go back (Options -> Settings -> back to Options)
    public partial class SecondWindowService : WindowServiceBase<SecondWindowPageBase>, IWindowService<SecondWindowPageBase>
    {
        private readonly DataStorageFeature _dataStorage;
        private readonly Stack<(Type Page, object? Args)> _history = new();
        private (Type Page, object? Args)? _current;

        [ObservableProperty]
        private bool _canGoBack;

        public SecondWindowService(INavigationService navigationService, DataStorageFeature dataStorage)
            : base(navigationService)
        {
            _dataStorage = dataStorage;
        }

        public override async Task OpenWith<TViewModel>(object? args = null)
        {
            if (_dataStorage.IsClosingApp)
                return;

            if (_current is { } current && current.Page != typeof(TViewModel))
                _history.Push(current);

            await Show(typeof(TViewModel), args);
        }

        public async Task GoBack()
        {
            if (_dataStorage.IsClosingApp || !_history.TryPop(out var previous))
                return;

            await Show(previous.Page, previous.Args);
        }

        public override async Task Close()
        {
            if (_dataStorage.IsClosingApp)
                return;

            _history.Clear();
            _current = null;
            CanGoBack = false;
            await ClearPage();
        }

        private async Task Show(Type pageType, object? args)
        {
            _current = (pageType, args);
            CanGoBack = _history.Count > 0;

            LoadedPage = (SecondWindowPageBase)NavigationService.NavigateTo(typeof(LoadingViewModel));
            await Task.Delay(150);
            await ClearPage();

            var page = (SecondWindowPageBase)NavigationService.NavigateTo(pageType);
            ActivePage = page;
            await Task.Run(async () =>
            {
                await page.OnPageOpeningAsync(args);
                Application.Current.Dispatcher.Invoke(() => LoadedPage = page);
            });
        }
    }
}
