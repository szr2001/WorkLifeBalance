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
        private INavigationService navigationService;

        [ObservableProperty]
        private MainWindowDetailsPageBase? loadedPage;

        public MainWindowDetailsService(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public void CloseWindow()
        {
            LoadedPage = null;
        }

        public async Task OpenWindowWith<T>() where T : MainWindowDetailsPageBase
        {
            LoadedPage = (MainWindowDetailsPageBase)navigationService.NavigateTo<T>();

            await Task.CompletedTask;
        }
    }
}
