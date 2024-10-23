using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.DirectoryServices.ActiveDirectory;
using System.Threading.Tasks;
using System.Windows;
using WorkLifeBalance.Interfaces;

namespace WorkLifeBalance.ViewModels
{
    public partial class SecondWindowVM : ObservableObject
    {
        //Maybe send the SecondWindowService and bind to it instead of having this field? Needs testing
        [ObservableProperty]
        private SecondWindowPageVMBase? activePage;

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

    public enum SecondWindowType
    {
        Settings,
        ViewData,
        ViewDays,
        BackgroundProcesses,
        ViewDayActivity,
        Options
    }
}

