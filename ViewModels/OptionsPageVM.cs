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
            RequiredWindowSize = new Vector2(250, 320);
            WindowPageName = "Options";
        }

        public override Task OnPageClosingAsync()
        {
            secondWindowService.OpenWindowWith<SettingsPageVM>();
            return Task.CompletedTask;
        }

        [RelayCommand]
        private void OpenSettings()
        {
            secondWindowService.OpenWindowWith<SettingsPageVM>();
        }
    }
}
