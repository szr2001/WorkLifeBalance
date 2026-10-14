using CommunityToolkit.Mvvm.ComponentModel;

namespace WorkLifeBalance.Shared.Navigation
{
    public abstract partial class WindowServiceBase<T> : ObservableObject where T : PageViewModelBase
    {
        protected readonly INavigationService NavigationService;

        [ObservableProperty]
        private T? _loadedPage;

        protected WindowServiceBase(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        public Action? OnPageLoaded { get; set; } = () => { };

        protected T? ActivePage { get; set; }

        public abstract Task OpenWith<TViewModel>(object? args = null) where TViewModel : PageViewModelBase;

        public virtual async Task Close() => await ClearPage();

        protected virtual async Task ClearPage()
        {
            if (ActivePage != null)
            {
                await ActivePage.OnPageClosingAsync();
                ActivePage = null;
            }
        }

        partial void OnLoadedPageChanged(T? value)
        {
            if (value != null)
                OnPageLoaded?.Invoke();
        }
    }
}
