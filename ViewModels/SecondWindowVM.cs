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

        [ObservableProperty]
        public SecondWindowPageVMBase? activePage;

        [ObservableProperty]
        public string pageName = "Page";

        [ObservableProperty]
        public int height = 300;

        [ObservableProperty]
        public int width = 250;

        public Action OnWindowClosing = new(() => { });

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

