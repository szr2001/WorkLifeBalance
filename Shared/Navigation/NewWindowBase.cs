using CommunityToolkit.Mvvm.ComponentModel;

namespace WorkLifeBalance.Shared.Navigation
{
    public abstract class NewWindowBase<TViewModel> : ObservableObject where TViewModel : PageViewModelBase
    {
        protected NewWindowBase(IWindowService<TViewModel> windowService)
        {
            WindowService = windowService;
        }

        public IWindowService<TViewModel> WindowService { get; }

        protected abstract Task CloseWindow();
    }
}
