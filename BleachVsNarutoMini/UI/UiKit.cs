using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace BleachVsNarutoMini.UI
{
    public static class UiKit
    {
        public static readonly FontFamily DisplayFont = new("Impact, Arial Black, Segoe UI");
        public static readonly FontFamily BodyFont = new("Arial Black, Bahnschrift, Segoe UI");

        // Màu nền tối, bụi bặm, pha chút khói than
        public static Brush DimBackground() => new RadialGradientBrush
        {
            Center = new Point(0.5, 0.5),
            RadiusX = 0.9,
            RadiusY = 0.9,
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromRgb(0x1B, 0x14, 0x11), 0.0),
                new GradientStop(Color.FromRgb(0x0A, 0x07, 0x09), 1.0)
            }
        };

        /// <summary>
        /// Nút bấm Arcade phong cách thép gỉ, vát chéo, tối bụi bặm
        /// </summary>
        public static Button MakeButton(string text, RoutedEventHandler onClick, Thickness margin, bool isPrimary = true)
        {
            var normalBg = isPrimary
                ? new LinearGradientBrush(
                    Color.FromRgb(0xD8, 0x43, 0x15), // Cam cháy tối
                    Color.FromRgb(0x7E, 0x1B, 0x00),
                    new Point(0, 0), new Point(0, 1))
                : new LinearGradientBrush(
                    Color.FromRgb(0x32, 0x32, 0x3E), // Thép gỉ tối
                    Color.FromRgb(0x18, 0x18, 0x1E),
                    new Point(0, 0), new Point(0, 1));

            var hoverBg = isPrimary
                ? new LinearGradientBrush(
                    Color.FromRgb(0xFF, 0x6F, 0x00), // Rực lửa khi hover
                    Color.FromRgb(0xBF, 0x2A, 0x00),
                    new Point(0, 0), new Point(0, 1))
                : new LinearGradientBrush(
                    Color.FromRgb(0x4A, 0x4A, 0x5C),
                    Color.FromRgb(0x23, 0x23, 0x2D),
                    new Point(0, 0), new Point(0, 1));

            var normalBorder = isPrimary
                ? new SolidColorBrush(Color.FromRgb(0xFF, 0xA7, 0x26))
                : new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61));

            var glow = new DropShadowEffect
            {
                Color = isPrimary ? Color.FromRgb(0xFF, 0x57, 0x22) : Colors.Black,
                BlurRadius = isPrimary ? 14 : 6,
                ShadowDepth = 1,
                Opacity = isPrimary ? 0.7 : 0.4
            };

            var label = new TextBlock
            {
                Text = text,
                FontFamily = BodyFont,
                FontSize = 17,
                FontWeight = FontWeights.Black,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                RenderTransform = new SkewTransform(10, 0),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            var border = new Border
            {
                Background = normalBg,
                BorderBrush = normalBorder,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(1), // Góc vát vuông sắc cạnh
                Padding = new Thickness(24, 8, 24, 8),
                RenderTransform = new SkewTransform(-10, 0),
                RenderTransformOrigin = new Point(0.5, 0.5),
                Effect = glow,
                Child = label
            };

            var btn = new Button
            {
                Content = border,
                Margin = margin,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Focusable = false,
                Template = BuildCleanButtonTemplate()
            };

            btn.MouseEnter += (_, _) =>
            {
                border.Background = hoverBg;
                border.BorderBrush = isPrimary ? Brushes.White : new SolidColorBrush(Color.FromRgb(0xFF, 0xB7, 0x4D));
                if (isPrimary) glow.BlurRadius = 20;
                btn.RenderTransform = new ScaleTransform(1.04, 1.04);
                btn.RenderTransformOrigin = new Point(0.5, 0.5);
            };

            btn.MouseLeave += (_, _) =>
            {
                border.Background = normalBg;
                border.BorderBrush = normalBorder;
                glow.BlurRadius = isPrimary ? 14 : 6;
                btn.RenderTransform = Transform.Identity;
            };

            btn.Click += onClick;
            return btn;
        }

        private static ControlTemplate BuildCleanButtonTemplate()
        {
            var template = new ControlTemplate(typeof(Button));
            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            template.VisualTree = presenter;
            return template;
        }
    }
}