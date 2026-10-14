using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace WorkLifeBalance.Shell
{
    // The macOS "genie" minimize: a snapshot of the content is drawn on a mesh that narrows towards the target and
    // slides into it. Expanding runs the same path backwards. Toggling in the middle reverses from where it is.
    public sealed class GenieEffect
    {
        private const int Rows = 48;
        private const int Columns = 12;
        private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(480);

        private readonly FrameworkElement _content;
        private readonly Viewport3D _viewport = new() { IsHitTestVisible = false, Visibility = Visibility.Collapsed };
        private readonly OrthographicCamera _camera = new() { LookDirection = new Vector3D(0, 0, -1), UpDirection = new Vector3D(0, 1, 0) };
        private readonly MeshGeometry3D _mesh = new();
        private readonly DiffuseMaterial _material = new();

        private Rect _target;
        private double _progress;
        private int _direction;
        private TimeSpan? _lastFrame;
        private bool _running;

        public GenieEffect(FrameworkElement content, Panel host)
        {
            _content = content;
            _viewport.Camera = _camera;

            var model = new Model3DGroup();
            model.Children.Add(new AmbientLight(Colors.White));
            model.Children.Add(new GeometryModel3D(_mesh, _material));
            _viewport.Children.Add(new ModelVisual3D { Content = model });

            // Right after the content, so whatever is drawn after it (the toggle button) stays on top
            host.Children.Insert(host.Children.IndexOf(content) + 1, _viewport);
            BuildTriangles();
        }

        public bool IsCollapsed { get; private set; }

        public void Collapse(Rect target) => Start(target, 1);

        public void Expand(Rect target) => Start(target, -1);

        private void Start(Rect target, int direction)
        {
            _target = target;
            _direction = direction;
            IsCollapsed = direction > 0;

            if (_running)
                return;

            _progress = direction > 0 ? 0 : 1;
            TakeSnapshot();
            _content.Opacity = 0;
            _content.IsHitTestVisible = false;
            UpdateMesh(Ease(_progress));
            _viewport.Visibility = Visibility.Visible;

            _lastFrame = null;
            _running = true;
            CompositionTarget.Rendering += OnRendering;
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            var now = ((RenderingEventArgs)e).RenderingTime;
            if (_lastFrame is { } last)
                _progress = Math.Clamp(_progress + _direction * (now - last).TotalMilliseconds / Duration.TotalMilliseconds, 0, 1);

            _lastFrame = now;
            UpdateMesh(Ease(_progress));

            var finished = _direction > 0 ? _progress >= 1 : _progress <= 0;
            if (!finished)
                return;

            CompositionTarget.Rendering -= OnRendering;
            _running = false;
            _viewport.Visibility = Visibility.Collapsed;

            if (_progress == 0)
            {
                _content.Opacity = 1;
                _content.IsHitTestVisible = true;
            }
        }

        private void TakeSnapshot()
        {
            var size = new Size(_content.ActualWidth, _content.ActualHeight);
            var dpi = VisualTreeHelper.GetDpi(_content);

            // The content may be invisible (collapsed): render it as it looks when shown
            var opacity = _content.Opacity;
            _content.Opacity = 1;
            var bitmap = new RenderTargetBitmap(
                (int)Math.Ceiling(size.Width * dpi.DpiScaleX),
                (int)Math.Ceiling(size.Height * dpi.DpiScaleY),
                dpi.PixelsPerInchX,
                dpi.PixelsPerInchY,
                PixelFormats.Pbgra32);
            bitmap.Render(_content);
            _content.Opacity = opacity;
            bitmap.Freeze();

            _material.Brush = new ImageBrush(bitmap);
            _viewport.Width = size.Width;
            _viewport.Height = size.Height;

            // 1 unit = 1 px, y pointing down like the screen
            _camera.Width = size.Width;
            _camera.Position = new Point3D(size.Width / 2, -size.Height / 2, 10);
        }

        private void BuildTriangles()
        {
            var textureCoordinates = new PointCollection();
            var indices = new Int32Collection();
            for (var row = 0; row <= Rows; row++)
            {
                for (var column = 0; column <= Columns; column++)
                {
                    textureCoordinates.Add(new Point((double)column / Columns, (double)row / Rows));
                    if (row == Rows || column == Columns)
                        continue;

                    var topLeft = row * (Columns + 1) + column;
                    var bottomLeft = topLeft + Columns + 1;
                    indices.Add(topLeft);
                    indices.Add(bottomLeft);
                    indices.Add(topLeft + 1);
                    indices.Add(topLeft + 1);
                    indices.Add(bottomLeft);
                    indices.Add(bottomLeft + 1);
                }
            }

            _mesh.TextureCoordinates = textureCoordinates;
            _mesh.TriangleIndices = indices;
        }

        // progress 0 = content in place, 1 = inside the target
        private void UpdateMesh(double progress)
        {
            var width = _viewport.Width;
            var height = _viewport.Height;

            // First the bottom narrows towards the target, then everything slides in, the bottom rows first
            var bend = Math.Clamp(progress / 0.45, 0, 1);
            var slide = Math.Clamp((progress - 0.25) / 0.75, 0, 1);

            var positions = new Point3DCollection((Rows + 1) * (Columns + 1));
            for (var row = 0; row <= Rows; row++)
            {
                var v = (double)row / Rows;
                var rowSlide = Ease(Math.Clamp(slide * 1.6 - (1 - v) * 0.6, 0, 1));
                var y = Lerp(v * height, _target.Top + v * _target.Height, rowSlide);

                var depth = Math.Clamp(y / _target.Bottom, 0, 1);
                var pull = Math.Max(bend * (0.5 - 0.5 * Math.Cos(Math.PI * depth)), rowSlide);
                var left = Lerp(0, _target.Left, pull);
                var right = Lerp(width, _target.Right, pull);

                for (var column = 0; column <= Columns; column++)
                    positions.Add(new Point3D(Lerp(left, right, (double)column / Columns), -y, 0));
            }

            _mesh.Positions = positions;

            // Fades out at the end so nothing sticks out of the round target
            _viewport.Opacity = 1 - Ease(Math.Clamp((progress - 0.7) / 0.3, 0, 1));
        }

        private static double Lerp(double from, double to, double amount) => from + (to - from) * amount;

        private static double Ease(double t) => t * t * (3 - 2 * t);
    }
}
