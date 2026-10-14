using CommunityToolkit.Mvvm.Input;
using WorkLifeBalance.Shared.Navigation;

namespace WorkLifeBalance.Shell
{
    public partial class SecondWindowViewModel : NewWindowBase<SecondWindowPageBase>
    {
        public SecondWindowViewModel(SecondWindowService windowService)
            : base(windowService)
        {
            Navigation = windowService;
            windowService.OnPageLoaded += () => OnShowView?.Invoke();
        }

        public SecondWindowService Navigation { get; }

        public Action? OnShowView { get; set; } = () => { };
        public Action? OnHideView { get; set; } = () => { };

        [RelayCommand]
        private async Task GoBack() => await Navigation.GoBack();

        [RelayCommand]
        protected override async Task CloseWindow()
        {
            await WindowService.Close();
            OnHideView?.Invoke();
        }
    }
}
