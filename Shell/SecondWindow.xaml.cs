using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using WorkLifeBalance.Shared.Navigation;

namespace WorkLifeBalance.Shell
{
    public partial class SecondWindow : Window
    {
        private static readonly Duration ResizeDuration = TimeSpan.FromMilliseconds(220);

        private readonly SecondWindowViewModel _viewModel;

        public SecondWindow(SecondWindowViewModel viewModel)
        {
            Topmost = true;
            DataContext = _viewModel = viewModel;
            viewModel.OnShowView += Show;
            viewModel.OnHideView += Hide;
            viewModel.Navigation.PropertyChanged += OnNavigationChanged;
            InitializeComponent();
        }

        // Resizes the card to the new page; the loading page keeps the size of the one before it
        private void OnNavigationChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SecondWindowService.LoadedPage) || _viewModel.Navigation.LoadedPage is not { } page || page is LoadingViewModel)
                return;

            if (!IsVisible)
            {
                Card.BeginAnimation(WidthProperty, null);
                Card.BeginAnimation(HeightProperty, null);
                Card.Width = page.PageWidth;
                Card.Height = page.PageHeight;
                return;
            }

            AnimateTo(WidthProperty, page.PageWidth);
            AnimateTo(HeightProperty, page.PageHeight);
        }

        private void AnimateTo(DependencyProperty property, double value) =>
            Card.BeginAnimation(property, new DoubleAnimation(value, ResizeDuration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        // The back button on the side of the mouse
        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.XButton1)
                return;

            _viewModel.GoBackCommand.Execute(null);
            e.Handled = true;
        }
    }
}
