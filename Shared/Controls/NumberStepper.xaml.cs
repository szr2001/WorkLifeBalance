using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WorkLifeBalance.Shared.Controls
{
    // - value +, for small counts and intervals. Holding a button repeats it and the mouse wheel works too.
    public partial class NumberStepper : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(int), typeof(NumberStepper), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnChanged));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(int), typeof(NumberStepper), new PropertyMetadata(0, OnChanged));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(int), typeof(NumberStepper), new PropertyMetadata(100, OnChanged));

        public static readonly DependencyProperty SuffixProperty = DependencyProperty.Register(
            nameof(Suffix), typeof(string), typeof(NumberStepper), new PropertyMetadata("", OnChanged));

        public NumberStepper()
        {
            InitializeComponent();
            ShowValue();
        }

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        // Shown after the number ("5 min")
        public string Suffix
        {
            get => (string)GetValue(SuffixProperty);
            set => SetValue(SuffixProperty, value);
        }

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((NumberStepper)d).ShowValue();

        private void ShowValue()
        {
            Label.Text = string.IsNullOrEmpty(Suffix) ? Value.ToString() : $"{Value} {Suffix}";
            Decrease.IsEnabled = Value > Minimum;
            Increase.IsEnabled = Value < Maximum;
        }

        private void Step(int amount) => Value = Math.Clamp(Value + amount, Minimum, Maximum);

        private void OnDecrease(object sender, RoutedEventArgs e) => Step(-1);

        private void OnIncrease(object sender, RoutedEventArgs e) => Step(1);

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Step(e.Delta > 0 ? 1 : -1);
            e.Handled = true;
        }
    }
}
