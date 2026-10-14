using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace WorkLifeBalance.Shell
{
    public partial class MainWindow : Window, IDisposable
    {
        private readonly MainViewModel _viewModel;
        private readonly NotifyIcon _notifyIcon;
        private GenieEffect? _genie;

        public MainWindow(MainViewModel viewModel)
        {
            Topmost = true;
            DataContext = _viewModel = viewModel;

            _notifyIcon = new NotifyIcon { Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location) };
            _notifyIcon.Click += OnNotifyIconClick;

            SetStartUpLocation();
            InitializeComponent();
            Loaded += (_, _) => _genie = new GenieEffect(Body, Root);
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
            GC.SuppressFinalize(this);
        }

        // bottom left corner of the main screen
        private void SetStartUpLocation()
        {
            Left = 0;
            Top = (int)SystemParameters.PrimaryScreenHeight - 297;
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void OnNotifyIconClick(object? sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            _notifyIcon.Visible = false;
        }

        private void OnCollapseToggleUnchecked(object sender, RoutedEventArgs e) => _genie?.Collapse(ToggleBounds());

        private void OnCollapseToggleChecked(object sender, RoutedEventArgs e) => _genie?.Expand(ToggleBounds());

        // A small square in the middle of the button, so the content goes behind the circle
        private Rect ToggleBounds()
        {
            var center = CollapseToggle.TranslatePoint(new Point(CollapseToggle.ActualWidth / 2, CollapseToggle.ActualHeight / 2), Root);
            var size = CollapseToggle.ActualWidth * 0.35;
            return new Rect(center.X - size / 2, center.Y - size / 2, size, size);
        }

        private void HideWindow(object sender, RoutedEventArgs e)
        {
            if (_viewModel.MinimizeToTray)
            {
                Hide();
                _notifyIcon.Visible = true;
            }
            else
            {
                WindowState = WindowState.Minimized;
            }
        }
    }
}
