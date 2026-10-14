using CommunityToolkit.Mvvm.ComponentModel;

namespace WorkLifeBalance.Shared.Navigation
{
    public abstract partial class PopupWindowPageBase : PageViewModelBase
    {
        [ObservableProperty]
        private double _pageWidth = 260;

        [ObservableProperty]
        private double _pageHeight = 360;

        [ObservableProperty]
        private string _pageName = "Page";
    }
}
