using CommunityToolkit.Mvvm.Input;
using System.Numerics;
using System.Threading.Tasks;

namespace WorkLifeBalance.ViewModels
{
    public partial class OptionsPageVM : SecondWindowPageVMBase
    {
        public OptionsPageVM()
        {
            RequiredWindowSize = new Vector2(250, 320);
            WindowPageName = "Options";
        }

        public override Task OnPageClosingAsync()
        {
            //SecondWindow.RequestSecondWindow(SecondWindowType.Settings);
            return Task.CompletedTask;
        }

        [RelayCommand]
        private void OpenSettings()
        {
            //SecondWindow.RequestSecondWindow(SecondWindowType.Settings);
        }
    }
}
