using System.Windows;
using System.Windows.Controls;

namespace WorkLifeBalance.Shared.Controls
{
    // A field that shows the chosen option and opens the options as a grid; picking one closes it
    public partial class PickerField : UserControl
    {
        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(
            nameof(Options), typeof(IEnumerable<PickerOption>), typeof(PickerField), new PropertyMetadata(null, OnValueChanged));

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
            nameof(SelectedValue), typeof(object), typeof(PickerField), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            nameof(Columns), typeof(int), typeof(PickerField), new PropertyMetadata(4));

        public PickerField()
        {
            InitializeComponent();
        }

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

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var field = (PickerField)d;
            var option = field.Options?.FirstOrDefault(option => Equals(option.Value, field.SelectedValue));
            field.Label.Text = option?.Label ?? field.SelectedValue?.ToString();
        }

        private void OnPicked(object? sender, EventArgs e) => Toggle.IsChecked = false;
    }
}
