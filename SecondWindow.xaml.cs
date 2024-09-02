using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WorkLifeBalance.Pages;
using WorkLifeBalance.ViewModels;
using WorkLifeBalance.Views;

namespace WorkLifeBalance
{
    /// <summary>
    /// Interaction logic for SecondWindow.xaml
    /// </summary>
    public partial class SecondWindow : Window
    {
        public static SecondWindow? Instance = null;

        public SecondWindowType WindowType;

        private SecondWindowPageBase WindowPage;
        private SecondWindowVM secondWindowVM;
        public SecondWindow(SecondWindowVM secondwindowVM)
        {
            if (Instance != null)
            {
                Instance.Close();
                Instance = this;
            }
            InitializeComponent();
            secondWindowVM = secondwindowVM;
        }

        public static void RequestSecondWindow(SecondWindowType Type, object? args = null)
        {
            _ = OpenWindow(Type, args);
        }

        public void CloseWindowButton(object sender, RoutedEventArgs e)
        {
            CloseBtn.IsEnabled = false;

            _ = CloseWindow();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Instance = null;
        }

        //create the specific requested page and args is used to box specific pages data
        //the unboxing of the args is made in every page that is expected to receive some data
        private void InitializeWindowType(object? args)
        {
            switch (WindowType)
            {
                case SecondWindowType.Settings:
                    WindowPage = new SettingsPage(args);
                    break;

                case SecondWindowType.ViewData:
                    WindowPage = new ViewDataPage(args);
                    break;

                case SecondWindowType.ViewDays:
                    WindowPage = new ViewDaysPage(args);
                    break;

                case SecondWindowType.BackgroundProcesses:
                    WindowPage = new BackgroundWindowsViewPage(args);
                    break;

                case SecondWindowType.ViewDayActivity:
                    WindowPage = new ViewDayDetailsPage(args);
                    break;

                case SecondWindowType.Options:
                    WindowPage = new OptionsPage(args);
                    break;
            }


            Width = WindowPage.RequiredWindowSize.X;
            Height = WindowPage.RequiredWindowSize.Y;
            WindowPageF.Content = WindowPage;
            WindowTitleT.Text = WindowPage.pageNme;
        }

        private async Task CloseWindow()
        {
            await WindowPage.ClosePageAsync();
            Instance = null;
            Close();
        }
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        //open a new window with the specified page, wait for the closing of any existing page
        private static async Task OpenWindow(SecondWindowType Type, object? args = null)
        {
            if (Instance != null && Instance.WindowType == Type)
            {
                await Instance.CloseWindow();
                return;
            }
            if (Instance != null)
            {
                await Instance.CloseWindow();
            }

            Instance = new SecondWindow();
            Instance.WindowType = Type;
            Instance.InitializeWindowType(args);
            Instance.Show();
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
