using System;
using System.Threading.Tasks;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.Interfaces;

public interface IWindowService<in T> where T: PageViewModelBase
{
    Task Close();
    Task OpenWith<Tvm>(object? args = null) where Tvm : PageViewModelBase;
    Action? OnPageLoaded { get; set; }
}