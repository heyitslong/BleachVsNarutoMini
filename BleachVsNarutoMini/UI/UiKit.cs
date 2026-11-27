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
        public static readonly FontFamily BodyFont = new("Bahnschrift, Arial Black, Segoe UI");

        public static Brush DimBackground() => new RadialGradientBrush
        {
            Center = new Point(0.5, 0.45),
            RadiusX = 0.85,
            RadiusY = 0.85,
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromRgb(0x28, 0x16, 0x14), 0.0),
                new GradientStop(Color.FromRgb(0x0E, 0x0D, 0x10), 1.0)
            }
        };

        public static Button MakeButton(string text, RoutedEventHandler onClick, Thickness margin, bool isPrimary = true)
        {
            var normalBg = isPrimary
                ? new LinearGradientBrush(
                    Color.FromRgb(0x8D, 0x3B, 0x18),
                    Color.FromRgb(0x4E, 0x1D, 0x0A),
                    new Point(0, 0), new Point(0, 1))
                : new LinearGradientBrush(
                    Color.FromRgb(0x27, 0x24, 0x22),
                    Color.FromRgb(0x16, 0x14, 0x13),
                    new Point(0, 0), new Point(0, 1));

            var hoverBg = isPrimary
                ? new LinearGradientBrush(
                    Color.FromRgb(0xB8, 0x4E, 0x1E),
                    Color.FromRgb(0x6D, 0x29, 0x0E),
                    new Point(0, 0), new Point(0, 1))
                : new LinearGradientBrush(
                    Color.FromRgb(0x3A, 0x35, 0x31),
                    Color.FromRgb(0x21, 0x1E, 0x1C),
                    new Point(0, 0), new Point(0, 1));

            var normalBorder = isPrimary
                ? new SolidColorBrush(Color.FromRgb(0xC8, 0x9B, 0x62))
                : new SolidColorBrush(Color.FromRgb(0x6E, 0x62, 0x56));

            var label = new TextBlock
            {
                Text = text,
                FontFamily = BodyFont,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0xEB, 0xDF, 0xCE)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var border = new Border
            {
                Background = normalBg,
                BorderBrush = normalBorder,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(18, 7, 18, 7),
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 8, ShadowDepth = 2, Opacity = 0.7 },
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
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(0xF5, 0xD3, 0x93));
                label.Foreground = Brushes.White;
            };

            btn.MouseLeave += (_, _) =>
            {
                border.Background = normalBg;
                border.BorderBrush = normalBorder;
                label.Foreground = new SolidColorBrush(Color.FromRgb(0xEB, 0xDF, 0xCE));
            };

            btn.Click += onClick;
            return btn;
        }

        private static ControlTemplate BuildCleanButtonTemplate()
        {
            var template = new ControlTemplate(typeof(Button));
            template.VisualTree = new FrameworkElementFactory(typeof(ContentPresenter));
            return template;
        }
    }
}