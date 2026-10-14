namespace WorkLifeBalance.Shared.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly Func<Type, ViewModelBase> _viewModelFactory;

        public NavigationService(Func<Type, ViewModelBase> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public ViewModelBase NavigateTo<T>() where T : ViewModelBase => _viewModelFactory(typeof(T));

        public ViewModelBase NavigateTo(Type viewModelType) => _viewModelFactory(viewModelType);
    }
}
