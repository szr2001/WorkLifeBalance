using System.Threading.Tasks;

namespace WorkLifeBalance.ViewModels
{
    public class LoadingPageVM : SecondWindowPageVMBase
    {
        public LoadingPageVM()
        {
            PageName = "Loading...";
        }

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
