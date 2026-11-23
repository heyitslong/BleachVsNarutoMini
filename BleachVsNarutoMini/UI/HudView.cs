using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BleachVsNarutoMini.UI
{
    // Thanh máu (đổi màu theo %)
    public class HealthBar : Grid
    {
        private readonly ProgressBar _bar;
        // [Phát thêm] Đã xóa TextBlock _text để ẩn dòng chữ số máu (300 / 300) trên thanh máu

        public HealthBar(bool rightToLeft)
        {
            Height = 24;

            _bar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 45,
                Value = 45,
                Background = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                Foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                // P2: thanh máu giảm từ trái sang phải
                FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
            };

            Children.Add(_bar);
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
        }
    }

    // Thanh Chakra (kèm hiệu ứng báo đỏ và rung khi bấm chiêu lúc thiếu năng lượng)
    public class ChakraBar : Border
    {
        private readonly ProgressBar _bar;
        private readonly TranslateTransform _shakeTransform = new(); // [Phát thêm] Dịch chuyển ngang để tạo hiệu ứng rung
        private readonly DispatcherTimer _warnTimer;                 // [Phát thêm] Bộ đếm thời gian cảnh báo (0.35s)
        private double _warnElapsed;

        private static readonly Brush NormalBorder = Brushes.Black;
        private static readonly Brush NormalBg = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B));
        private static readonly Brush NormalFg = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));
        private static readonly Brush WarnBorder = new SolidColorBrush(Color.FromRgb(0xFF, 0x17, 0x44));
        private static readonly Brush WarnBg = new SolidColorBrush(Color.FromRgb(0x5C, 0x10, 0x1A));
        private static readonly Brush WarnFg = new SolidColorBrush(Color.FromRgb(0xFF, 0x52, 0x52));

        public ChakraBar(bool rightToLeft)
        {
            Height = 14;
            Margin = new Thickness(0, 4, 0, 0);
            BorderBrush = NormalBorder;
            BorderThickness = new Thickness(1);
            RenderTransform = _shakeTransform;

            _bar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 200,
                Value = 200,
                Background = NormalBg,
                Foreground = NormalFg,
                BorderThickness = new Thickness(0),
                FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
            };

            Child = _bar;

            // [Phát thêm] Timer chạy hiệu ứng rung và đổi màu đỏ trong 0.35 giây
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

        // [Phát thêm] Kích hoạt cảnh báo đỏ + rung ngang khi bấm chiêu lúc thiếu Chakra
        public void TriggerWarning()
        {
            _warnElapsed = 0;
            BorderBrush = WarnBorder;
            _bar.Background = WarnBg;
            _bar.Foreground = WarnFg;
            if (!_warnTimer.IsEnabled) _warnTimer.Start();
        }

        private void OnWarnTick(object? sender, EventArgs e)
        {
            _warnElapsed += 0.016;
            const double duration = 0.35;

            if (_warnElapsed >= duration)
            {
                _warnTimer.Stop();
                _shakeTransform.X = 0;
                BorderBrush = NormalBorder;
                _bar.Background = NormalBg;
                _bar.Foreground = NormalFg;
                return;
            }

            // Biên độ rung giảm dần theo thời gian
            double decay = 1.0 - (_warnElapsed / duration);
            _shakeTransform.X = Math.Sin(_warnElapsed * 55) * 6 * decay;
        }
    }

    // [Phát thêm] Thanh chống đỡ màu trắng hơi trong suốt, rộng bằng 1/3 thanh Chakra
    public class GuardBar : Border
    {
        private readonly ProgressBar _bar;

        public GuardBar(bool rightToLeft)
        {
            Height = 8;
            Margin = new Thickness(0, 3, 0, 0);
            HorizontalAlignment = rightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x88, 0x00, 0x00, 0x00));
            BorderThickness = new Thickness(1);

            _bar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 30,
                Value = 30,
                Background = new SolidColorBrush(Color.FromArgb(0x45, 0x00, 0x00, 0x00)),
                Foreground = new SolidColorBrush(Color.FromArgb(0xB8, 0xFF, 0xFF, 0xFF)), // [Phát thêm] Màu trắng hơi trong suốt
                BorderThickness = new Thickness(0),
                FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
            };

            Child = _bar;
        }

        public void SetValue(double value, double max)
        {
            if (max <= 0) max = 1;
            value = Math.Max(0, Math.Min(value, max));

            _bar.Maximum = max;
            _bar.Value = value;
        }
    }

    // Giao diện HUD tổng hợp phía trên màn hình trận đấu
    public class HudView : Grid
    {
        private readonly TextBlock _p1Name;
        private readonly TextBlock _p2Name;
        private readonly HealthBar _p1Health = new(false);
        private readonly HealthBar _p2Health = new(true);
        private readonly ChakraBar _p1Chakra = new(false);
        private readonly ChakraBar _p2Chakra = new(true);
        private readonly GuardBar _p1Guard = new(false); // [Phát thêm] Thanh chống đỡ P1
        private readonly GuardBar _p2Guard = new(true);  // [Phát thêm] Thanh chống đỡ P2
        private readonly TextBlock _timeText;

        public HudView()
        {
            VerticalAlignment = VerticalAlignment.Top;
            Margin = new Thickness(24, 16, 24, 0);
            IsHitTestVisible = false;

            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // [Phát thêm] Giữ chiều rộng thanh chống đỡ luôn bằng đúng 1/3 thanh Chakra
            _p1Chakra.SizeChanged += (_, e) => _p1Guard.Width = e.NewSize.Width / 3.0;
            _p2Chakra.SizeChanged += (_, e) => _p2Guard.Width = e.NewSize.Width / 3.0;

            // Cụm thanh trạng thái Player 1 (Trái)
            _p1Name = new TextBlock
            {
                Text = "PLAYER 1",
                Foreground = Brushes.White,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4)
            };
            var p1Panel = new StackPanel();
            p1Panel.Children.Add(_p1Name);
            p1Panel.Children.Add(_p1Health);
            p1Panel.Children.Add(_p1Chakra);
            p1Panel.Children.Add(_p1Guard); // [Phát thêm] Gắn thanh chống đỡ ngay dưới thanh Chakra của P1
            SetColumn(p1Panel, 0);
            Children.Add(p1Panel);

            // Đồng hồ & chữ VS ở giữa
            _timeText = new TextBlock
            {
                Text = "60",
                Foreground = Brushes.White,
                FontSize = 28,
                FontWeight = FontWeights.ExtraBold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var vsText = new TextBlock
            {
                Text = "VS",
                Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xD5, 0x4F)),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var centerPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            centerPanel.Children.Add(_timeText);
            centerPanel.Children.Add(vsText);
            SetColumn(centerPanel, 1);
            Children.Add(centerPanel);

            // Cụm thanh trạng thái Player 2 (Phải)
            _p2Name = new TextBlock
            {
                Text = "PLAYER 2",
                Foreground = Brushes.White,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 0, 4)
            };
            var p2Panel = new StackPanel();
            p2Panel.Children.Add(_p2Name);
            p2Panel.Children.Add(_p2Health);
            p2Panel.Children.Add(_p2Chakra);
            p2Panel.Children.Add(_p2Guard); // [Phát thêm] Gắn thanh chống đỡ ngay dưới thanh Chakra của P2
            SetColumn(p2Panel, 2);
            Children.Add(p2Panel);
        }

        public void SetNames(string p1Name, string p2Name)
        {
            _p1Name.Text = p1Name;
            _p2Name.Text = p2Name;
        }

        public void SetHealth(double p1Health, double p2Health, double maxHealth = 100)
        {
            _p1Health.SetValue(p1Health, maxHealth);
            _p2Health.SetValue(p2Health, maxHealth);
        }

        public void SetChakra(double p1Chakra, double p2Chakra, double maxChakra = 100)
        {
            _p1Chakra.SetValue(p1Chakra, maxChakra);
            _p2Chakra.SetValue(p2Chakra, maxChakra);
        }

        // [Phát thêm] Cập nhật giá trị thanh chống đỡ của 2 người chơi (tối đa 45)
        public void SetGuard(double p1Guard, double p2Guard, double maxGuard = 45)
        {
            _p1Guard.SetValue(p1Guard, maxGuard);
            _p2Guard.SetValue(p2Guard, maxGuard);
        }

        public void SetTime(int seconds)
        {
            _timeText.Text = seconds.ToString();
            _timeText.Foreground = seconds <= 10
                ? new SolidColorBrush(Color.FromRgb(0xFF, 0x52, 0x52))
                : Brushes.White;
        }
        //Nghĩa thêm, settimeinfinite để tính thời gian vô hạn
        public void SetTimeInfinite()
        {
            _timeText.Text = "∞";
            _timeText.Foreground = Brushes.White;
        }
        // [Phát thêm] Gọi hiệu ứng nháy đỏ và rung thanh Chakra của người chơi tương ứng
        public void WarnLowChakra(int playerIndex)
        {
            if (playerIndex == 1) _p1Chakra.TriggerWarning();
            else _p2Chakra.TriggerWarning();
        }
    }
}