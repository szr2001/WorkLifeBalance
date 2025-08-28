using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Services;

public class PopupWindowService : WindowServiceBase<PopupWindowPageVMBase>,
    IWindowService<PopupWindowPageVMBase>
{
    public PopupWindowService(INavigationService navigationService) : base(navigationService)
    {
    }

    public override async Task OpenWith<TVm>(object? args = null)
    {
        await ClearPage();

        activePage = (PopupWindowPageVMBase)navigationService.NavigateTo<TVm>();

        await App.Current.Dispatcher.InvokeAsync(async () =>
        {
            await activePage.OnPageOpeningAsync(args);
            LoadedPage = activePage;
        });
    }

    protected virtual async Task ClearPage()
    {
        if (LoadedPage != null)
        {
            await LoadedPage.OnPageClosingAsync();
            LoadedPage = null;
        }
    }
}