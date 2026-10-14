using CommunityToolkit.Mvvm.ComponentModel;

namespace WorkLifeBalance.Shared.Navigation
{
    // The second window animates to the size of the page
    public abstract partial class SecondWindowPageBase : PageViewModelBase
    {
        [ObservableProperty]
        private double _pageWidth = 320;

        [ObservableProperty]
        private double _pageHeight = 320;

        [ObservableProperty]
        private string _pageName = "Page";
    }
}
