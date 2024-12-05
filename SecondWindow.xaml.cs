using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
        private readonly SecondWindowVM ViewModel;

        public SecondWindow(SecondWindowVM viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            ViewModel.OnShowView += Show;

            //maybe fixes the problem where the user clicks to view a page
            //but the second window is behind something or minimized
            //ViewModel.OnShowView += () =>
            //{
            //    set minimized to false idk what the code was
            //    Show();
            //    BringIntoView();
            //};
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
