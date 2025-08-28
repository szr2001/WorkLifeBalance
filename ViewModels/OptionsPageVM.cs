using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services;

namespace WorkLifeBalance.ViewModels
{
    public partial class OptionsPageVM : SecondWindowPageVMBase
    {
        private readonly IWindowService<SecondWindowPageVMBase> secondWindowService;
        private readonly LowLevelHandler LowLevelHandler;
        public OptionsPageVM(IWindowService<SecondWindowPageVMBase> secondWindowService, LowLevelHandler lowLevelHandler)
        {
            this.secondWindowService = secondWindowService;
            PageHeight = 320;
            PageWidth = 250;
            PageName = "Options";
            LowLevelHandler = lowLevelHandler;
        }
        public override Task OnPageClosingAsync() => Task.CompletedTask;

        public override Task OnPageOpeningAsync(object? args = null) => Task.CompletedTask;

        [RelayCommand]
        private void OpenSettings()
        {
            secondWindowService.OpenWith<SettingsPageVM>();
        }

        [RelayCommand]
        private void ConfigureAutoDetect()
        {
            secondWindowService.OpenWith<BackgroundProcessesViewPageVM>();
        }

        [RelayCommand]
        private void OpenDonations()
        {
            LowLevelHandler.OpenLink("https://buymeacoffee.com/RoberBot");
        }

        [RelayCommand]
        private void OpenFeedback()
        {

            LowLevelHandler.OpenLink(@"https://docs.google.com/forms/d/e/1FAIpQLSfkPDHOLysWAPLZc9pdLFyRmiFxlVBN0xefXFcZ7XACOnnPhw/viewform?usp=sf_link");
        }

        [RelayCommand]
        private void OpenForceWork()
        {
            secondWindowService.OpenWith<ForceWorkPageVM>();
        }
    }
}
