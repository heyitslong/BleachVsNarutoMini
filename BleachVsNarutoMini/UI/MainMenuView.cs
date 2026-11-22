using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BleachVsNarutoMini.UI
{
    //  THÔNG TIN NHÂN VẬT HIỂN THỊ Ở MENU
    public class CharacterInfo
    {
        public string Id { get; set; }        // id dùng trong code: "Naruto", "Sasuke"
        public string Name { get; set; }      // tên hiển thị
        public string Image { get; set; }     // ảnh đứng trong thư mục Assets
        public string Special { get; set; }   // tên chiêu cuối
        public int Attack { get; set; }       // sát thương đánh thường
        public int Throw { get; set; }        // sát thương phi tiêu
        public int Ult { get; set; }          // sát thương chiêu cuối
    }

    //  TIỆN ÍCH: nạp ảnh, chữ có viền
    internal static class MenuKit
    {
        // Thư mục chứa ảnh (giống ImageCache của nhóm: Assets/Images/)
        public const string AssetFolder = "Assets/Images/";

        private static readonly Dictionary<string, ImageSource> Cache = new Dictionary<string, ImageSource>();

        // Nạp ảnh; không có ảnh thì trả về null (menu vẫn chạy bình thường).
        public static ImageSource LoadImage(string file)
        {
            ImageSource img;
            if (Cache.TryGetValue(file, out img)) return img;

            //  đường dẫn mặc định (Assets/tên_ảnh)
            img = TryLoad(new Uri("pack://application:,,,/" + AssetFolder + file, UriKind.Absolute));

            //  tự tìm ảnh trong các resource đã đóng gói, bất kể nằm thư mục con nào
            if (img == null)
            {
                string key = FindResourceKey(file);
                if (key != null)
                    img = TryLoad(new Uri("pack://application:,,,/" + key, UriKind.Absolute));
            }

            // tìm file ảnh nằm cạnh file .exe (kiểu Copy to Output Directory)
            if (img == null)
            {
                string full = FindFileOnDisk(file);
                if (full != null)
                    img = TryLoad(new Uri(full, UriKind.Absolute));
            }

            // Không tìm thấy thì ghi vào cửa sổ Output của Visual Studio để dễ kiểm tra
            if (img == null)
                System.Diagnostics.Debug.WriteLine("[MenuKit] Khong tim thay anh: " + file);

            Cache[file] = img;
            return img;
        }

        // Liệt kê các resource trong chương trình và tìm tên file khớp
        private static string FindResourceKey(string file)
        {
            try
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = asm.GetManifestResourceStream(asm.GetName().Name + ".g.resources"))
                {
                    if (stream == null) return null;
                    using (var reader = new System.Resources.ResourceReader(stream))
                    {
                        string target = file.ToLowerInvariant();
                        foreach (System.Collections.DictionaryEntry entry in reader)
                        {
                            string key = entry.Key as string;
                            if (key == null) continue;
                            if (key == target || key.EndsWith("/" + target)) return key;
                        }
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        // Tìm file ảnh trong thư mục chứa file .exe (kể cả thư mục con)
        private static string FindFileOnDisk(string file)
        {
            try
            {
                var found = System.IO.Directory.GetFiles(
                    AppDomain.CurrentDomain.BaseDirectory, file, System.IO.SearchOption.AllDirectories);
                if (found.Length > 0) return found[0];
            }
            catch
            {
            }
            return null;
        }

        private static ImageSource TryLoad(Uri uri)
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = uri;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        // Ảnh pixel art: phóng to không bị nhòe
        public static Image MakePixelImage(string file, double w, double h)
        {
            var img = new Image
            {
                Width = w,
                Height = h,
                Stretch = Stretch.Uniform,
                Source = LoadImage(file),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            return img;
        }

        // Chữ có viền đậm + đổ bóng (dùng cho tiêu đề và chữ VS)
        public static FrameworkElement MakeOutlinedText(string text, double size, Brush fill,
                                                        Brush stroke, double thickness, Color glow)
        {
            var ft = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.ExtraBold, FontStretches.Normal),
                size,
                fill,
                1.0);

            return new System.Windows.Shapes.Path
            {
                Data = ft.BuildGeometry(new Point(0, 0)),
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = thickness,
                StrokeLineJoin = PenLineJoin.Round,
                Margin = new Thickness(thickness),
                HorizontalAlignment = HorizontalAlignment.Center,
                Effect = new DropShadowEffect { Color = glow, BlurRadius = 16, ShadowDepth = 3, Opacity = 0.9 }
            };
        }
    }

    //  NHÓM THẺ BẤM (thay cho RadioButton): chọn 1 trong nhiều thẻ, thẻ được chọn có viền vàng phát sáng và phóng nhẹ lên
    internal class CardGroup
    {
        private static readonly Brush NormalBg = new SolidColorBrush(Color.FromArgb(0x33, 255, 255, 255));
        private static readonly Brush SelectedBg = new SolidColorBrush(Color.FromArgb(0x55, 255, 200, 0));
        private static readonly Brush NormalBorder = new SolidColorBrush(Color.FromArgb(0x70, 255, 255, 255));

        private readonly List<Border> _cards = new List<Border>();
        private readonly List<string> _values = new List<string>();

        public string Value { get; private set; }
        public event Action<string> Changed;

        public Border Add(string value, UIElement content, double width, double height)
        {
            var card = new Border
            {
                Width = width,
                Height = height,
                Margin = new Thickness(5),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(2),
                Background = NormalBg,
                BorderBrush = NormalBorder,
                Cursor = Cursors.Hand,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1),
                Child = content
            };
            card.MouseLeftButtonDown += delegate { Select(value); };

            _cards.Add(card);
            _values.Add(value);
            return card;
        }

        public void Select(string value)
        {
            Value = value;
            for (int i = 0; i < _cards.Count; i++)
            {
                bool on = _values[i] == value;
                var c = _cards[i];
                c.BorderBrush = on ? Brushes.Gold : NormalBorder;
                c.Background = on ? SelectedBg : NormalBg;

                var scale = (ScaleTransform)c.RenderTransform;
                scale.ScaleX = on ? 1.08 : 1.0;
                scale.ScaleY = on ? 1.08 : 1.0;

                c.Effect = on
                    ? new DropShadowEffect { Color = Colors.Gold, BlurRadius = 18, ShadowDepth = 0, Opacity = 0.9 }
                    : null;
            }
            if (Changed != null) Changed(value);
        }
    }

    //  CỘT CỦA MỘT NGƯỜI CHƠI: ảnh to + chỉ số + các thẻ chọn nhân vật
    internal class PlayerPanel
    {
        public Border Root { get; private set; }
        public CardGroup Group { get; private set; }

        private readonly Image _preview;
        private readonly TextBlock _name;
        private readonly TextBlock _special;
        private ProgressBar _barAtk, _barThrow, _barUlt;
        private TextBlock _numAtk, _numThrow, _numUlt;

        public PlayerPanel(string title, Color accent, string selectedId, CharacterInfo[] list)
        {
            Group = new CardGroup();
            var stack = new StackPanel();

            // Tiêu đề cột
            stack.Children.Add(new TextBlock
            {
                Text = title,
                Foreground = new SolidColorBrush(accent),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            });

            // Ảnh nhân vật phóng to (cập nhật mỗi khi chọn)
            _preview = MenuKit.MakePixelImage(list[0].Image, 130, 130);
            stack.Children.Add(new Border
            {
                Width = 170,
                Height = 140,
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromArgb(0x55, 0, 0, 0)),
                BorderBrush = new SolidColorBrush(accent),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = _preview
            });

            // Tên + chiêu cuối
            _name = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 6, 0, 0)
            };
            _special = new TextBlock
            {
                Foreground = Brushes.Gold,
                FontSize = 13,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 6)
            };
            stack.Children.Add(_name);
            stack.Children.Add(_special);

            // Bảng chỉ số nhỏ
            AddStatRow(stack, "Đánh thường", 10, Color.FromRgb(0xFF, 0xA7, 0x26), out _barAtk, out _numAtk);
            AddStatRow(stack, "Phi tiêu", 20, Color.FromRgb(0x29, 0xB6, 0xF6), out _barThrow, out _numThrow);
            AddStatRow(stack, "Chiêu cuối", 50, Color.FromRgb(0xEF, 0x53, 0x50), out _barUlt, out _numUlt);

            // Các thẻ chọn nhân vật
            var cards = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 6, 0, 0)
            };
            foreach (var c in list)
            {
                var content = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                content.Children.Add(MenuKit.MakePixelImage(c.Image, 46, 46));
                content.Children.Add(new TextBlock
                {
                    Text = c.Name,
                    Foreground = Brushes.White,
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 2, 0, 0)
                });
                cards.Children.Add(Group.Add(c.Id, content, 84, 76));
            }
            stack.Children.Add(cards);

            Root = new Border
            {
                Width = 320,
                Background = new SolidColorBrush(Color.FromArgb(0x99, 10, 10, 25)),
                BorderBrush = new SolidColorBrush(accent),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(12),
                Child = stack
            };

            // Khi chọn thẻ khác thì cập nhật ảnh to và chỉ số
            Group.Changed += delegate (string id) { Update(Find(list, id)); };
            Group.Select(Find(list, selectedId).Id);
        }

        private static CharacterInfo Find(CharacterInfo[] list, string id)
        {
            foreach (var c in list)
                if (c.Id == id) return c;
            return list[0];
        }

        private void Update(CharacterInfo info)
        {
            _preview.Source = MenuKit.LoadImage(info.Image);
            _name.Text = info.Name;
            _special.Text = "Chiêu cuối: " + info.Special;

            _barAtk.Value = info.Attack;
            _barThrow.Value = info.Throw;
            _barUlt.Value = info.Ult;
            _numAtk.Text = info.Attack.ToString();
            _numThrow.Text = info.Throw.ToString();
            _numUlt.Text = info.Ult.ToString();
        }

        // Một dòng chỉ số: tên | thanh | số
        private static void AddStatRow(StackPanel parent, string label, double max, Color color,
                                       out ProgressBar bar, out TextBlock num)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(92) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });

            var lb = new TextBlock { Text = label, Foreground = Brushes.White, FontSize = 12 };

            bar = new ProgressBar
            {
                Maximum = max,
                Height = 8,
                Foreground = new SolidColorBrush(color),
                Background = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                BorderThickness = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 4, 0)
            };

            num = new TextBlock
            {
                Foreground = Brushes.Gold,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Grid.SetColumn(bar, 1);
            Grid.SetColumn(num, 2);
            grid.Children.Add(lb);
            grid.Children.Add(bar);
            grid.Children.Add(num);
            parent.Children.Add(grid);
        }
    }
    //  MÀN HÌNH MENU: CHỌN TƯỚNG 
       public class MainMenuView : UserControl
    {
        // DANH SÁCH NHÂN VẬT
        // Muốn thêm nhân vật mới: thêm một dòng vào đây (và một nhánh trong GameWindow.CreateFighter).
        public static readonly CharacterInfo[] CharacterList =
        {
            new CharacterInfo { Id = "Naruto", Name = "Naruto", Image = "narutodung.png",
                                Special = "Rasengan", Attack = 5, Throw = 12, Ult = 40 },
            new CharacterInfo { Id = "Sasuke", Name = "Sasuke", Image = "sskdung1.png",
                                Special = "Chidori", Attack = 6, Throw = 18, Ult = 32 }
        };

        // Đổi id ("Naruto") sang tên hiển thị
        public static string DisplayName(string id)
        {
            foreach (var c in CharacterList)
                if (c.Id == id) return c.Name;
            return id;
        }

        // Các mức thời gian (giây). "0" nghĩa là không giới hạn.
        private static readonly string[] TimeValues = { "30", "60", "90", "0" };
        private static readonly string[] TimeLabels = { "30s", "60s", "90s", "Vô hạn" };

        // (nhân vật P1, nhân vật P2, đấu trường, số giây của trận)
        public event Action<string, string, string, int> StartClicked;

        private readonly PlayerPanel _p1;
        private readonly PlayerPanel _p2;
        private readonly CardGroup _mapGroup = new CardGroup();
        private readonly CardGroup _timeGroup = new CardGroup();

        public MainMenuView(string lastP1 = "Naruto", string lastP2 = "Sasuke",
                            string lastMap = "Arena1", int lastSeconds = 60)
        {
            // Nền: ảnh menu_bg.png phủ lớp mờ
            var root = new Grid();
            var bg = MenuKit.LoadImage("menu_bg.png");
            if (bg != null)
            {
                root.Children.Add(new Image { Source = bg, Stretch = Stretch.UniformToFill });
                root.Children.Add(new System.Windows.Shapes.Rectangle
                {
                    Fill = new SolidColorBrush(Color.FromArgb(0x88, 0, 0, 0))
                });
            }
            else
            {
                root.Background = UiKit.DimBackground();
            }

            var panel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            //  Tiêu đề game 
            var title = MenuKit.MakeOutlinedText("BLEACH VS NARUTO", 44,
                new SolidColorBrush(Color.FromRgb(0xFF, 0xD5, 0x4F)),
                new SolidColorBrush(Color.FromRgb(0x3E, 0x1A, 0x00)), 4, Colors.Black);
            title.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(title);

            //  Hai cột người chơi + chữ VS ở giữa
            _p1 = new PlayerPanel("NGƯỜI CHƠI 1", Color.FromRgb(0x4F, 0xC3, 0xF7), lastP1, CharacterList);
            _p2 = new PlayerPanel("NGƯỜI CHƠI 2", Color.FromRgb(0xEF, 0x53, 0x50), lastP2, CharacterList);

            var vs = MenuKit.MakeOutlinedText("VS", 54,
                new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)),
                new SolidColorBrush(Color.FromRgb(0xB7, 0x1C, 0x1C)), 3, Colors.OrangeRed);
            vs.VerticalAlignment = VerticalAlignment.Center;
            vs.Margin = new Thickness(18, 0, 18, 0);

            var cols = new Grid { HorizontalAlignment = HorizontalAlignment.Center };
            cols.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            cols.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            cols.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(_p1.Root, 0);
            Grid.SetColumn(vs, 1);
            Grid.SetColumn(_p2.Root, 2);
            cols.Children.Add(_p1.Root);
            cols.Children.Add(vs);
            cols.Children.Add(_p2.Root);
            panel.Children.Add(cols);

            //  Hàng dưới: đấu trường (ảnh thu nhỏ) + thời gian 
            var options = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };

            // Đấu trường
            var mapBox = new StackPanel();
            mapBox.Children.Add(MakeLabel("Chọn đấu trường"));
            var mapRow = new StackPanel { Orientation = Orientation.Horizontal };
            mapRow.Children.Add(MakeMapCard("Arena1", "Đấu trường Đá", "arena1_bg.png", Color.FromRgb(0x2B, 0x2B, 0x3C)));
            mapRow.Children.Add(MakeMapCard("Arena2", "Đấu trường Băng", "arena2_bg.png", Color.FromRgb(0x1B, 0x3A, 0x4B)));
            mapBox.Children.Add(mapRow);
            options.Children.Add(mapBox);

            // Thời gian
            var timeBox = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(40, 0, 0, 0)
            };
            timeBox.Children.Add(MakeLabel("Thời gian trận"));
            var timeRow = new StackPanel { Orientation = Orientation.Horizontal };
            for (int i = 0; i < TimeValues.Length; i++)
            {
                var text = new TextBlock
                {
                    Text = TimeLabels[i],
                    Foreground = Brushes.White,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                timeRow.Children.Add(_timeGroup.Add(TimeValues[i], text, 66, 36));
            }
            timeBox.Children.Add(timeRow);
            options.Children.Add(timeBox);
            panel.Children.Add(options);

            // Chọn sẵn lựa chọn của lần trước
            _mapGroup.Select(lastMap == "Arena2" ? "Arena2" : "Arena1");
            string t = lastSeconds.ToString();
            _timeGroup.Select(Array.IndexOf(TimeValues, t) >= 0 ? t : "60");

            // Nút bấm 
            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var start = UiKit.MakeButton("BẮT ĐẦU", Start_Click, new Thickness(0));
            start.Width = 220;
            var exit = UiKit.MakeButton("THOÁT", Exit_Click, new Thickness(12, 0, 0, 0));
            exit.Width = 150;
            buttons.Children.Add(start);
            buttons.Children.Add(exit);
            panel.Children.Add(buttons);

            // Viewbox: nếu cửa sổ nhỏ hơn nội dung thì tự thu nhỏ cho vừa, không bị cắt
            root.Children.Add(new Viewbox
            {
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.DownOnly,
                Margin = new Thickness(10),
                Child = panel
            });
            Content = root;
        }

        private static TextBlock MakeLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 0, 0, 2)
            };
        }

        // Thẻ đấu trường: ảnh thu nhỏ + tên. Không có ảnh thì tô màu nền của đấu trường.
        private Border MakeMapCard(string id, string caption, string imageFile, Color fallback)
        {
            UIElement thumb;
            var src = MenuKit.LoadImage(imageFile);
            if (src != null)
                thumb = new Image { Source = src, Width = 120, Height = 75, Stretch = Stretch.UniformToFill };
            else
                thumb = new System.Windows.Shapes.Rectangle { Width = 120, Height = 75, Fill = new SolidColorBrush(fallback) };

            var content = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            content.Children.Add(thumb);
            content.Children.Add(new TextBlock
            {
                Text = caption,
                Foreground = Brushes.White,
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 3, 0, 0)
            });
            return _mapGroup.Add(id, content, 132, 104);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            int seconds;
            if (!int.TryParse(_timeGroup.Value, out seconds)) seconds = 60;

            if (StartClicked != null)
                StartClicked(_p1.Group.Value, _p2.Group.Value, _mapGroup.Value, seconds);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}