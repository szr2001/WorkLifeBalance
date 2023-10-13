using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WorkLifeBalance.Pages;

namespace WorkLifeBalance.Windows
{
    /// <summary>
    /// Interaction logic for SecondWindow.xaml
    /// </summary>
    public partial class SecondWindow : Window
    {
        public static SecondWindow? Instance = null;

        public SecondWindowType WindowType;

        private SecondWindowPageBase? WindowPage;
        public SecondWindow(SecondWindowType Windowtype,object? args)
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Instance.Close();
                Instance = this;
            }
            InitializeComponent();
            WindowType = Windowtype;
            InitializeWindowType(args);
        }
        public static void OpenSecondWindow(SecondWindowType Page, object? args = null)
        {
            if (Instance != null && Instance.WindowType == Page)
            {
                Instance.Close();
                Instance = null;
                return;
            }
            Instance = new SecondWindow(Page, args);
            Instance.Show();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Instance = null;
        }

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
            }

            
            Width = WindowPage.RequiredWindowSize.X;
            Height = WindowPage.RequiredWindowSize.Y;
            WindowPageF.Content = WindowPage;
            WindowTitleT.Text = WindowPage.pageNme;
        }

        public void CloseWindowButton(object sender, RoutedEventArgs e)
        {
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
    }

    public enum SecondWindowType
    {
        Settings,
        ViewData,
        ViewDays,
        BackgroundProcesses
    }
}
