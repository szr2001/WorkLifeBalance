using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Services
{
    public class SecondWindowService : WindowServiceBase<SecondWindowPageVMBase>, IWindowService<SecondWindowPageVMBase>
    {
        private readonly DataStorageFeature dataStorageFeature;
        
        public SecondWindowService(INavigationService navigation, DataStorageFeature dataStorageFeature) : base(navigation)
        {
            this.dataStorageFeature = dataStorageFeature;
        }
        
        public override async Task OpenWith<TVm>(object? args = null)
        {
            if (dataStorageFeature.IsClosingApp) return;

            SecondWindowPageVMBase loading = (SecondWindowPageVMBase)navigationService.NavigateTo<LoadingPageVM>();
            
            if(activePage != null)
            {
                loading.PageWidth = activePage.PageWidth;
                loading.PageHeight= activePage.PageHeight;
            }

            LoadedPage = loading;

            await Task.Delay(150);

            await ClearPage();

            activePage = (SecondWindowPageVMBase)navigationService.NavigateTo<TVm>();

            await Task.Run(async () =>
            {
                await activePage.OnPageOpeningAsync(args);
                App.Current.Dispatcher.Invoke(() =>
                {
                    LoadedPage = activePage;
                });
            });
        }

        public override async Task Close()
        {
            if (dataStorageFeature.IsClosingApp) return;
            await ClearPage();
        }
    }
}
