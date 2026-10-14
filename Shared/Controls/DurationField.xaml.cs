using System.Windows;
using System.Windows.Controls;

namespace WorkLifeBalance.Shared.Controls
{
    // Hours and minutes in one field ("1 h 30 min"); the popup has a wheel for each, like the iOS time picker
    public partial class DurationField : UserControl
    {
        public static readonly IReadOnlyList<PickerOption> HourOptions = PickerOption.Numbers(Enumerable.Range(0, 13));

        public static readonly IReadOnlyList<PickerOption> MinuteOptions =
            Enumerable.Range(0, 12).Select(i => new PickerOption(i * 5, (i * 5).ToString("00"))).ToList();

        public static readonly DependencyProperty HoursProperty = DependencyProperty.Register(
            nameof(Hours), typeof(int), typeof(DurationField), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public static readonly DependencyProperty MinutesProperty = DependencyProperty.Register(
            nameof(Minutes), typeof(int), typeof(DurationField), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public DurationField()
        {
            InitializeComponent();
            ShowValue();
        }

        public int Hours
        {
            get => (int)GetValue(HoursProperty);
            set => SetValue(HoursProperty, value);
        }

        public int Minutes
        {
            get => (int)GetValue(MinutesProperty);
            set => SetValue(MinutesProperty, value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DurationField)d).ShowValue();

        private void ShowValue()
        {
            if (Hours == 0)
                Label.Text = $"{Minutes} min";
            else if (Minutes == 0)
                Label.Text = $"{Hours} h";
            else
                Label.Text = $"{Hours} h {Minutes} min";
        }
    }
}
