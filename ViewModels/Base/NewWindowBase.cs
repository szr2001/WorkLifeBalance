using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using WorkLifeBalance.Interfaces;

namespace WorkLifeBalance.ViewModels.Base;

public abstract class NewWindowBase<TViewModel> : ObservableObject
    where TViewModel : PageViewModelBase
{
    // ReSharper disable once MemberCanBeProtected.Global
    public IWindowService<TViewModel> WindowService { get; set; }

    protected NewWindowBase(IWindowService<TViewModel> windowService)
    {
        this.WindowService = windowService;
    }

    protected abstract Task CloseWindow();
}