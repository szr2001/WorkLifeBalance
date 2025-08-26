using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Models.Messages;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.ViewModels;

public partial class PopupWindowBaseVm : NewWindowBase<PopupWindowPageVMBase>, IRecipient<PopupCloseMessage>
{
    public Action? OnShowView { get; set; } = () => { };
    public Action? OnHideView { get; set; } = () => { };

    public PopupWindowBaseVm(IWindowService<PopupWindowPageVMBase> windowService) : base(windowService)
    {
        windowService.OnPageLoaded += () =>
        {
            if (!WeakReferenceMessenger.Default.IsRegistered<PopupCloseMessage>(this))
            {
                WeakReferenceMessenger.Default.Register(this);
            }
            
            OnShowView?.Invoke();
        };
    }
    
    [RelayCommand]
    protected override async Task CloseWindow()
    {
        WeakReferenceMessenger.Default.Unregister<PopupCloseMessage>(this);
        await WindowService.Close();
        OnHideView?.Invoke();
    }

    public void Receive(PopupCloseMessage message)
    {
        CloseWindow();
    }
}