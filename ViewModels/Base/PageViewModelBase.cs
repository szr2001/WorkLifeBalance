using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkLifeBalance.ViewModels.Base
{
    public class PageViewModelBase : ViewModelBase
    {
        public virtual Task OnPageClosingAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnPageOppeningAsync(object? args = null)
        {
            return Task.CompletedTask;
        }
    }
}
