using CommunityToolkit.Mvvm.ComponentModel;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.ViewModels
{
    public abstract partial class SecondWindowPageVMBase : PageViewModelBase
    {
        [ObservableProperty]
        private double pageWidth = 250;
        
        [ObservableProperty]
        private double pageHeight = 300;
        
        [ObservableProperty]
        private string pageName = "Page";
    }
}
