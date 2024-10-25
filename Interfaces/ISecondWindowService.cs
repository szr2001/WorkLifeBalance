using System;
using System.Threading.Tasks;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Interfaces
{
    public interface ISecondWindowService
    {
        public Action? OnPageLoaded { get; set; }
        public void CloseWindow();
        Task OpenWindowWith<T>(object? args = null) where T : SecondWindowPageVMBase;
    }
}