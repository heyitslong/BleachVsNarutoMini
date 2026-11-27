using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace BleachVsNarutoMini.UI
{
    public class GameOverView : UserControl
    {
        public event Action? ReplayClicked;
        public event Action? MenuClicked;

        public GameOverView(string winnerName)
        {
            var root = new Grid { Background = UiKit.DimBackground() };

            // Phủ hiệu ứng khói bụi gỉ mờ
            var smoke = new Rectangle
            {
                Fill = new RadialGradientBrush(Color.FromArgb(0x11, 0, 0, 0), Color.FromArgb(0xAA, 0, 0, 0)),
                IsHitTestVisible = false
            };
            root.Children.Add(smoke);

            var panel = new Border
            {
                Width = 360,
                Background = new SolidColorBrush(Color.FromArgb(0xF0, 0x12, 0x10, 0x14)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xD8, 0x43, 0x15)), // Viền sắt tối
                BorderThickness = new Thickness(3),
                CornerRadius = new CornerRadius(2),
                Padding = new Thickness(24, 28, 24, 28),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(0xBF, 0x36, 0x0C),
                    BlurRadius = 24,
                    ShadowDepth = 0,
                    Opacity = 0.7
                }
            };

            var content = new StackPanel();

            // Chữ KẾT THÚC
            var title = MenuKit.MakeOutlinedText("KẾT THÚC", 40,
            new LinearGradientBrush(Color.FromRgb(0xEF, 0x53, 0x50), Color.FromRgb(0xB7, 0x1C, 0x1C), new Point(0, 0), new Point(0, 1)),
            Brushes.Black, -8);
            title.Margin = new Thickness(0, 0, 0, 10);
            content.Children.Add(title);

            // Dải ruy băng xéo phân cách
            content.Children.Add(new Line
            {
                X1 = 0,
                X2 = 312,
                Y1 = 0,
                Y2 = 0,
                Stroke = new SolidColorBrush(Color.FromRgb(0xFF, 0x8F, 0x00)),
                StrokeThickness = 2,
                Margin = new Thickness(0, 0, 0, 14)
            });

            // Người thắng
            var winnerText = new TextBlock
            {
                Text = winnerName == null ? "HÒA NHAU!" : winnerName + " THẮNG!",
                FontFamily = UiKit.DisplayFont,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = winnerName == null ? Brushes.White : new SolidColorBrush(Color.FromRgb(0xFF, 0xCA, 0x28)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 28),
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 4, ShadowDepth = 2 }
            };
            content.Children.Add(winnerText);

            content.Children.Add(UiKit.MakeButton("CHƠI LẠI", Replay_Click, new Thickness(0, 0, 0, 12), isPrimary: true));
            content.Children.Add(UiKit.MakeButton("VỀ MENU", Menu_Click, new Thickness(0), isPrimary: false));

            panel.Child = content;
            root.Children.Add(panel);
            Content = root;
        }

        private void Replay_Click(object s, RoutedEventArgs e) => ReplayClicked?.Invoke();
        private void Menu_Click(object s, RoutedEventArgs e) => MenuClicked?.Invoke();
    }
}