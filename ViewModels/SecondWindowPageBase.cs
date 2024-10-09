using CommunityToolkit.Mvvm.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;

namespace WorkLifeBalance.ViewModels
{
    public abstract class SecondWindowPageVMBase : ViewModelBase
    {
        public Vector2 RequiredWindowSize = new Vector2(250, 300);
        public string WindowPageName = "Page";

        //pass object args
        public virtual Task OnPageClosingAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnPageOppeningAsync(object args) 
        {
            return Task.CompletedTask;
        }
    }
}
