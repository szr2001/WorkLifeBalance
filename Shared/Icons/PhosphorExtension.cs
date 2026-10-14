using System.Collections.Concurrent;
using System.Windows.Markup;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace WorkLifeBalance.Shared.Icons
{
    // A Phosphor icon as a Geometry, to use straight in XAML: <Path Data="{icons:Phosphor GearSixBold}" />
    [MarkupExtensionReturnType(typeof(Geometry))]
    public sealed class PhosphorExtension : MarkupExtension
    {
        private static readonly ConcurrentDictionary<PackIconPhosphorIconsKind, Geometry> Cache = new();

        public PhosphorExtension()
        {
        }

        public PhosphorExtension(PackIconPhosphorIconsKind kind)
        {
            Kind = kind;
        }

        [ConstructorArgument("kind")]
        public PackIconPhosphorIconsKind Kind { get; set; }

        public static Geometry Get(PackIconPhosphorIconsKind kind) => Cache.GetOrAdd(kind, key =>
        {
            var data = new PackIconPhosphorIcons { Kind = key }.Data;
            if (string.IsNullOrEmpty(data))
                return Geometry.Empty;

            var geometry = Geometry.Parse(data);
            geometry.Freeze();
            return geometry;
        });

        public override object ProvideValue(IServiceProvider serviceProvider) => Get(Kind);
    }
}
