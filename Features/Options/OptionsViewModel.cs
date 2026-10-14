using CommunityToolkit.Mvvm.Input;
using WorkLifeBalance.Features.ForceWork;
using WorkLifeBalance.Features.Settings;
using WorkLifeBalance.Features.WorkApps;
using WorkLifeBalance.Shared.Native;
using WorkLifeBalance.Shared.Navigation;

namespace WorkLifeBalance.Features.Options
{
    public partial class OptionsViewModel : SecondWindowPageBase
    {
        private const string DonationsLink = "https://buymeacoffee.com/RoberBot";
        private const string FeedbackLink = "https://docs.google.com/forms/d/e/1FAIpQLSfkPDHOLysWAPLZc9pdLFyRmiFxlVBN0xefXFcZ7XACOnnPhw/viewform?usp=sf_link";

        private readonly IWindowService<SecondWindowPageBase> _secondWindowService;
        private readonly LowLevelHandler _lowLevelHandler;

        public OptionsViewModel(IWindowService<SecondWindowPageBase> secondWindowService, LowLevelHandler lowLevelHandler)
        {
            _secondWindowService = secondWindowService;
            _lowLevelHandler = lowLevelHandler;
            PageWidth = 320;
            PageHeight = 380;
            PageName = "Options";
        }

        [RelayCommand]
        private void OpenSettings() => _secondWindowService.OpenWith<SettingsViewModel>();

        [RelayCommand]
        private void ConfigureAutoDetect() => _secondWindowService.OpenWith<WorkAppsViewModel>();

        [RelayCommand]
        private void OpenForceWork() => _secondWindowService.OpenWith<ForceWorkViewModel>();

        [RelayCommand]
        private void OpenDonations() => _lowLevelHandler.OpenLink(DonationsLink);

        [RelayCommand]
        private void OpenFeedback() => _lowLevelHandler.OpenLink(FeedbackLink);
    }
}
