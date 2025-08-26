using System.Threading.Tasks;

namespace WorkLifeBalance.ViewModels.Base
{
    public class MainWindowDetailsPageBase : PageViewModelBase
    {
        public override Task OnPageClosingAsync()
        {
            return Task.CompletedTask;
        }

        public override Task OnPageOpeningAsync(object? args = null)
        {
            return Task.CompletedTask;
        }
    }
}
