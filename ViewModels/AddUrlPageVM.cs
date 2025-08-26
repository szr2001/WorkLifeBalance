using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Models.Messages;

namespace WorkLifeBalance.ViewModels;

public partial class AddUrlPageVM: PopupWindowPageVMBase, IRecipient<UrlsMessage>
{
    private readonly IWindowService<PopupWindowPageVMBase> windowService;

    [ObservableProperty] 
    private string urls = string.Empty;

    public AddUrlPageVM(IWindowService<PopupWindowPageVMBase> windowService)
    {
        PageHeight = 320;
        PageWidth = 300;
        PageName = "Enter \"working\" URLs";
        this.windowService = windowService;
    }

    public override Task OnPageOpeningAsync(object? args = null)
    {
        WeakReferenceMessenger.Default.Register(this);

        return Task.CompletedTask;
    }

    public override Task OnPageClosingAsync()
    {
        WeakReferenceMessenger.Default.Send(new UrlsMessage(urls));
        WeakReferenceMessenger.Default.Unregister<UrlsMessage>(this);
        WeakReferenceMessenger.Default.Send(new PopupCloseMessage());
        return Task.CompletedTask;
    }

    public void Receive(UrlsMessage? message)
    {
       if(message != null)
       {
            Urls = message.Value;
       }
    }

    [RelayCommand]
    private async Task ClosePage()
    {
        await windowService.Close();
    }
}