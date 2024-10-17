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
            SecondWindowPageVMBase activeModel = (SecondWindowPageVMBase)navigation.ActiveView;
            if(activeModel != null)
            {
                await activeModel.OnPageClosingAsync().ConfigureAwait(false);
            }
        }

        public async Task OpenWindowWith<T>(object? args = null) where T : SecondWindowPageVMBase 
        {
            _ = Task.Run(ClearPage);

            navigation.NavigateTo<T>();
            SecondWindowPageVMBase activeModel = (SecondWindowPageVMBase)navigation.ActiveView;
            //add a loading page first
            secondWindowVm.Width = (int)activeModel.RequiredWindowSize.X;
            secondWindowVm.Height = (int)activeModel.RequiredWindowSize.Y;
            secondWindowVm.PageName = activeModel.WindowPageName;
            secondWindowVm.ActivePage = activeModel;
            await activeModel.OnPageOppeningAsync(args);
            secondWindowView.Show();
        }
    }
}
