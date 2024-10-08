using CommunityToolkit.Mvvm.ComponentModel;
using System;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Services
{
    public partial class NavigationService : ObservableObject, INavigationService
    {
        [ObservableProperty]
        public ViewModelBase? activeView;
        private readonly Func<Type, ViewModelBase> _viewModelFactory;

        public NavigationService(Func<Type, ViewModelBase> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public void NavigateTo<TViewModelbase>() where TViewModelbase : ViewModelBase
        {
            ViewModelBase viewModel = _viewModelFactory.Invoke(typeof(TViewModelbase)); 
            ActiveView = viewModel;
        }
    }
}
