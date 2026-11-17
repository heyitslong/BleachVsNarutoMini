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
    //  Chú thích: [MỚI] = phần thêm vào, [SỬA] = phần sửa nhẹ so với bản cũ
    public class GameWindow : Window
    {
        public Canvas MapLayer { get; private set; }
        public Canvas GameCanvas { get; private set; }
        public HudView Hud { get; private set; }
        public MapData CurrentMap { get; private set; }

        // [SỬA] Trước đây là SelectedCharacter, giờ mỗi người chơi chọn nhân vật riêng
        public string SelectedP1 { get; private set; }
        public string SelectedP2 { get; private set; }
        public string SelectedMap { get; private set; }

        // [MỚI] Thời gian một trận (giây)
        public int MatchSeconds { get; set; }

        // [SỬA] Thêm tham số nhân vật của P2: (nhân vật P1, nhân vật P2, đấu trường)
        public event Action<string, string, string> GameStarted;

        public bool DemoMode { get; set; }

        private readonly ContentControl _screenHost;
        private DispatcherTimer _demoTimer;
        private double _maxHealth = 100;

        // [SỬA] Đổi kiểu sang Fighter để P1/P2 có thể là Naruto hoặc Sasuke tùy lựa chọn
        private Entities.Fighter player1;
        private Entities.Fighter player2;
        private readonly List<Entities.Projectile> _projectiles = new();

        private TimeSpan _lastRender;
        private bool _running;
        private int _shownHp1 = -1, _shownHp2 = -1;
        private double p1Health = 100;
        private double p2Health = 100;

        // [MỚI] Đồng hồ đếm ngược
        private double _timeLeft;
        private int _shownTime = -1;

        public GameWindow()
        {
            DemoMode = false;
            MatchSeconds = 60;              // [MỚI]
            SelectedP1 = "Naruto";          // [SỬA]
            SelectedP2 = "Sasuke";          // [SỬA]
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

        // HOẠT ĐỘNG CỦA NHÂN VẬT
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
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                var proj = _projectiles[i];
                var target = proj.Owner == player1 ? (Entities.Fighter)player2 : player1;
                proj.Update(dt, target, CurrentMap.Obstacles, MapLibrary.Width);

                if (!proj.IsActive)
                {
                    GameCanvas.Children.Remove(proj.Sprite);
                    _projectiles.RemoveAt(i);
                }
            }

            // Chỉ cập nhật thanh máu khi máu thật sự đổi
            if (player1.Hp != _shownHp1 || player2.Hp != _shownHp2)
            {
                _shownHp1 = player1.Hp;
                _shownHp2 = player2.Hp;
                UpdateHud(player1.Hp, player2.Hp, _maxHealth);
            }

            // [MỚI] Thanh chakra. TODO (khi N2 thêm Chakra vào Fighter): bỏ comment dòng dưới
            // UpdateChakra(player1.Chakra, player2.Chakra, player1.MaxChakra);

            // [MỚI] Đồng hồ đếm ngược (tính theo dt nên dừng game là đồng hồ dừng theo)
            _timeLeft -= dt;
            int secs = (int)Math.Ceiling(Math.Max(0, _timeLeft));
            if (secs != _shownTime)
            {
                _shownTime = secs;
                Hud.SetTime(secs);
            }

            // [SỬA] Tên người thắng lấy theo nhân vật đã chọn; truyền null nghĩa là HÒA
            if (player1.IsDead && player2.IsDead) ShowGameOver(null);
            else if (player1.IsDead) ShowGameOver(PlayerLabel(2, SelectedP2));
            else if (player2.IsDead) ShowGameOver(PlayerLabel(1, SelectedP1));
            // [MỚI] Hết giờ: ai còn nhiều máu hơn thì thắng, bằng nhau thì hòa
            else if (_timeLeft <= 0)
            {
                if (player1.Hp > player2.Hp) ShowGameOver(PlayerLabel(1, SelectedP1));
                else if (player2.Hp > player1.Hp) ShowGameOver(PlayerLabel(2, SelectedP2));
                else ShowGameOver(null);
            }
        }

        public void ShowMenu()
        {
            StopDemo();
            StopLoop(); // Tắt nhân vật khi ra menu
            Hud.Visibility = Visibility.Collapsed;
            GameCanvas.Children.Clear();
            _projectiles.Clear();

            // [SỬA] Menu giờ có 2 cột chọn nhân vật cho P1 và P2
            var menu = new MainMenuView(SelectedP1, SelectedP2, SelectedMap);
            menu.StartClicked += delegate (string p1, string p2, string map)
            {
                SelectedP1 = p1;
                SelectedP2 = p2;
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
            _projectiles.Clear();

            CurrentMap = MapLibrary.Get(SelectedMap);
            MapRenderer.Draw(MapLayer, CurrentMap);

            p1Health = _maxHealth;
            p2Health = _maxHealth;

            Hud.Visibility = Visibility.Visible;
            Hud.SetNames(PlayerName(1, SelectedP1), PlayerName(2, SelectedP2));   // [SỬA] hiện tên nhân vật
            UpdateHud(p1Health, p2Health, _maxHealth);
            UpdateChakra(100, 100, 100);                                          // [MỚI]

            // [MỚI] Đặt lại đồng hồ và HUD cho trận mới
            _shownHp1 = -1;
            _shownHp2 = -1;
            _timeLeft = MatchSeconds;
            _shownTime = MatchSeconds;
            Hud.SetTime(MatchSeconds);

            if (GameStarted != null) GameStarted(SelectedP1, SelectedP2, SelectedMap);   // [SỬA]

            // === ĐƯA NHÂN VẬT LÊN VÕ ĐÀI ===
            // [SỬA] Tạo nhân vật theo lựa chọn (xem hàm CreateFighter bên dưới)
            player1 = CreateFighter(SelectedP1);
            player2 = CreateFighter(SelectedP2);
            GameCanvas.Children.Add(player1.Sprite);
            GameCanvas.Children.Add(player2.Sprite);
            StartLoop(); // Bắt đầu cho phép di chuyển
            if (DemoMode) StartDemo();
        }

        // Tạo nhân vật theo id chọn ở menu. Việc gắn OnSpawnProjectile đặt luôn ở đây.
        // Muốn thêm nhân vật mới: thêm một nhánh ở đây và một dòng trong MainMenuView.Characters.
        private Entities.Fighter CreateFighter(string id)
        {
            if (id == "Sasuke")
            {
                var s = new Entities.Sasuke
                {
                    StageWidth = MapLibrary.Width,
                    Platforms = CurrentMap.Obstacles
                };
                s.OnSpawnProjectile += AddProjectile;
                return s;
            }

            var n = new Entities.Naruto
            {
                StageWidth = MapLibrary.Width,
                Platforms = CurrentMap.Obstacles
            };
            n.OnSpawnProjectile += AddProjectile;
            return n;
        }

        // "PLAYER 1 - Naruto"
        private static string PlayerName(int index, string characterId)
        {
            return "PLAYER " + index + " - " + MainMenuView.DisplayName(characterId);
        }

        // "PLAYER 1 (NARUTO)" 
        private static string PlayerLabel(int index, string characterId)
        {
            return "PLAYER " + index + " (" + MainMenuView.DisplayName(characterId).ToUpper() + ")";
        }

        private void AddProjectile(Entities.Projectile proj)
        {
            _projectiles.Add(proj);
            GameCanvas.Children.Add(proj.Sprite);
        }

        // [SỬA] winnerName = null nghĩa là HÒA
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

        // Cập nhật thanh chakra
        public void UpdateChakra(double p1Chakra, double p2Chakra, double maxChakra = 100)
        {
            Hud.SetChakra(p1Chakra, p2Chakra, maxChakra);
        }
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

        // BẮT SỰ KIỆN BÀN PHÍM TỪ BÊN NGOÀI 
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
            
            Rect hitbox1 = new Rect(Canvas.GetLeft(img1), Canvas.GetTop(img1), img1.Width, img1.Height);

            Rect hitbox2 = new Rect(Canvas.GetLeft(img2), Canvas.GetTop(img2), img2.Width, img2.Height);

            return hitbox1.IntersectsWith(hitbox2);
        }
    }
}