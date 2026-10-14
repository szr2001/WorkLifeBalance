namespace WorkLifeBalance.Shared.Navigation
{
    public interface INavigationService
    {
        ViewModelBase NavigateTo<T>() where T : ViewModelBase;

        ViewModelBase NavigateTo(Type viewModelType);
    }
}
