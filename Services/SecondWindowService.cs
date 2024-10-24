using System;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Services
{
    public class SecondWindowService : ISecondWindowService
    {
        private readonly SecondWindowVM secondWindowVm;
        private readonly SecondWindow secondWindowView;
        private readonly INavigationService navigation;

        private SecondWindowPageVMBase? activeSecondWindowPage;

        public SecondWindowService(SecondWindowVM secondWindowVm, SecondWindow secondWindowView, INavigationService navigation)
        {
            this.secondWindowVm = secondWindowVm;
            this.secondWindowView = secondWindowView;
            this.navigation = navigation;
            secondWindowVm.OnWindowClosing += CloseWindow;
        }

        private void CloseWindow()
        {
            _ = Task.Run(ClearPage);
            secondWindowView.Hide();
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
            _ = Task.Run(ClearPage);

            SecondWindowPageVMBase loading = (SecondWindowPageVMBase)navigation.NavigateTo<LoadingPageVM>();

            secondWindowVm.ActivePage = loading;
            secondWindowView.Show();

            activeSecondWindowPage = (SecondWindowPageVMBase)navigation.NavigateTo<T>();
            secondWindowVm.Width = activeSecondWindowPage.PageWidth;
            secondWindowVm.Height = activeSecondWindowPage.PageHeight;
            secondWindowVm.PageName = activeSecondWindowPage.PageName;

            await Task.Delay(300);

            _ = Task.Run(async () =>
            {
                await activeSecondWindowPage.OnPageOppeningAsync(args);

                App.Current.Dispatcher.Invoke(() =>
                {
                    secondWindowVm.ActivePage = activeSecondWindowPage;
                });
            });
        }
    }
}
