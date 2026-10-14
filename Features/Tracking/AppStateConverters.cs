using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using WorkLifeBalance.Shared.Icons;

namespace WorkLifeBalance.Features.Tracking
{
    // State.WorkingBrush, State.RestingBrush or State.IdleBrush from the design system
    public class AppStateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is AppState or string ? Application.Current.FindResource($"State.{value}Brush") : Brushes.Transparent;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

    public class AppStateToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ToState(value) switch
        {
            AppState.Working => PhosphorExtension.Get(PackIconPhosphorIconsKind.BriefcaseBold),
            AppState.Resting => PhosphorExtension.Get(PackIconPhosphorIconsKind.CoffeeBold),
            AppState.Idle => PhosphorExtension.Get(PackIconPhosphorIconsKind.MoonBold),
            _ => Geometry.Empty,
        };

        // The rows of the main window pass the state as text in the Tag
        private static AppState? ToState(object value) => value switch
        {
            AppState state => state,
            string text when Enum.TryParse<AppState>(text, out var state) => state,
            _ => null,
        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
