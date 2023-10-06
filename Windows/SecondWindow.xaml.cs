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
        public MainWindow MainWindowParent;

        private SecondWindowPageBase? WindowPage;
        public SecondWindow(MainWindow parent,SecondWindowType Windowtype,object? args)
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
            MainWindowParent = parent;
            InitializeComponent();
            WindowType = Windowtype;
            InitializeWindowType(args);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            MainWindowParent.SecondWindow = null;
        }

        private void InitializeWindowType(object? args)
        {
            switch (WindowType)
            {
                case SecondWindowType.Settings:
                    WindowPage = new SettingsPage(this, args);
                    break;

                case SecondWindowType.ViewData:
                    WindowPage = new ViewDataPage(this, args);
                    break;

                case SecondWindowType.ViewDays:
                    WindowPage = new ViewDaysPage(this, args);
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
        ViewDays
    }
}
