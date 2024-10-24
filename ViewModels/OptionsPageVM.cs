using CommunityToolkit.Mvvm.Input;
using System.Numerics;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;

namespace WorkLifeBalance.ViewModels
{
    public partial class OptionsPageVM : SecondWindowPageVMBase
    {
        private ISecondWindowService secondWindowService;
        public OptionsPageVM(ISecondWindowService secondWindowService)
        {
            this.secondWindowService = secondWindowService;
            PageHeight = 320;
            PageWidth = 250;
            PageName = "Options";
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
    }
}
