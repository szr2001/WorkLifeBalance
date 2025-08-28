using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.ViewModels;

public partial class SecondWindowBaseVM : NewWindowBase<SecondWindowPageVMBase>
{
    public Action? OnShowView { get; set; } = () => { };
    public Action? OnHideView { get; set; } = () => { };

    public SecondWindowBaseVM(IWindowService<SecondWindowPageVMBase> windowService) : base(windowService)
    {
        windowService.OnPageLoaded += () => { OnShowView?.Invoke(); };
    }

    [RelayCommand]
    protected override async Task CloseWindow()
    {
        await WindowService.Close();
        OnHideView?.Invoke();
    }
}