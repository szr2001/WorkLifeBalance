using System.Windows;

namespace WorkLifeBalance.Shared.Navigation
{
    public class PopupWindowService : WindowServiceBase<PopupWindowPageBase>, IWindowService<PopupWindowPageBase>
    {
        public PopupWindowService(INavigationService navigationService)
            : base(navigationService)
        {
        }

        public override async Task OpenWith<TViewModel>(object? args = null)
        {
            await ClearLoadedPage();

            var page = (PopupWindowPageBase)NavigationService.NavigateTo<TViewModel>();
            ActivePage = page;
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await page.OnPageOpeningAsync(args);
                LoadedPage = page;
            });
        }

        private async Task ClearLoadedPage()
        {
            if (LoadedPage != null)
            {
                await LoadedPage.OnPageClosingAsync();
                LoadedPage = null;
            }
        }
    }
}
