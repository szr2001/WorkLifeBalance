using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.Services
{
    public class MainWindowDetailsService : IMainWindowDetailsService
    {
        private MainWindowDetailsPageBase? activeMainWindowPage;
        private INavigationService navigationService;
        public MainWindowDetailsService(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public async Task OpenWindowWith<T>() where T : MainWindowDetailsPageBase
        {

        }
    }
}
