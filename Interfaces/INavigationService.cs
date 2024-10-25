using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.Interfaces
{
    public interface INavigationService
    {
        ViewModelBase NavigateTo<T>() where T : ViewModelBase;
    }
}
