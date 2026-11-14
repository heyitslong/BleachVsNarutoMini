using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Tui thêm thư viện này để nhận diện phím bấm
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

        public bool DemoMode { get; set; }

        private readonly ContentControl _screenHost;
        private DispatcherTimer _demoTimer;
        private double _maxHealth = 100;

        // === PHẦN CODE CỦA BẠN (Khai báo nhân vật và Timer riêng) ===
        private Entities.Naruto player1;
        private Entities.Sasuke player2;

        private TimeSpan _lastRender;
        private bool _running;
        private int _shownHp1 = -1, _shownHp2 = -1;
        // ==============================================================
        private double p1Health = 100;
        private double p2Health = 100;

        public GameWindow()
        {
            DemoMode = false;
            SelectedCharacter = "Knight";
            SelectedMap = "Arena1";

            Title = "PVP Game";
            InputMethod.SetIsInputMethodEnabled(this, false);
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = Brushes.Black;

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

            CurrentMap = MapLibrary.Get(SelectedMap);
            MapRenderer.Draw(MapLayer, CurrentMap);

           

            ShowMenu();
        }

        // === BỘ NÃO HOẠT ĐỘNG CỦA NHÂN VẬT ===
        private void StartLoop()
        {
            _lastRender = TimeSpan.Zero;
            if (_running) return;
            CompositionTarget.Rendering += OnRendering;
            _running = true;
        }

        private void StopLoop()
        {
            if (!_running) return;
            CompositionTarget.Rendering -= OnRendering;
            _running = false;
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            var now = ((RenderingEventArgs)e).RenderingTime;
            if (_lastRender == TimeSpan.Zero) { _lastRender = now; return; }

            double dt = (now - _lastRender).TotalSeconds;
            if (dt <= 0) return;              // WPF đôi khi báo trùng một khung
            _lastRender = now;
            if (dt > 0.05) dt = 0.05;         // tránh nhân vật "nhảy vọt" khi bị khựng

            if (player1 == null || player2 == null) return;

            player1.UpdateLogic(dt);
            player2.UpdateLogic(dt);
            player1.TryHit(player2);
            player2.TryHit(player1);

            // Chỉ cập nhật thanh máu khi máu thật sự đổi
            if (player1.Hp != _shownHp1 || player2.Hp != _shownHp2)
            {
                _shownHp1 = player1.Hp;
                _shownHp2 = player2.Hp;
                UpdateHud(player1.Hp, player2.Hp, _maxHealth);
            }

            if (player1.IsDead && player2.IsDead) ShowGameOver("HÒA");
            else if (player1.IsDead) ShowGameOver("PLAYER 2 (SASUKE)");
            else if (player2.IsDead) ShowGameOver("PLAYER 1 (NARUTO)");
        }

        public void ShowMenu()
        {
            StopDemo();
            StopLoop(); // Tắt nhân vật khi ra menu
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
            _screenHost.Content = null;
            GameCanvas.Children.Clear();

            CurrentMap = MapLibrary.Get(SelectedMap);
            MapRenderer.Draw(MapLayer, CurrentMap);

            p1Health = _maxHealth;
            p2Health = _maxHealth;

            Hud.Visibility = Visibility.Visible;
            Hud.SetNames("PLAYER 1", "PLAYER 2");
            UpdateHud(p1Health, p2Health, _maxHealth);

            if (GameStarted != null) GameStarted(SelectedCharacter, SelectedMap);

            // === ĐƯA NHÂN VẬT LÊN VÕ ĐÀI ===
            player1 = new Entities.Naruto { StageWidth = MapLibrary.Width };
            player2 = new Entities.Sasuke { StageWidth = MapLibrary.Width };
            GameCanvas.Children.Add(player1.Sprite);
            GameCanvas.Children.Add(player2.Sprite);
            StartLoop(); // Bắt đầu cho phép di chuyển
            // ===============================

            // Code của bạn bè vẫn chạy bình thường không sứt mẻ 1 miếng
            if (DemoMode) StartDemo();
        }

        public void ShowGameOver(string winnerName)
        {
            StopDemo();
            StopLoop(); // Dừng nhân vật khi có người thắng

            var over = new GameOverView(winnerName);
            over.ReplayClicked += StartGame;
            over.MenuClicked += ShowMenu;
            _screenHost.Content = over;
        }

        public void UpdateHud(double p1Health, double p2Health, double maxHealth = 100)
        {
            _maxHealth = maxHealth;
            Hud.SetHealth(p1Health, p2Health, maxHealth);
        }

        // CHẾ ĐỘ CỦA BẠN BÈ (Không đụng chạm)
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

        // === BẮT SỰ KIỆN BÀN PHÍM TỪ BÊN NGOÀI ===
        private static Key Normalize(KeyEventArgs e)
        {
            var key = e.Key == Key.ImeProcessed ? e.ImeProcessedKey : e.Key;

            // Đổi phím số Numpad thành phím số hàng trên, để cả hai đều dùng được
            if (key >= Key.NumPad0 && key <= Key.NumPad9)
                key = key - Key.NumPad0 + Key.D0;
            return key;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            BleachVsNarutoMini.Engine.InputManager.KeyDown(Normalize(e));
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            BleachVsNarutoMini.Engine.InputManager.KeyUp(Normalize(e));
        }
        private bool CheckCollision(Image img1, Image img2)
        {
            // Lấy tọa độ và kích thước tạo thành 1 khung chữ nhật cho Player 1
            Rect hitbox1 = new Rect(Canvas.GetLeft(img1), Canvas.GetTop(img1), img1.Width, img1.Height);

            // Tạo khung chữ nhật cho Player 2
            Rect hitbox2 = new Rect(Canvas.GetLeft(img2), Canvas.GetTop(img2), img2.Width, img2.Height);

            // Lệnh IntersectsWith sẽ trả về true nếu 2 khung chạm nhau
            return hitbox1.IntersectsWith(hitbox2);
        }
    }
}