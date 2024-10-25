using System.Windows;
using System.Windows.Input;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private readonly MainWindowVM ViewModel;

        public MainWindow(MainWindowVM viewModel)
        {
            Topmost = true;
            ViewModel = viewModel;
            DataContext = viewModel;
            SetStartUpLocation();
            InitializeComponent();
        }

        private void SetStartUpLocation()
        {
            int ScreenHeight = (int)SystemParameters.PrimaryScreenHeight;
            Left = 0;
            Top = ScreenHeight - 297;
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
