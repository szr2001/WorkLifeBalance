using System.Windows;
using System.Windows.Input;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance
{
    /// <summary>
    /// Interaction logic for SecondWindow.xaml
    /// </summary>
    /// //use it in dependency injection, make searate method for req windows
    public partial class SecondWindow : Window
    {
        private readonly SecondWindowBaseVM ViewModel;

        public SecondWindow(SecondWindowBaseVM viewModel)
        {
            Topmost = true;
            ViewModel = viewModel;
            DataContext = ViewModel;
            ViewModel.OnShowView += Show;
            ViewModel.OnHideView += Hide;
            InitializeComponent();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
