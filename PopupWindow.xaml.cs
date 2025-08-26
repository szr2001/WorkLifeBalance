using System.Windows;
using System.Windows.Input;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window
    {
        private readonly PopupWindowBaseVm ViewModel;

        public PopupWindow(PopupWindowBaseVm viewModel)
        {
            Topmost = true;
            ViewModel = viewModel;
            DataContext = ViewModel;
            ViewModel.OnShowView += () => Show();
            ViewModel.OnHideView += () => Hide();
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
