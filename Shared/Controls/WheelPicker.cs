using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WorkLifeBalance.Shared.Controls
{
    // A column that scrolls like the iOS time picker: the selected value sits in the band in the middle and the
    // others fade out above and below. Mouse wheel, drag or click an item to choose.
    public class WheelPicker : UserControl
    {
        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(
            nameof(Options), typeof(IReadOnlyList<PickerOption>), typeof(WheelPicker), new PropertyMetadata(null, OnOptionsChanged));

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
            nameof(SelectedValue), typeof(object), typeof(WheelPicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

        private const double ItemHeight = 30;
        private const int VisibleItems = 5;
        private static readonly Duration ScrollDuration = TimeSpan.FromMilliseconds(180);

        private readonly StackPanel _items = new();
        private readonly TranslateTransform _offset = new();
        private int _index;
        private double? _dragStart;
        private double _dragStartOffset;
        private int _wheelDelta;

        public WheelPicker()
        {
            Width = 56;
            Height = ItemHeight * VisibleItems;
            Cursor = Cursors.Hand;
            _items.RenderTransform = _offset;

            var band = new Border
            {
                Height = ItemHeight,
                VerticalAlignment = VerticalAlignment.Center,
                CornerRadius = new CornerRadius(4),
            };
            band.SetResourceReference(Border.BackgroundProperty, "SecondaryRegionBrush");

            // The values fade out towards the top and the bottom. Absolute, because a relative mask follows the
            // bounds of the content, which move with the scroll.
            var fade = new LinearGradientBrush { MappingMode = BrushMappingMode.Absolute, StartPoint = new Point(0, 0), EndPoint = new Point(0, Height) };
            fade.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 0));
            fade.GradientStops.Add(new GradientStop(Color.FromArgb(255, 0, 0, 0), 0.4));
            fade.GradientStops.Add(new GradientStop(Color.FromArgb(255, 0, 0, 0), 0.6));
            fade.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 1));

            // A Canvas, so the column keeps its full height: inside a Grid it would be cut at the visible part
            _items.Width = Width;
            var items = new Canvas { OpacityMask = fade };
            items.Children.Add(_items);

            var root = new Grid { ClipToBounds = true, Background = Brushes.Transparent };
            root.Children.Add(band);
            root.Children.Add(items);
            Content = root;

            MouseWheel += OnMouseWheel;
            MouseLeftButtonDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseLeftButtonUp += OnMouseUp;
        }

        public IReadOnlyList<PickerOption>? Options
        {
            get => (IReadOnlyList<PickerOption>?)GetValue(OptionsProperty);
            set => SetValue(OptionsProperty, value);
        }

        public object? SelectedValue
        {
            get => GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }

        private int Count => Options?.Count ?? 0;

        private static void OnOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((WheelPicker)d).BuildItems();

        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (WheelPicker)d;
            var index = picker.IndexOf(e.NewValue);
            if (index >= 0 && index != picker._index)
                picker.ScrollTo(index, animate: picker.IsLoaded);
        }

        private void BuildItems()
        {
            _items.Children.Clear();
            foreach (var option in Options ?? [])
            {
                var text = new TextBlock
                {
                    Height = ItemHeight,
                    Text = option.Label,
                    TextAlignment = TextAlignment.Center,
                    Padding = new Thickness(0, 6, 0, 0),
                    FontSize = 15,
                };
                text.SetResourceReference(TextBlock.FontFamilyProperty, "FontFamily.Numbers");
                text.SetResourceReference(TextBlock.ForegroundProperty, "SecondaryTextBrush");
                _items.Children.Add(text);
            }

            ScrollTo(Math.Max(IndexOf(SelectedValue), 0), animate: false);
        }

        private int IndexOf(object? value)
        {
            for (var i = 0; i < Count; i++)
            {
                if (Equals(Options![i].Value, value))
                    return i;
            }

            return -1;
        }

        // Moves the column so the item sits in the middle band, and selects it
        private void ScrollTo(int index, bool animate)
        {
            if (Count == 0)
                return;

            _index = Math.Clamp(index, 0, Count - 1);
            var target = OffsetFor(_index);
            if (animate)
                _offset.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(target, ScrollDuration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
            else
            {
                _offset.BeginAnimation(TranslateTransform.YProperty, null);
                _offset.Y = target;
            }

            for (var i = 0; i < _items.Children.Count; i++)
            {
                var text = (TextBlock)_items.Children[i];
                text.SetResourceReference(TextBlock.ForegroundProperty, i == _index ? "PrimaryTextBrush" : "SecondaryTextBrush");
                text.FontWeight = i == _index ? FontWeights.SemiBold : FontWeights.Normal;
            }

            if (!Equals(SelectedValue, Options![_index].Value))
                SelectedValue = Options[_index].Value;
        }

        private static double OffsetFor(int index) => (VisibleItems / 2 - index) * ItemHeight;

        // A notch of the wheel is 120; touchpads send smaller steps, so they add up
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _wheelDelta += e.Delta;
            var steps = _wheelDelta / 120;
            if (steps != 0)
            {
                _wheelDelta -= steps * 120;
                ScrollTo(_index - steps, animate: true);
            }

            e.Handled = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(this).Y;
            _dragStartOffset = _offset.Y;
            _offset.BeginAnimation(TranslateTransform.YProperty, null);
            _offset.Y = _dragStartOffset;
            CaptureMouse();
            e.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_dragStart is not { } start)
                return;

            var min = OffsetFor(Count - 1) - ItemHeight / 2;
            var max = OffsetFor(0) + ItemHeight / 2;
            _offset.Y = Math.Clamp(_dragStartOffset + e.GetPosition(this).Y - start, min, max);
        }

        // A drag snaps to the nearest item; a click (no drag) picks the item under the mouse
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_dragStart is not { } start)
                return;

            ReleaseMouseCapture();
            _dragStart = null;

            var y = e.GetPosition(this).Y;
            if (Math.Abs(y - start) < 4)
            {
                var clicked = _index + (int)Math.Floor((y - Height / 2 + ItemHeight / 2) / ItemHeight);
                ScrollTo(clicked, animate: true);
                return;
            }

            ScrollTo((int)Math.Round(VisibleItems / 2 - _offset.Y / ItemHeight), animate: true);
        }
    }
}
