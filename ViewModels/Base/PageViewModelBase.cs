using System.Threading.Tasks;

namespace WorkLifeBalance.ViewModels.Base
{
    public abstract class PageViewModelBase : ViewModelBase
    {
        public abstract Task OnPageClosingAsync();
        
        public abstract Task OnPageOpeningAsync(object? args = null);
    }
}
