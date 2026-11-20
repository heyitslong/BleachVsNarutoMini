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

    //  MÀN HÌNH MENU: 2 cột chọn nhân vật (P1 | P2) + chọn đấu trường

    public class MainMenuView : UserControl
    {
        // DANH SÁCH NHÂN VẬT 
        // Muốn thêm nhân vật mới thì ghi thêm 1 dòng.
        public static readonly KeyValuePair<string, string>[] Characters =
        {
            new KeyValuePair<string, string>("Naruto", "Naruto"),
            new KeyValuePair<string, string>("Sasuke", "Sasuke")
        };

        // Đổi id sang tên hiển thị 
        public static string DisplayName(string id)
        {
            foreach (var c in Characters)
                if (c.Key == id) return c.Value;
            return id;
        }

        // (nhân vật P1, nhân vật P2, đấu trường)  
        // [Phát thêm] Bổ sung tham số thứ 4 là thời gian trận đấu (giây, -1 = vô hạn)
        public event Action<string, string, string, int> StartClicked;

        private readonly List<RadioButton> _p1Radios = new List<RadioButton>();
        private readonly List<RadioButton> _p2Radios = new List<RadioButton>();
        private readonly List<RadioButton> _timeRadios = new List<RadioButton>(); // [Phát thêm] Danh sách nút chọn thời gian
        private readonly RadioButton _rbArena1;
        private readonly RadioButton _rbArena2;

        // [Phát thêm] Thêm tham số lastTime = 60 để ghi nhớ lựa chọn thời gian của trận trước
        public MainMenuView(string lastP1 = "Knight", string lastP2 = "Archer", string lastMap = "Arena1", int lastTime = 60)
        {
            var panel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 520
            };

            panel.Children.Add(new TextBlock
            {
                Text = "PVP ARENA",
                FontSize = 48,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Gold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 25)
            });

            //  Hai cột chọn nhân vật 
            var columns = new Grid();
            columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var colP1 = BuildColumn("NGƯỜI CHƠI 1", "P1Char", lastP1,
                                    Color.FromRgb(0x4F, 0xC3, 0xF7), _p1Radios);
            var colP2 = BuildColumn("NGƯỜI CHƠI 2", "P2Char", lastP2,
                                    Color.FromRgb(0xEF, 0x53, 0x50), _p2Radios);
            Grid.SetColumn(colP1, 0);
            Grid.SetColumn(colP2, 2);
            columns.Children.Add(colP1);
            columns.Children.Add(colP2);
            panel.Children.Add(columns);

            // Chọn đấu trường 
            panel.Children.Add(new TextBlock
            {
                Text = "Chọn đấu trường:",
                Foreground = Brushes.White,
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 8)
            });

            var mapRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 14)
            };
            _rbArena1 = UiKit.MakeRadio("Đấu trường Đá", "Map", lastMap != "Arena2", new Thickness(0, 0, 25, 0));
            _rbArena2 = UiKit.MakeRadio("Đấu trường Băng", "Map", lastMap == "Arena2", new Thickness(0));
            mapRow.Children.Add(_rbArena1);
            mapRow.Children.Add(_rbArena2);
            panel.Children.Add(mapRow);

            // [Phát thêm] Khu vực chọn thời gian trận đấu: 60s, 90s, 120s và Vô hạn
            panel.Children.Add(new TextBlock
            {
                Text = "Thời gian trận đấu:",
                Foreground = Brushes.White,
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 8)
            });

            var timeRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 22)
            };

            var timeOptions = new[]
            {
                new KeyValuePair<string, int>("60s", 60),
                new KeyValuePair<string, int>("90s", 90),
                new KeyValuePair<string, int>("120s", 120),
                new KeyValuePair<string, int>("Vô hạn", -1)
            };

            for (int i = 0; i < timeOptions.Length; i++)
            {
                var opt = timeOptions[i];
                var margin = i < timeOptions.Length - 1 ? new Thickness(0, 0, 20, 0) : new Thickness(0);
                var rb = UiKit.MakeRadio(opt.Key, "MatchTime", opt.Value == lastTime, margin);
                rb.Tag = opt.Value;
                _timeRadios.Add(rb);
                timeRow.Children.Add(rb);
            }
            panel.Children.Add(timeRow);
            // [Phát thêm] Kết thúc phần chọn thời gian

            //  Nút bấm 
            var buttons = new StackPanel { Width = 300, HorizontalAlignment = HorizontalAlignment.Center };
            buttons.Children.Add(UiKit.MakeButton("BẮT ĐẦU", Start_Click, new Thickness(0)));
            buttons.Children.Add(UiKit.MakeButton("THOÁT", Exit_Click, new Thickness(0, 10, 0, 0)));
            panel.Children.Add(buttons);

            var root = new Grid { Background = UiKit.DimBackground() };
            root.Children.Add(panel);
            Content = root;
        }

        // Dựng một cột: tiêu đề + danh sách nhân vật (RadioButton cùng nhóm)
        private static Border BuildColumn(string title, string group, string selectedId,
                                          Color accent, List<RadioButton> radios)
        {
            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = title,
                Foreground = new SolidColorBrush(accent),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 12)
            });

            foreach (var c in Characters)
            {
                var rb = UiKit.MakeRadio(c.Value, group, c.Key == selectedId, new Thickness(0, 0, 0, 8));
                rb.Tag = c.Key;                 // lưu id nhân vật để đọc lại khi bấm Bắt đầu
                radios.Add(rb);
                stack.Children.Add(rb);
            }

            // Nếu id lưu từ lần trước không còn trong danh sách thì chọn người đầu tiên
            bool any = false;
            foreach (var rb in radios) if (rb.IsChecked == true) any = true;
            if (!any && radios.Count > 0) radios[0].IsChecked = true;

            return new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x40, 255, 255, 255)),
                BorderBrush = new SolidColorBrush(accent),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20, 15, 20, 8),
                Child = stack
            };
        }

        // Đọc id nhân vật đang được chọn trong một cột
        private static string GetSelected(List<RadioButton> radios)
        {
            foreach (var rb in radios)
                if (rb.IsChecked == true) return (string)rb.Tag;
            return Characters[0].Key;
        }

        // [Phát thêm] Đọc thời gian trận đấu đang được chọn (mặc định 60s)
        private int GetSelectedTime()
        {
            foreach (var rb in _timeRadios)
                if (rb.IsChecked == true) return (int)rb.Tag;
            return 60;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            string p1 = GetSelected(_p1Radios);
            string p2 = GetSelected(_p2Radios);
            string map = _rbArena1.IsChecked == true ? "Arena1" : "Arena2";
            int seconds = GetSelectedTime(); // [Phát thêm] Lấy thời gian đã chọn
            if (StartClicked != null) StartClicked(p1, p2, map, seconds); // [Phát thêm] Truyền thêm seconds
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}