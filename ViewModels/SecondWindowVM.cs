using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using System.Windows;
using WorkLifeBalance.Interfaces;

namespace WorkLifeBalance.ViewModels
{
    public partial class SecondWindowVM : ObservableObject
    {
        [ObservableProperty]
        private INavigationService navigationService;

        public Action OnWindowRequested = new(()=> { });
        public Action OnWindowClose = new(() => { });

        public SecondWindowVM(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public void RequestSettings()
        {

        }

        //create the specific requested page and args is used to box specific pages data
        //the unboxing of the args is made in every page that is expected to receive some data
        private void InitializeWindowType(object? args)
        {
            //switch (WindowType)
            //{
            //    case SecondWindowType.Settings:
            //        WindowPage = new SettingsPage(args);
            //        break;

            //    case SecondWindowType.ViewData:
            //        WindowPage = new ViewDataPage(args);
            //        break;

            //    case SecondWindowType.ViewDays:
            //        WindowPage = new ViewDaysPage(args);
            //        break;

            //    case SecondWindowType.BackgroundProcesses:
            //        WindowPage = new BackgroundWindowsViewPage(args);
            //        break;

            //    case SecondWindowType.ViewDayActivity:
            //        WindowPage = new ViewDayDetailsPage(args);
            //        break;

            //    case SecondWindowType.Options:
            //        WindowPage = new OptionsPage(args);
            //        break;
            //}


            //Width = WindowPage.RequiredWindowSize.X;
            //Height = WindowPage.RequiredWindowSize.Y;
            //WindowPageF.Content = WindowPage;
            //WindowTitleT.Text = WindowPage.pageNme;
        }

        //open a new window with the specified page, wait for the closing of any existing page
        private static async Task OpenWindow(SecondWindowType Type, object? args = null)
        {
            //if (Instance != null && Instance.WindowType == Type)
            //{
            //    await Instance.CloseWindow();
            //    return;
            //}
            //if (Instance != null)
            //{
            //    await Instance.CloseWindow();
            //}

            ////Instance = new SecondWindow();
            //Instance.WindowType = Type;
            //Instance.InitializeWindowType(args);
            //Instance.Show();
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

