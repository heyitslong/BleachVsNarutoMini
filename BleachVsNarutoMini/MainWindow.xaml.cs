using System.Windows;
using System.Windows.Input;
using BleachVsNarutoMini.Engine;

// Đặt bí danh để tránh đụng độ với InputManager của WPF
using MyInputManager = BleachVsNarutoMini.Engine.InputManager;

namespace BleachVsNarutoMini
{
    public partial class MainWindow : Window
    {
        private GameLoop _gameLoop;

        public MainWindow()
        {
            InitializeComponent();

            // Khởi tạo Game Loop
            _gameLoop = new GameLoop();
            _gameLoop.Update += GameUpdate;
            _gameLoop.Start();
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
            if (GameStateManager.CurrentState == GameState.Playing)
            {
                // Nơi Người 2 sẽ lắp code Player.Update() và Bullet.Update() vào đây
            }
        }
    }
}