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
    //  CỬA SỔ CHÍNH (ghép tất cả lại)
    public class GameWindow : Window
    {
        public Canvas MapLayer { get; private set; }
        public Canvas GameCanvas { get; private set; }
        public HudView Hud { get; private set; }
        public MapData CurrentMap { get; private set; }
        public string SelectedCharacter { get; private set; }
        public string SelectedMap { get; private set; }

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

        // Điều hướng 
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

        // gọi hàm này khi có người hết máu
        public void ShowGameOver(string winnerName)
        {
            StopDemo();

            var over = new GameOverView(winnerName);
            over.ReplayClicked += StartGame;
            over.MenuClicked += ShowMenu;
            _screenHost.Content = over;
        }

        //  gọi hàm này mỗi frame hoặc khi có sát thương
        public void UpdateHud(double p1Health, double p2Health, double maxHealth = 100)
        {
            _maxHealth = maxHealth;
            Hud.SetHealth(p1Health, p2Health, maxHealth);
        }

        //  Chế độ chạy thử (dữ liệu giả) 
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
