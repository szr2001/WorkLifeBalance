using System;
using System.Threading.Tasks;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.Interfaces
{
    public interface IMainWindowDetailsService
    {
        public void CloseWindow();
        Task OpenDetailsPageWith<T>(object? args = null) where T : MainWindowDetailsPageBase;
    }
}