using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using WorkLifeBalance.Interfaces;

namespace WorkLifeBalance.ViewModels
{
    public partial class SecondWindowVM : ObservableObject
    {
        public ISecondWindowService SecondWindowService { get; set; }

        public SecondWindowVM(ISecondWindowService secondWindowService)
        {
            this.SecondWindowService = secondWindowService;
        }

        [ObservableProperty]
        private string pageName = "Page";

        [ObservableProperty]
        private int height = 300;

        [ObservableProperty]
        private int width = 250;

        public Action OnWindowClosing { get; set; } = new(() => { });

        [RelayCommand]
        private void CloseSecondWindow()
        {
            OnWindowClosing.Invoke();
        }
    }
}

