
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
    // =================================================================
    //  1. DỮ LIỆU BẢN ĐỒ
    // =================================================================
    public class MapData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Brush Background { get; set; }
        public Brush ObstacleFill { get; set; }
        public Brush ObstacleBorder { get; set; }
        public Point P1Spawn { get; set; }          // vị trí xuất phát người chơi 1
        public Point P2Spawn { get; set; }          // vị trí xuất phát người chơi 2
        public List<Rect> Obstacles { get; set; } = new List<Rect>();

        // N2 dùng hàm này để chặn nhân vật / hủy đạn khi đụng hộp, tường
        public bool HitsObstacle(Rect body)
        {
            foreach (var o in Obstacles)
                if (body.IntersectsWith(o)) return true;
            return false;
        }
    }

    public static class MapLibrary
    {
        public const double Width = 1024;
        public const double Height = 640;

        public static readonly string[] AllIds = { "Arena1", "Arena2" };

        public static MapData Get(string id)
        {
            return id == "Arena2" ? CreateArena2() : CreateArena1();
        }

        // ----- Đấu trường 1: Đấu trường Đá -----
        private static MapData CreateArena1()
        {
            var map = new MapData
            {
                Id = "Arena1",
                Name = "Đấu trường Đá",
                Background = Frozen(Color.FromRgb(0x2B, 0x2B, 0x3C)),
                ObstacleFill = Frozen(Color.FromRgb(0x7A, 0x6A, 0x58)),
                ObstacleBorder = Brushes.Black,
                P1Spawn = new Point(80, 300),
                P2Spawn = new Point(920, 300)
            };

            AddBorderWalls(map.Obstacles, 20);

            map.Obstacles.Add(new Rect(472, 270, 80, 100));   // hộp lớn ở giữa
            map.Obstacles.Add(new Rect(200, 150, 100, 40));   // 4 hộp nhỏ đối xứng
            map.Obstacles.Add(new Rect(724, 150, 100, 40));
            map.Obstacles.Add(new Rect(200, 450, 100, 40));
            map.Obstacles.Add(new Rect(724, 450, 100, 40));
            return map;
        }

        // ----- Đấu trường 2: Đấu trường Băng -----
        private static MapData CreateArena2()
        {
            var map = new MapData
            {
                Id = "Arena2",
                Name = "Đấu trường Băng",
                Background = Frozen(Color.FromRgb(0x1B, 0x3A, 0x4B)),
                ObstacleFill = Frozen(Color.FromRgb(0x8E, 0xCA, 0xE6)),
                ObstacleBorder = Frozen(Color.FromRgb(0x21, 0x9E, 0xBC)),
                P1Spawn = new Point(80, 300),
                P2Spawn = new Point(920, 300)
            };

            AddBorderWalls(map.Obstacles, 20);

            map.Obstacles.Add(new Rect(300, 20, 40, 200));    // 4 cột đối xứng
            map.Obstacles.Add(new Rect(684, 20, 40, 200));
            map.Obstacles.Add(new Rect(300, 420, 40, 200));
            map.Obstacles.Add(new Rect(684, 420, 40, 200));
            map.Obstacles.Add(new Rect(432, 300, 160, 40));   // thanh ngang ở giữa
            return map;
        }

        // 4 bức tường bao quanh sân
        private static void AddBorderWalls(List<Rect> list, double t)
        {
            list.Add(new Rect(0, 0, Width, t));               // trên
            list.Add(new Rect(0, Height - t, Width, t));      // dưới
            list.Add(new Rect(0, 0, t, Height));              // trái
            list.Add(new Rect(Width - t, 0, t, Height));      // phải
        }

        private static Brush Frozen(Color c)
        {
            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }
    }

    // =================================================================
    //  2. VẼ BẢN ĐỒ LÊN MỘT CANVAS RIÊNG
    // =================================================================
    public static class MapRenderer
    {
        public static void Draw(Canvas layer, MapData map)
        {
            layer.Children.Clear();
            layer.Background = map.Background;

            var gridBrush = new SolidColorBrush(Color.FromArgb(0x14, 255, 255, 255));

            // Lưới mờ trên nền cho dễ nhìn
            for (double x = 64; x < MapLibrary.Width; x += 64)
                layer.Children.Add(new Line
                {
                    X1 = x, X2 = x, Y1 = 0, Y2 = MapLibrary.Height,
                    Stroke = gridBrush, StrokeThickness = 1
                });
            for (double y = 64; y < MapLibrary.Height; y += 64)
                layer.Children.Add(new Line
                {
                    X1 = 0, X2 = MapLibrary.Width, Y1 = y, Y2 = y,
                    Stroke = gridBrush, StrokeThickness = 1
                });

            // Vạch giữa sân
            layer.Children.Add(new Line
            {
                X1 = MapLibrary.Width / 2, X2 = MapLibrary.Width / 2,
                Y1 = 0, Y2 = MapLibrary.Height,
                Stroke = new SolidColorBrush(Color.FromArgb(0x30, 255, 255, 255)),
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 6, 6 }
            });

            // Hộp và tường
            foreach (var r in map.Obstacles)
            {
                var box = new Rectangle
                {
                    Width = r.Width,
                    Height = r.Height,
                    Fill = map.ObstacleFill,
                    Stroke = map.ObstacleBorder,
                    StrokeThickness = 2,
                    RadiusX = 3,
                    RadiusY = 3
                };
                Canvas.SetLeft(box, r.X);
                Canvas.SetTop(box, r.Y);
                layer.Children.Add(box);
            }
        }
    }

    // =================================================================
    //  3. TIỆN ÍCH GIAO DIỆN (style nút bấm, nhãn, radio)
    // =================================================================
    internal static class UiKit
    {
        private const string ButtonStyleXaml = @"
<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
       xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
       TargetType='Button'>
  <Setter Property='Foreground' Value='White'/>
  <Setter Property='Background' Value='#3A3A55'/>
  <Setter Property='FontSize' Value='18'/>
  <Setter Property='FontWeight' Value='SemiBold'/>
  <Setter Property='Cursor' Value='Hand'/>
  <Setter Property='Template'>
    <Setter.Value>
      <ControlTemplate TargetType='Button'>
        <Border x:Name='bd' Background='{TemplateBinding Background}'
                BorderBrush='Gold' BorderThickness='1' CornerRadius='8'>
          <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
        </Border>
        <ControlTemplate.Triggers>
          <Trigger Property='IsMouseOver' Value='True'>
            <Setter TargetName='bd' Property='Background' Value='#5A5A85'/>
          </Trigger>
          <Trigger Property='IsPressed' Value='True'>
            <Setter TargetName='bd' Property='Background' Value='#2A2A40'/>
          </Trigger>
        </ControlTemplate.Triggers>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>";

        private static Style _buttonStyle;

        public static Style ButtonStyle
        {
            get
            {
                if (_buttonStyle == null)
                    _buttonStyle = (Style)XamlReader.Parse(ButtonStyleXaml);
                return _buttonStyle;
            }
        }

        public static Button MakeButton(string text, RoutedEventHandler onClick, Thickness margin)
        {
            var b = new Button
            {
                Content = text,
                Height = 45,
                Margin = margin,
                Style = ButtonStyle
            };
            b.Click += onClick;
            return b;
        }

        public static TextBlock MakeLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 8)
            };
        }

        public static RadioButton MakeRadio(string text, string group, bool isChecked, Thickness margin)
        {
            return new RadioButton
            {
                Content = text,
                GroupName = group,
                IsChecked = isChecked,
                Foreground = Brushes.White,
                FontSize = 15,
                Margin = margin
            };
        }

        public static Brush DimBackground()
        {
            return new SolidColorBrush(Color.FromArgb(0xCC, 0, 0, 0));
        }
    }

    // =================================================================
    //  4. MÀN HÌNH MENU (chọn tướng + chọn đấu trường)
    // =================================================================
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

    // =================================================================
    //  5. MÀN HÌNH KẾT THÚC
    // =================================================================
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

    // =================================================================
    //  6. HUD: THANH MÁU
    // =================================================================
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

    // =================================================================
    //  7. CỬA SỔ CHÍNH (ghép tất cả lại)
    //
    //  Các lớp chồng lên nhau:
    //    MapLayer   -> bản đồ (của người 3)
    //    GameCanvas -> nhân vật, đạn (N1, N2 vẽ vào đây)
    //    Hud        -> thanh máu
    //    ScreenHost -> Menu / GameOver
    // =================================================================
    public class GameWindow : Window
    {
        // ---- N1, N2 dùng các thành viên public này ----
        public Canvas MapLayer { get; private set; }
        public Canvas GameCanvas { get; private set; }
        public HudView Hud { get; private set; }
        public MapData CurrentMap { get; private set; }
        public string SelectedCharacter { get; private set; }
        public string SelectedMap { get; private set; }

        // N1 đăng ký sự kiện này để bắt đầu game thật:
        //   window.GameStarted += (character, map) => { ... };
        public event Action<string, string> GameStarted;

        // Đặt = false khi N1 đã nối engine thật (tắt chế độ chạy thử)
        public bool DemoMode { get; set; }

        private readonly ContentControl _screenHost;
        private DispatcherTimer _demoTimer;
        private double _maxHealth = 100;

        public GameWindow()
        {
            DemoMode = true;
            SelectedCharacter = "Knight";
            SelectedMap = "Arena1";

            Title = "PVP Game";
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = Brushes.Black;

            // Khung chơi cố định 1024x640 để tọa độ bản đồ khớp màn hình
            var root = new Grid
            {
                Width = MapLibrary.Width,
                Height = MapLibrary.Height,
                ClipToBounds = true
            };

            MapLayer = new Canvas();
            GameCanvas = new Canvas { Background = Brushes.Transparent, ClipToBounds = true };
            Hud = new HudView { Visibility = Visibility.Collapsed };
            _screenHost = new ContentControl();

            root.Children.Add(MapLayer);
            root.Children.Add(GameCanvas);
            root.Children.Add(Hud);
            root.Children.Add(_screenHost);
            Content = root;

            // Vẽ sẵn đấu trường mặc định làm nền cho menu
            CurrentMap = MapLibrary.Get(SelectedMap);
            MapRenderer.Draw(MapLayer, CurrentMap);

            ShowMenu();
        }

        // ---------- Điều hướng ----------
        public void ShowMenu()
        {
            StopDemo();
            Hud.Visibility = Visibility.Collapsed;
            GameCanvas.Children.Clear();

            var menu = new MainMenuView(SelectedCharacter, SelectedMap);
            menu.StartClicked += delegate (string character, string map)
            {
                SelectedCharacter = character;
                SelectedMap = map;
                StartGame();
            };
            _screenHost.Content = menu;
        }

        public void StartGame()
        {
            StopDemo();
            _screenHost.Content = null;                 // ẩn menu / game over
            GameCanvas.Children.Clear();

            // Nạp và vẽ đấu trường đã chọn
            CurrentMap = MapLibrary.Get(SelectedMap);
            MapRenderer.Draw(MapLayer, CurrentMap);

            Hud.Visibility = Visibility.Visible;
            Hud.SetNames("PLAYER 1", "PLAYER 2");
            UpdateHud(_maxHealth, _maxHealth, _maxHealth);

            // Báo cho N1 biết để tạo Player tại CurrentMap.P1Spawn / P2Spawn
            if (GameStarted != null) GameStarted(SelectedCharacter, SelectedMap);

            if (DemoMode) StartDemo();
        }

        // N1 gọi hàm này khi có người hết máu
        public void ShowGameOver(string winnerName)
        {
            StopDemo();

            var over = new GameOverView(winnerName);
            over.ReplayClicked += StartGame;
            over.MenuClicked += ShowMenu;
            _screenHost.Content = over;
        }

        // N1 gọi hàm này mỗi frame hoặc khi có sát thương
        public void UpdateHud(double p1Health, double p2Health, double maxHealth = 100)
        {
            _maxHealth = maxHealth;
            Hud.SetHealth(p1Health, p2Health, maxHealth);
        }

        // ---------- Chế độ chạy thử (dữ liệu giả) ----------
        private void StartDemo()
        {
            double p1 = _maxHealth;
            double p2 = _maxHealth;

            _demoTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _demoTimer.Tick += delegate
            {
                p1 -= 1;
                p2 -= 2;
                UpdateHud(p1, p2, _maxHealth);

                if (p2 <= 0) ShowGameOver("Player 1");
                else if (p1 <= 0) ShowGameOver("Player 2");
            };
            _demoTimer.Start();
        }

        private void StopDemo()
        {
            if (_demoTimer != null)
            {
                _demoTimer.Stop();
                _demoTimer = null;
            }
        }
    }
}
