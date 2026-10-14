using System.Windows;
using System.Windows.Controls;

namespace WorkLifeBalance.Shared.Controls
{
    // The options as a grid of cells, the selected one in the accent color
    public partial class GridPicker : UserControl
    {
        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(
            nameof(Options), typeof(IEnumerable<PickerOption>), typeof(GridPicker));

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
            nameof(SelectedValue), typeof(object), typeof(GridPicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            nameof(Columns), typeof(int), typeof(GridPicker), new PropertyMetadata(4));

        public GridPicker()
        {
            InitializeComponent();
        }

        public event EventHandler? Picked;

        public IEnumerable<PickerOption>? Options
        {
            get => (IEnumerable<PickerOption>?)GetValue(OptionsProperty);
            set => SetValue(OptionsProperty, value);
        }

        public object? SelectedValue
        {
            get => GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }

        public int Columns
        {
            get => (int)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        private void OnOptionClick(object sender, RoutedEventArgs e)
        {
            SelectedValue = ((PickerOption)((FrameworkElement)sender).DataContext).Value;
            Picked?.Invoke(this, EventArgs.Empty);
        }
    }
}
