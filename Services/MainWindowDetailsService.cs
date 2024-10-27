using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.Services
{
    public partial class MainWindowDetailsService : ObservableObject, IMainWindowDetailsService
    {
        //needs a way to stop multiple features requesting a MainWindowDetails page.
        private INavigationService navigationService;

        [ObservableProperty]
        private MainWindowDetailsPageBase? loadedPage;

        private MainWindowDetailsPageBase? activeMainWindowPage;

        public MainWindowDetailsService(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public void CloseWindow()
        {
            _ = Task.Run(ClearPage);
        }

        private async Task ClearPage()
        {
            if (LoadedPage != null)
            {
                await LoadedPage.OnPageClosingAsync();
                LoadedPage = null;
            }
        }

        public async Task OpenDetailsPageWith<T>(object? args) where T : MainWindowDetailsPageBase
        {
            await Task.Run(ClearPage);

            activeMainWindowPage = (MainWindowDetailsPageBase)navigationService.NavigateTo<T>();

            await Task.Run(async () =>
            {
                await activeMainWindowPage.OnPageOppeningAsync(args);
                App.Current.Dispatcher.Invoke(() =>
                {
                    LoadedPage = activeMainWindowPage;
                });
            });
        }
    }
}
