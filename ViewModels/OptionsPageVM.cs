using System.Numerics;
using System.Threading.Tasks;

namespace WorkLifeBalance.ViewModels
{
    public class OptionsPageVM : SecondWindowPageVMBase
    {
        public OptionsPageVM()
        {
            RequiredWindowSize = new Vector2(250, 320);
            WindowPageName = "Options";
        }

        public override Task ClosePageAsync()
        {
            return Task.CompletedTask;
        }
    }
}
