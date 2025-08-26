using System;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.Services
{
    public partial class NavigationService : INavigationService
    {

        private readonly Func<Type, ViewModelBase> _viewModelFactory;

        public NavigationService(Func<Type, ViewModelBase> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public ViewModelBase NavigateTo<TViewModelbase>() where TViewModelbase : ViewModelBase
        {
            return _viewModelFactory.Invoke(typeof(TViewModelbase)); 
        }
    }
}
