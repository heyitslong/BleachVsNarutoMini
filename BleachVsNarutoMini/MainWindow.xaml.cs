using System.Windows;
using System.Windows.Input;
using System.Collections.Generic; // Thêm dòng này để xài List
using BleachVsNarutoMini.Engine;
using BleachVsNarutoMini.Entities; // Thêm dòng này để gọi được Naruto, Sasuke, Projectile

// Đặt bí danh để tránh đụng độ với InputManager của WPF
using MyInputManager = BleachVsNarutoMini.Engine.InputManager;

namespace BleachVsNarutoMini
{
    public partial class MainWindow : Window
    {
        private GameLoop _gameLoop;

        // 1. Khai báo 2 nhân vật và danh sách phi tiêu
        private Naruto player1;
        private Sasuke player2;
        private List<Projectile> _projectiles = new List<Projectile>();
        private List<Rect> platforms = new List<Rect>(); // Danh sách bục đứng (hiện tại để rỗng)

        public MainWindow()
        {
            InitializeComponent();

            // 2. Khởi tạo nhân vật
            player1 = new Naruto();
            player2 = new Sasuke();

            // Truyền nền tảng cho nhân vật (để check va chạm rơi rớt)
            player1.Platforms = platforms;
            player2.Platforms = platforms;

            // 3. Đăng ký bắt sự kiện khi nhân vật ném phi tiêu
            player1.OnSpawnProjectile += HandleSpawnProjectile;
            player2.OnSpawnProjectile += HandleSpawnProjectile;

            // 4. QUAN TRỌNG: Vẽ 2 nhân vật lên GameCanvas
            GameCanvas.Children.Add(player1.Sprite);
            GameCanvas.Children.Add(player2.Sprite);

            // Khởi tạo Game Loop
            _gameLoop = new GameLoop();
            _gameLoop.Update += GameUpdate;
            _gameLoop.Start();
        }

        // Hàm này tự động được gọi mỗi khi Naruto bấm U hoặc Sasuke bấm C
        private void HandleSpawnProjectile(Projectile proj)
        {
            _projectiles.Add(proj);
            GameCanvas.Children.Add(proj.Sprite); // Vẽ phi tiêu lên màn hình
        }

        // Truyền sự kiện bấm phím vào InputManager
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            MyInputManager.KeyDown(e.Key);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            MyInputManager.KeyUp(e.Key);
        }

        // Hàm chạy liên tục theo nhịp Game Loop (~60 FPS)
        private void GameUpdate()
        {
            // Tạm thời bỏ qua GameStateManager nếu bạn muốn test ngay lập tức, 
            // hoặc cứ để lại nếu bạn đã code menu xong xuôi.
            if (GameStateManager.CurrentState == GameState.Playing)
            {
                double dt = 0.016; // Giả lập thời gian trôi qua mỗi frame (1/60 giây)

                // 5. Cập nhật logic di chuyển, nhảy, rớt của 2 nhân vật
                player1.UpdateLogic(dt);
                player2.UpdateLogic(dt);

                // 6. Kiểm tra xem 2 đứa có đấm trúng nhau không
                player1.TryHit(player2);
                player2.TryHit(player1);

                // 7. Cập nhật phi tiêu bay
                for (int i = _projectiles.Count - 1; i >= 0; i--)
                {
                    var p = _projectiles[i];

                    // Mục tiêu là đứa bị ném (ai ném thì trúng đứa còn lại)
                    Fighter target = (p.Owner == player1) ? (Fighter)player2 : (Fighter)player1;

                    // Cho phi tiêu bay
                    p.Update(dt, target, platforms, 1000); // 1000 là StageWidth (chiều rộng màn hình)

                    // Nếu phi tiêu đụng mục tiêu hoặc bay ra ngoài mép -> Dọn dẹp
                    if (!p.IsActive)
                    {
                        GameCanvas.Children.Remove(p.Sprite);
                        _projectiles.RemoveAt(i);
                    }
                }
            }
        }
    }
}