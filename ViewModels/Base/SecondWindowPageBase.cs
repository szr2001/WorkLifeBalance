using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.ViewModels
{
    public abstract partial class SecondWindowPageVMBase : ViewModelBase
    {
        [ObservableProperty]
        private int pageWidth = 250;
        
        [ObservableProperty]
        private int pageHeight = 300;
        
        [ObservableProperty]
        private string pageName = "Page";

        public virtual Task OnPageClosingAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnPageOppeningAsync(object? args = null) 
        {
            return Task.CompletedTask;
        }
    }
}
