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

        await Task.Run(async () =>
        {
            await activePage.OnPageOpeningAsync(args);
            App.Current.Dispatcher.Invoke(() => { LoadedPage = activePage; });
        });
    }

    public override async Task Close()
    {
        await ClearPage();
    }
}