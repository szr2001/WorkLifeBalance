using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Interfaces
{
    public interface INavigationService
    {
        ViewModelBase ActiveView { get; }
        
        void NavigateTo<T>() where T : ViewModelBase;
    }
}
