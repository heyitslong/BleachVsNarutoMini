using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BleachVsNarutoMini.UI
{
    //  HUD: THANH MÁU + THANH CHAKRA + ĐỒNG HỒ

    // Thanh máu (có chữ "80 / 100", đổi màu theo %) 
    public class HealthBar : Grid
    {
        private readonly ProgressBar _bar;
        private readonly TextBlock _text;

        public HealthBar(bool rightToLeft)
        {
            Height = 24;

            _bar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                Background = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                Foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                // P2: thanh máu giảm từ trái sang phải
                FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
            };

            _text = new TextBlock
            {
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = "100 / 100"
            };

            Children.Add(_bar);
            Children.Add(_text);
        }

        public void SetValue(double value, double max)
        {
            if (max <= 0) max = 1;
            value = Math.Max(0, Math.Min(value, max));

            _bar.Maximum = max;
            _bar.Value = value;

            // Xanh > 50%, vàng > 25%, đỏ còn lại
            double pct = value / max;
            Color c = pct > 0.5 ? Color.FromRgb(0x4C, 0xAF, 0x50)
                    : pct > 0.25 ? Color.FromRgb(0xFF, 0xC1, 0x07)
                    : Color.FromRgb(0xF4, 0x43, 0x36);
            _bar.Foreground = new SolidColorBrush(c);

            _text.Text = (int)Math.Ceiling(value) + " / " + (int)max;
        }
    }

    // Thanh chakra: nhỏ, màu xanh dương, nằm dưới thanh máu 
    public class ChakraBar : Grid
    {
        private readonly ProgressBar _bar;
        private readonly TranslateTransform _shakeTransform = new TranslateTransform(); // [Phát thêm] Dịch chuyển ngang tạo hiệu ứng rung
        private readonly DispatcherTimer _warnTimer; // [Phát thêm] Bộ đếm thời gian chạy hiệu ứng cảnh báo
        private double _warnTimeLeft; // [Phát thêm] Thời gian rung còn lại (giây)

        private static readonly SolidColorBrush NormalBrush = new SolidColorBrush(Color.FromRgb(0x29, 0xB6, 0xF6)); // [Phát thêm] Màu xanh mặc định
        private static readonly SolidColorBrush WarningBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)); // [Phát thêm] Màu đỏ cảnh báo thiếu mana

        public ChakraBar(bool rightToLeft)
        {
            Height = 10;
            Margin = new Thickness(0, 3, 0, 0);
            RenderTransform = _shakeTransform; // [Phát thêm] Gắn hiệu ứng rung ngang vào thanh Chakra

            _bar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                Background = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x33)),
                Foreground = NormalBrush,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
            };

            Children.Add(_bar);

            // [Phát thêm] Timer cập nhật khung hình rung lắc trong 0.35 giây
            _warnTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _warnTimer.Tick += OnWarnTick;
        }

        public void SetValue(double value, double max)
        {
            if (max <= 0) max = 1;
            value = Math.Max(0, Math.Min(value, max));
            _bar.Maximum = max;
            _bar.Value = value;
        }

        // [Phát thêm] Kích hoạt báo đỏ và rung thanh Chakra khi bấm chiêu lúc thiếu mana
        public void TriggerWarning()
        {
            _warnTimeLeft = 0.35;
            _bar.Foreground = WarningBrush;
            _bar.BorderBrush = Brushes.Red;
            if (!_warnTimer.IsEnabled) _warnTimer.Start();
        }

        // [Phát thêm] Xử lý rung ngang và trả về màu xanh sau khi hết cảnh báo
        private void OnWarnTick(object? sender, EventArgs e)
        {
            _warnTimeLeft -= 0.016;
            if (_warnTimeLeft <= 0)
            {
                _warnTimer.Stop();
                _shakeTransform.X = 0;
                _bar.Foreground = NormalBrush;
                _bar.BorderBrush = Brushes.Black;
                return;
            }

            double progress = _warnTimeLeft / 0.35;
            _shakeTransform.X = Math.Sin(_warnTimeLeft * 55) * 6 * progress;
        }
    }

    // HUD tổng: tên, máu, chakra của 2 người + đồng hồ ở giữa 
    public class HudView : UserControl
    {
        private readonly TextBlock _p1Name;
        private readonly TextBlock _p2Name;
        private readonly HealthBar _p1Bar;
        private readonly HealthBar _p2Bar;
        private readonly ChakraBar _p1Chakra;
        private readonly ChakraBar _p2Chakra;
        private readonly TextBlock _timeText;

        public HudView()
        {
            var grid = new Grid { Margin = new Thickness(20, 15, 20, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Người chơi 1 (bên trái) 
            var left = new StackPanel();
            _p1Name = new TextBlock
            {
                Text = "PLAYER 1",
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 3)
            };
            _p1Bar = new HealthBar(false);
            _p1Chakra = new ChakraBar(false);
            left.Children.Add(_p1Name);
            left.Children.Add(_p1Bar);
            left.Children.Add(_p1Chakra);
            Grid.SetColumn(left, 0);

            //  Giữa: đồng hồ + chữ VS 
            var center = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            _timeText = new TextBlock
            {
                Text = "60",
                Foreground = Brushes.White,
                FontSize = 30,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var vs = new TextBlock
            {
                Text = "VS",
                Foreground = Brushes.Gold,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            center.Children.Add(_timeText);
            center.Children.Add(vs);
            Grid.SetColumn(center, 1);

            //  Người chơi 2 (bên phải)
            var right = new StackPanel();
            _p2Name = new TextBlock
            {
                Text = "PLAYER 2",
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 0, 3)
            };
            _p2Bar = new HealthBar(true);
            _p2Chakra = new ChakraBar(true);
            right.Children.Add(_p2Name);
            right.Children.Add(_p2Bar);
            right.Children.Add(_p2Chakra);
            Grid.SetColumn(right, 2);

            grid.Children.Add(left);
            grid.Children.Add(center);
            grid.Children.Add(right);

            VerticalAlignment = VerticalAlignment.Top;
            IsHitTestVisible = false;          // HUD không chặn chuột/phím
            Content = grid;
        }

        public void SetNames(string p1, string p2)
        {
            _p1Name.Text = p1;
            _p2Name.Text = p2;
        }

        public void SetHealth(double p1, double p2, double max)
        {
            _p1Bar.SetValue(p1, max);
            _p2Bar.SetValue(p2, max);
        }

        public void SetChakra(double p1, double p2, double max)
        {
            _p1Chakra.SetValue(p1, max);
            _p2Chakra.SetValue(p2, max);
        }

        // [Phát thêm] Gọi hiệu ứng báo đỏ và rung thanh Chakra của người chơi tương ứng
        public void WarnLowChakra(int playerIndex)
        {
            if (playerIndex == 1) _p1Chakra.TriggerWarning();
            else if (playerIndex == 2) _p2Chakra.TriggerWarning();
        }

        // Hiện số giây còn lại; từ 10 giây trở xuống thì chuyển sang màu đỏ
        public void SetTime(int seconds)
        {
            // [Phát thêm] Hiển thị ký hiệu vô cực khi chọn chế độ vô hạn thời gian (-1)
            if (seconds < 0)
            {
                _timeText.Text = "∞";
                _timeText.Foreground = Brushes.White;
                return;
            }

            _timeText.Text = seconds.ToString();
            _timeText.Foreground = seconds <= 10 ? Brushes.Red : Brushes.White;
        }
    }
}