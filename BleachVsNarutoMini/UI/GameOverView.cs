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
    //  5. MÀN HÌNH KẾT THÚC
    public class GameOverView : UserControl
    {
        public event Action ReplayClicked;
        public event Action MenuClicked;

        public GameOverView(string winnerName)
        {
            var panel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 320
            };

            panel.Children.Add(new TextBlock
            {
                Text = "KẾT THÚC",
                FontSize = 44,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            panel.Children.Add(new TextBlock
            {
                Text = winnerName + " THẮNG!",
                FontSize = 28,
                Foreground = Brushes.Gold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 30)
            });

            panel.Children.Add(UiKit.MakeButton("CHƠI LẠI", Replay_Click, new Thickness(0)));
            panel.Children.Add(UiKit.MakeButton("VỀ MENU", Menu_Click, new Thickness(0, 10, 0, 0)));

            var root = new Grid { Background = UiKit.DimBackground() };
            root.Children.Add(panel);
            Content = root;
        }

        private void Replay_Click(object s, RoutedEventArgs e)
        {
            if (ReplayClicked != null) ReplayClicked();
        }

        private void Menu_Click(object s, RoutedEventArgs e)
        {
            if (MenuClicked != null) MenuClicked();
        }
    }
}
