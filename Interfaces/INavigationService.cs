using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Interfaces
{
    public interface INavigationService
    {
        ViewModelBase NavigateTo<T>() where T : ViewModelBase;
    }
}
