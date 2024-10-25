using System;
using System.Threading.Tasks;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.Interfaces
{
    public interface IMainWindowDetailsService
    {
        public void CloseWindow();
        Task OpenWindowWith<T>() where T : MainWindowDetailsPageBase;
    }
}