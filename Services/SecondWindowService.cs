using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Services
{
    public partial class SecondWindowService : ObservableObject, ISecondWindowService
    {
        private readonly INavigationService navigation;

        [ObservableProperty]
        private SecondWindowPageVMBase? loadedPage;

        private SecondWindowPageVMBase? activeSecondWindowPage;

        public Action? OnPageLoaded { get; set; } = new(() => { });

        partial void OnLoadedPageChanged(SecondWindowPageVMBase? oldValue, SecondWindowPageVMBase? newValue)
        {
            OnPageLoaded?.Invoke();
        }

        public SecondWindowService(INavigationService navigation)
        {
            this.navigation = navigation;
        }

        public void CloseWindow()
        {
            _ = Task.Run(ClearPage);
        }

        private async Task ClearPage()
        {
            if(activeSecondWindowPage != null)
            {
                await activeSecondWindowPage.OnPageClosingAsync();
                activeSecondWindowPage = null;
            }
        }

        public async Task OpenWindowWith<T>(object? args = null) where T : SecondWindowPageVMBase 
        {
            SecondWindowPageVMBase loading = (SecondWindowPageVMBase)navigation.NavigateTo<LoadingPageVM>();
            
            if(activeSecondWindowPage != null)
            {
                loading.PageWidth = activeSecondWindowPage.PageWidth;
                loading.PageHeight= activeSecondWindowPage.PageHeight;
            }

            LoadedPage = loading;

            await Task.Delay(150);
            
            await Task.Run(ClearPage);

            activeSecondWindowPage = (SecondWindowPageVMBase)navigation.NavigateTo<T>();

            await Task.Run(async () =>
            {
                await activeSecondWindowPage.OnPageOppeningAsync(args);
                App.Current.Dispatcher.Invoke(() =>
                {
                    LoadedPage = activeSecondWindowPage;
                });
            });
        }
    }
}
