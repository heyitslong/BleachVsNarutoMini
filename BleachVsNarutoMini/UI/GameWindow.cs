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
        private DispatcherTimer _gameLoopTimer;
        // ==============================================================
        private double p1Health = 100;
        private double p2Health = 100;

        public GameWindow()
        {
            DemoMode = false;
            SelectedCharacter = "Knight";
            SelectedMap = "Arena1";

            Title = "PVP Game";
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

            // === KHỞI TẠO TIMER RIÊNG CHO NARUTO & SASUKE ===
            // Tốc độ 50ms mượt mà như code cũ của bạn
            _gameLoopTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _gameLoopTimer.Tick += GameLoopTimer_Tick;
            // ================================================

            ShowMenu();
        }

        // === BỘ NÃO HOẠT ĐỘNG CỦA NHÂN VẬT ===
        private void GameLoopTimer_Tick(object sender, EventArgs e)
        {
            if (player1 != null) player1.UpdateLogic();
            if (player2 != null) player2.UpdateLogic();

            // NẾU 2 NHÂN VẬT ĐANG TỒN TẠI VÀ CHẠM VÀO NHAU:
            if (player1 != null && player2 != null && CheckCollision(player1.Sprite, player2.Sprite))
            {
                // Naruto đang tung chiêu
                if (player1.IsAttacking)
                {
                    p2Health -= 2; // Trừ máu Sasuke
                }

                // Sasuke đang tung chiêu
                if (player2.IsAttacking)
                {
                    p1Health -= 2; // Trừ máu Naruto
                }

                // Gửi dữ liệu lên thanh máu của bạn bè để nó vẽ lại UI
                UpdateHud(p1Health, p2Health, _maxHealth);

                // Phân định thắng thua
                if (p1Health <= 0)
                    ShowGameOver("PLAYER 2 (SASUKE)");
                else if (p2Health <= 0)
                    ShowGameOver("PLAYER 1 (NARUTO)");
            }
        }
        // =====================================

        public void ShowMenu()
        {
            StopDemo();
            _gameLoopTimer.Stop(); // Tắt nhân vật khi ra menu
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
            player1 = new Entities.Naruto();
            player2 = new Entities.Sasuke();
            GameCanvas.Children.Add(player1.Sprite);
            GameCanvas.Children.Add(player2.Sprite);
            _gameLoopTimer.Start(); // Bắt đầu cho phép di chuyển
            // ===============================

            // Code của bạn bè vẫn chạy bình thường không sứt mẻ 1 miếng
            if (DemoMode) StartDemo();
        }

        public void ShowGameOver(string winnerName)
        {
            StopDemo();
            _gameLoopTimer.Stop(); // Dừng nhân vật khi có người thắng

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
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (player1 != null) player1.HandleKeyDown(e.Key);
            if (player2 != null) player2.HandleKeyDown(e.Key);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (player1 != null) player1.HandleKeyUp(e.Key);
            if (player2 != null) player2.HandleKeyUp(e.Key);
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