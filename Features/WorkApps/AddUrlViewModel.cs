using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WorkLifeBalance.Shared.Navigation;

namespace WorkLifeBalance.Features.WorkApps
{
    // Popup to type the working pages by hand, separated by '|'. Closing it sends them back to WorkAppsViewModel.
    public partial class AddUrlViewModel : PopupWindowPageBase
    {
        private readonly IWindowService<PopupWindowPageBase> _windowService;

        [ObservableProperty]
        private string _urls = string.Empty;

        public AddUrlViewModel(IWindowService<PopupWindowPageBase> windowService)
        {
            _windowService = windowService;
            PageHeight = 340;
            PageWidth = 330;
            PageName = "Enter \"working\" URLs";
        }

        public override Task OnPageOpeningAsync(object? args = null)
        {
            if (args is string urls)
                Urls = urls;

            return Task.CompletedTask;
        }

        public override Task OnPageClosingAsync()
        {
            WeakReferenceMessenger.Default.Send(new UrlsMessage(Urls));
            WeakReferenceMessenger.Default.Send(new PopupCloseMessage());
            return Task.CompletedTask;
        }

        public void Receive(UrlsMessage? message)
        {
            if (message != null)
                Urls = message.Value;
        }

        [RelayCommand]
        private async Task ClosePage() => await _windowService.Close();
    }
}
