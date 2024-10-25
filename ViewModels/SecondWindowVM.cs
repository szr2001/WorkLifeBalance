using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using WorkLifeBalance.Interfaces;

namespace WorkLifeBalance.ViewModels
{
    public partial class SecondWindowVM : ObservableObject
    {
        public ISecondWindowService SecondWindowService { get; set; }

        public Action? OnShowView { get; set; } = new(() => { });
        public Action? OnHideView { get; set; } = new(() => { });

        public SecondWindowVM(ISecondWindowService secondWindowService)
        {
            this.SecondWindowService = secondWindowService;
            SecondWindowService.OnPageLoaded += () => { OnShowView?.Invoke(); };
        }

        [RelayCommand]
        private void CloseSecondWindow()
        {
            SecondWindowService.CloseWindow();
            OnHideView?.Invoke();
        }
    }
}

