using CommunityToolkit.Mvvm.Input;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services;

namespace WorkLifeBalance.ViewModels
{
    public partial class OptionsPageVM : SecondWindowPageVMBase
    {
        private readonly ISecondWindowService secondWindowService;
        private readonly LowLevelHandler LowLevelHandler;
        public OptionsPageVM(ISecondWindowService secondWindowService, LowLevelHandler lowLevelHandler)
        {
            this.secondWindowService = secondWindowService;
            PageHeight = 320;
            PageWidth = 250;
            PageName = "Options";
            LowLevelHandler = lowLevelHandler;
        }

        [RelayCommand]
        private void OpenSettings()
        {
            secondWindowService.OpenWindowWith<SettingsPageVM>();
        }

        [RelayCommand]
        private void ConfigureAutoDetect()
        {
            secondWindowService.OpenWindowWith<BackgroundProcessesViewPageVM>();
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
            secondWindowService.OpenWindowWith<ForceWorkPageVM>();
        }
    }
}
