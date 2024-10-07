using CommunityToolkit.Mvvm.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;

namespace WorkLifeBalance.ViewModels
{
    public abstract class SecondWindowPageVMBase : ObservableObject
    {
        public Vector2 RequiredWindowSize = new Vector2(250, 300);
        public string WindowPageName = "Page";

        public virtual Task ClosePageAsync()
        {
            return Task.CompletedTask;
        }
    }
}
