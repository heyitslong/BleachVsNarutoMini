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
    //  MÀN HÌNH MENU (chọn tướng + chọn đấu trường)
    //
    public class MainMenuView : UserControl
    {
        // (nhân vật, đấu trường)  ví dụ: ("Knight", "Arena2")
        public event Action<string, string> StartClicked;

        private readonly RadioButton _rbKnight;
        private readonly RadioButton _rbArcher;
        private readonly RadioButton _rbArena1;
        private readonly RadioButton _rbArena2;

        public MainMenuView(string lastCharacter = "Knight", string lastMap = "Arena1")
        {
            var panel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 320
            };

            panel.Children.Add(new TextBlock
            {
                Text = "PVP ARENA",
                FontSize = 48,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Gold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 30)
            });

            // Chọn tướng
            panel.Children.Add(UiKit.MakeLabel("Chọn tướng:"));
            _rbKnight = UiKit.MakeRadio("Hiệp sĩ", "Char", lastCharacter != "Archer", new Thickness(0));
            _rbArcher = UiKit.MakeRadio("Cung thủ", "Char", lastCharacter == "Archer", new Thickness(0, 5, 0, 20));
            panel.Children.Add(_rbKnight);
            panel.Children.Add(_rbArcher);

            // Chọn đấu trường
            panel.Children.Add(UiKit.MakeLabel("Chọn đấu trường:"));
            _rbArena1 = UiKit.MakeRadio("Đấu trường Đá", "Map", lastMap != "Arena2", new Thickness(0));
            _rbArena2 = UiKit.MakeRadio("Đấu trường Băng", "Map", lastMap == "Arena2", new Thickness(0, 5, 0, 25));
            panel.Children.Add(_rbArena1);
            panel.Children.Add(_rbArena2);

            // Nút
            panel.Children.Add(UiKit.MakeButton("BẮT ĐẦU", Start_Click, new Thickness(0)));
            panel.Children.Add(UiKit.MakeButton("THOÁT", Exit_Click, new Thickness(0, 10, 0, 0)));

            var root = new Grid { Background = UiKit.DimBackground() };
            root.Children.Add(panel);
            Content = root;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            string character = _rbKnight.IsChecked == true ? "Knight" : "Archer";
            string map = _rbArena1.IsChecked == true ? "Arena1" : "Arena2";
            if (StartClicked != null) StartClicked(character, map);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
