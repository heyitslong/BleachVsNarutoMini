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
    //  HUD: THANH MÁU
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

    public class HudView : UserControl
    {
        private readonly TextBlock _p1Name;
        private readonly TextBlock _p2Name;
        private readonly HealthBar _p1Bar;
        private readonly HealthBar _p2Bar;

        public HudView()
        {
            var grid = new Grid { Margin = new Thickness(20, 15, 20, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
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
            left.Children.Add(_p1Name);
            left.Children.Add(_p1Bar);
            Grid.SetColumn(left, 0);

            // VS
            var vs = new TextBlock
            {
                Text = "VS",
                Foreground = Brushes.Gold,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(vs, 1);

            // Người chơi 2 (bên phải)
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
            right.Children.Add(_p2Name);
            right.Children.Add(_p2Bar);
            Grid.SetColumn(right, 2);

            grid.Children.Add(left);
            grid.Children.Add(vs);
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
    }
}
