using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.Services;

public abstract partial class WindowServiceBase<T> : ObservableObject where T : PageViewModelBase
{
    protected readonly INavigationService navigationService;
    public Action? OnPageLoaded { get; set; } = new(() => { });
    
    [ObservableProperty] 
    protected T? loadedPage;

    protected T? activePage;

    protected WindowServiceBase(INavigationService navigation)
    {
        this.navigationService = navigation;
    }

    partial void OnLoadedPageChanged(T? oldValue, T? newValue)
    {
        if (newValue != null)
        {
            OnPageLoaded?.Invoke();
        }
    }
    
    public abstract Task OpenWith<TVm>(object? args = null) where TVm : PageViewModelBase;

    public virtual async Task Close()
    {
        await ClearPage();
    }
    
    protected virtual async Task ClearPage()
    {
        if (activePage != null)
        {
            await activePage.OnPageClosingAsync();
            activePage = null;
        }
    }
}