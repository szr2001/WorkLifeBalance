using System.Windows;
using System.Windows.Input;

namespace WorkLifeBalance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Topmost = true;
            SetStartUpLocation();
            InitializeComponent();
        }

        private void SetStartUpLocation()
        {
            int ScreenHeight = (int)SystemParameters.PrimaryScreenHeight;
            Left = 0;
            Top = ScreenHeight - 348;
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void HideWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
