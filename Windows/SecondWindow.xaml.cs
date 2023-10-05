using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
        public SecondWindow(MainWindow parent,SecondWindowType Windowtype)
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
            InitializeWindowType();
        }
        private void InitializeWindowType()
        {
            switch (WindowType)
            {
                case SecondWindowType.Settings:
                    WindowPage = new SettingsPage(this);
                    break;

                case SecondWindowType.ViewData:
                    WindowPage = new ViewDataPage(this);
                    break;

                case SecondWindowType.ViewDays:
                    WindowPage = new ViewDaysPage(this);
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
