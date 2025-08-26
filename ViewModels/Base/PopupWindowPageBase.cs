using CommunityToolkit.Mvvm.ComponentModel;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.ViewModels;

public abstract partial class PopupWindowPageVMBase : PageViewModelBase
{
    [ObservableProperty]
    private double pageWidth = 260;
        
    [ObservableProperty]
    private double pageHeight = 360;
        
    [ObservableProperty]
    private string pageName = "Page";
}