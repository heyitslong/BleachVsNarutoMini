using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BleachVsNarutoMini.UI
{
    public class HealthBar : Grid
    {
        private readonly ProgressBar _bar;

        public HealthBar(bool rightToLeft)
        {
            Height = 24;

            _bar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                Background = new SolidColorBrush(Color.FromRgb(0x1B, 0x14, 0x14)), // Đáy tối
                Foreground = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32)), // Xanh rêu trận mạc
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x3E, 0x27, 0x23)),
                BorderThickness = new Thickness(2),
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

            double pct = value / max;
            Color c = pct > 0.5 ? Color.FromRgb(0x38, 0x8E, 0x3C)
                    : pct > 0.25 ? Color.FromRgb(0xF5, 0x7F, 0x17) // Cam cháy
                    : Color.FromRgb(0xB7, 0x1C, 0x1C); // Đỏ thẫm
            _bar.Foreground = new SolidColorBrush(c);
        }
    }

    public class ChakraBar : Border
    {
        private readonly ProgressBar _bar;
        private readonly TranslateTransform _shakeTransform = new();
        private readonly DispatcherTimer _warnTimer;
        private double _warnElapsed;

        private static readonly Brush NormalBorder = new SolidColorBrush(Color.FromRgb(0x21, 0x21, 0x21));
        private static readonly Brush NormalBg = new SolidColorBrush(Color.FromRgb(0x12, 0x14, 0x1A));
        private static readonly Brush NormalFg = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0)); // Lam thẫm
        private static readonly Brush WarnBorder = new SolidColorBrush(Color.FromRgb(0xB7, 0x1C, 0x1C));
        private static readonly Brush WarnBg = new SolidColorBrush(Color.FromRgb(0x3E, 0x0C, 0x0C));
        private static readonly Brush WarnFg = new SolidColorBrush(Color.FromRgb(0xD3, 0x2F, 0x2F));

        public ChakraBar(bool rightToLeft)
        {
            Height = 14;
            Margin = new Thickness(0, 4, 0, 0);
            BorderBrush = NormalBorder;
            BorderThickness = new Thickness(1.5);
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

            double decay = 1.0 - (_warnElapsed / duration);
            _shakeTransform.X = Math.Sin(_warnElapsed * 55) * 6 * decay;
        }
    }

    public class GuardBar : Border
    {
        private readonly ProgressBar _bar;

        public GuardBar(bool rightToLeft)
        {
            Height = 8;
            Margin = new Thickness(0, 3, 0, 0);
            HorizontalAlignment = rightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x99, 0x1A, 0x13, 0x13));
            BorderThickness = new Thickness(1);
            Visibility = Visibility.Hidden;

            _bar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 45,
                Value = 45,
                Background = new SolidColorBrush(Color.FromArgb(0x55, 0x11, 0x11, 0x11)),
                Foreground = new SolidColorBrush(Color.FromArgb(0xC0, 0xD7, 0xCC, 0xC8)), // Trắng đục gỉ
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

    public class HudView : Grid
    {
        private readonly TextBlock _p1Name;
        private readonly TextBlock _p2Name;
        private readonly HealthBar _p1Health = new(false);
        private readonly HealthBar _p2Health = new(true);
        private readonly ChakraBar _p1Chakra = new(false);
        private readonly ChakraBar _p2Chakra = new(true);
        private readonly GuardBar _p1Guard = new(false);
        private readonly GuardBar _p2Guard = new(true);
        private readonly TextBlock _timeText;

        public HudView()
        {
            VerticalAlignment = VerticalAlignment.Top;
            Margin = new Thickness(24, 16, 24, 0);
            IsHitTestVisible = false;

            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            _p1Chakra.SizeChanged += (_, e) => _p1Guard.Width = e.NewSize.Width / 3.0;
            _p2Chakra.SizeChanged += (_, e) => _p2Guard.Width = e.NewSize.Width / 3.0;

            // Player 1
            _p1Name = new TextBlock
            {
                Text = "PLAYER 1",
                FontFamily = UiKit.BodyFont,
                Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0xB5, 0xF6)),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4)
            };
            var p1Panel = new StackPanel();
            p1Panel.Children.Add(_p1Name);
            p1Panel.Children.Add(_p1Health);
            p1Panel.Children.Add(_p1Chakra);
            p1Panel.Children.Add(_p1Guard);
            SetColumn(p1Panel, 0);
            Children.Add(p1Panel);

            // Đồng hồ & VS
            _timeText = new TextBlock
            {
                Text = "60",
                FontFamily = UiKit.DisplayFont,
                Foreground = Brushes.White,
                FontSize = 28,
                FontWeight = FontWeights.ExtraBold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var vsText = new TextBlock
            {
                Text = "VS",
                FontFamily = UiKit.DisplayFont,
                Foreground = new SolidColorBrush(Color.FromRgb(0xD8, 0x43, 0x15)),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var centerPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            centerPanel.Children.Add(_timeText);
            centerPanel.Children.Add(vsText);
            SetColumn(centerPanel, 1);
            Children.Add(centerPanel);

            // Player 2
            _p2Name = new TextBlock
            {
                Text = "PLAYER 2",
                FontFamily = UiKit.BodyFont,
                Foreground = new SolidColorBrush(Color.FromRgb(0xE5, 0x73, 0x73)),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 0, 4)
            };
            var p2Panel = new StackPanel();
            p2Panel.Children.Add(_p2Name);
            p2Panel.Children.Add(_p2Health);
            p2Panel.Children.Add(_p2Chakra);
            p2Panel.Children.Add(_p2Guard);
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

        public void SetGuard(double p1Guard, double p2Guard, double maxGuard = 45, bool showP1 = false, bool showP2 = false)
        {
            _p1Guard.Visibility = showP1 ? Visibility.Visible : Visibility.Hidden;
            _p2Guard.Visibility = showP2 ? Visibility.Visible : Visibility.Hidden;
            _p1Guard.SetValue(p1Guard, maxGuard);
            _p2Guard.SetValue(p2Guard, maxGuard);
        }

        public void SetTime(int seconds)
        {
            _timeText.Text = seconds.ToString();
            _timeText.Foreground = seconds <= 10
                ? new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28))
                : Brushes.White;
        }

        public void SetTimeInfinite()
        {
            _timeText.Text = "∞";
            _timeText.Foreground = Brushes.White;
        }

        public void WarnLowChakra(int playerIndex)
        {
            if (playerIndex == 1) _p1Chakra.TriggerWarning();
            else _p2Chakra.TriggerWarning();
        }
    }
}