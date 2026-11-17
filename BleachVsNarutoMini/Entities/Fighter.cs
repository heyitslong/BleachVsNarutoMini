using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using InputManager = BleachVsNarutoMini.Engine.InputManager;

namespace BleachVsNarutoMini.Entities
{
    public abstract class Fighter : GameObject
    {
        private const double Gravity = 1800;
        private const double JumpPower = -920;
        private const int AttackDamage = 5;
        private const double RunFrameTime = 0.10;

        private readonly FighterConfig _cfg;
        private readonly Key _left, _right, _jump, _attack, _block, _throw, _ult;
        private readonly ScaleTransform _flip = new(1, 1);

        private double _vy, _timer;
        private int _frame;
        private int _lastHitFrame = -1;
        private bool _combo, _attackWasDown, _hasThrown;
        private string _currentFile = "";

        public Image Sprite { get; }
        public FighterState State { get; private set; } = FighterState.Idle;
        public bool FacingRight { get; private set; }
        public int Hp { get; private set; } = 100;
        public bool IsDead => Hp <= 0;

        public double StageWidth { get; set; } = 1024;
        public double GroundY { get; set; } = 620;
        public IReadOnlyList<Rect> Platforms { get; set; } = Array.Empty<Rect>();
        public event Action<Projectile>? OnSpawnProjectile;

        private double BodyW => Width * _cfg.BodyWidthRatio;
        private double BodyOffsetX => (Width - BodyW) / 2;
        private double BodyX => X + BodyOffsetX;
        private double BodyTop => Y + 18;
        private double FeetY => Y + Height - _cfg.FootPadding;
        private double BodyH => Math.Max(20, FeetY - BodyTop);
        public Rect Bounds => new(BodyX, BodyTop, BodyW, BodyH);

        private Rect AttackBox => FacingRight
            ? new Rect(BodyX + BodyW, BodyTop, _cfg.AttackReach, BodyH)
            : new Rect(BodyX - _cfg.AttackReach, BodyTop, _cfg.AttackReach, BodyH);

        protected Fighter(double x, FighterConfig cfg,
                          Key left, Key right, Key jump, Key attack, Key block, Key throwSkill, Key ult)
            : base(x, cfg.GroundBottom - cfg.Height + cfg.FootPadding, cfg.Width, cfg.Height, 260)
        {
            _cfg = cfg;
            _left = left; _right = right; _jump = jump; _attack = attack;
            _block = block; _throw = throwSkill; _ult = ult;

            FacingRight = cfg.StartFacingRight;
            GroundY = cfg.GroundBottom;

            Sprite = new Image { Width = Width, Height = Height, RenderTransform = _flip, RenderTransformOrigin = new Point(0.5, 0.5) };
            RenderOptions.SetBitmapScalingMode(Sprite, BitmapScalingMode.NearestNeighbor);
            SetImage(_cfg.Idle); Draw();
        }

        public void UpdateLogic(double dt)
        {
            // Trạng thái chết: Chỉ cho rớt xuống đất và play animation chết
            if (State == FighterState.Dead) { UpdateDead(dt); Draw(); return; }

            bool attackDown = InputManager.IsKeyDown(_attack);
            bool attackPressed = attackDown && !_attackWasDown;
            _attackWasDown = attackDown;

            int dir = (InputManager.IsKeyDown(_right) ? 1 : 0) - (InputManager.IsKeyDown(_left) ? 1 : 0);

            // Bắt phím khi đang rảnh rỗi hoặc di chuyển
            if (State == FighterState.Idle || State == FighterState.Run)
            {
                if (InputManager.IsKeyDown(_block)) SetState(FighterState.Block);
                else if (InputManager.IsKeyDown(_ult)) SetState(FighterState.Ultimate);
                else if (InputManager.IsKeyDown(_throw)) SetState(FighterState.Throw);
                else if (attackPressed) SetState(FighterState.Attack);
                else if (InputManager.IsKeyDown(_jump)) { _vy = JumpPower; SetState(FighterState.Jump); }
            }
            // Kích hoạt Combo nếu đang ở đòn thường
            else if (State == FighterState.Attack && attackPressed) _combo = true;

            // Di chuyển (Cho phép chạy trên đất VÀ lướt trên không trung khi nhảy)
            bool canMoveX = State == FighterState.Idle || State == FighterState.Run || State == FighterState.Jump;
            if (canMoveX && dir != 0)
            {
                X = Math.Clamp(X + dir * Speed * dt, 20 - BodyOffsetX, StageWidth - 20 - BodyW - BodyOffsetX);
                FacingRight = dir > 0;
            }

            if (State != FighterState.Jump && !IsStandingOnSurface()) { _vy = 0; SetState(FighterState.Jump); }

            // Xử lý Frame Animation theo trạng thái
            switch (State)
            {
                case FighterState.Idle:
                case FighterState.Run: UpdateGround(dir, dt); break;
                case FighterState.Jump: UpdateJump(dt); break;
                case FighterState.Attack: UpdateAttack(dt); break;
                case FighterState.Throw: UpdateThrow(dt); break;
                case FighterState.Ultimate: UpdateUltimate(dt); break;
                case FighterState.Block: UpdateBlock(); break;
                case FighterState.Hurt: UpdateHurt(dt); break;
            }
            Draw();
        }

        public void TryHit(Fighter target)
        {
            if (State != FighterState.Attack || target.IsDead) return;
            bool hitFrame = _frame == 1 || (_combo && _frame >= 5); // Tùy chỉnh nhịp gây dame
            if (!hitFrame || _lastHitFrame == _frame) return;

            if (AttackBox.IntersectsWith(target.Bounds))
            {
                target.TakeDamage(AttackDamage);
                _lastHitFrame = _frame;
            }
        }

        public void TakeDamage(int amount)
        {
            if (State == FighterState.Block || IsDead) return; // Nếu đang đỡ thì không mất máu
            Hp = Math.Max(0, Hp - amount);
            if (Hp == 0) SetState(FighterState.Dead);
            else SetState(FighterState.Hurt);
        }

        private bool IsStandingOnSurface() => Math.Abs(FeetY - GroundY) <= 2;

        private void UpdateGround(int dir, double dt)
        {
            var want = dir != 0 ? FighterState.Run : FighterState.Idle;
            if (want != State) SetState(want);

            if (State == FighterState.Run)
            {
                SetImage($"{_cfg.RunPrefix}{_cfg.RunStart + _frame}.png");
                Advance(_cfg.RunFrames, RunFrameTime, true, dt);
            }
            else SetImage(_cfg.Idle);
        }

        private void UpdateJump(double dt)
        {
            _vy += Gravity * dt; Y += _vy * dt;
            if (FeetY >= GroundY) { Y = GroundY - Height + _cfg.FootPadding; _vy = 0; SetState(FighterState.Idle); return; }

            double t = Math.Clamp((_vy - JumpPower) / (-2 * JumpPower), 0, 0.999);
            SetImage($"{_cfg.JumpPrefix}{(int)(t * _cfg.JumpFrames) + 1}.png");
        }

        private void UpdateAttack(double dt)
        {
            int total = _combo ? _cfg.AttackFrames : _cfg.ComboSplitFrame; // Đánh tới 4, nếu có combo thì tới 12
            SetImage($"{_cfg.AttackPrefix}{_frame + 1}.png");
            if (Advance(total, _cfg.AttackFrameTime, false, dt)) SetState(FighterState.Idle);
        }

        private void UpdateThrow(double dt)
        {
            SetImage($"{_cfg.ThrowPrefix}{_frame + 1}.png");

            // Nếu đã tới frame ném (Naruto là 4, Sasuke là 2) và CHƯA NÉM
            if (_frame >= _cfg.ThrowFrames - 1 && !_hasThrown)
            {
                _hasThrown = true; // Bật cờ lên để đánh dấu là đã ném rồi

                double spawnX = FacingRight ? (BodyX + BodyW) : (BodyX - 30);

                // Truyền đúng "sskanhphitieu". Thằng Projectile.cs sẽ tự động lấy sskanhphitieu + 1 đến 5
                var proj = new Projectile(this, "sskanhphitieu", spawnX, BodyTop + 20, 30, 30, 600, 10, FacingRight);
                OnSpawnProjectile?.Invoke(proj);
            }

            if (Advance(_cfg.ThrowFrames, 0.08, false, dt)) SetState(FighterState.Idle);
        }

        private void UpdateUltimate(double dt)
        {
            SetImage($"{_cfg.UltimatePrefix}{_frame + 1}.png");
            if (Advance(_cfg.UltimateFrames, 0.08, false, dt)) SetState(FighterState.Idle);
        }

        private void UpdateBlock()
        {
            SetImage(_cfg.BlockImage);
            if (!InputManager.IsKeyDown(_block)) SetState(FighterState.Idle); // Nhả phím -> Trở về đứng yên
        }

        private void UpdateHurt(double dt)
        {
            SetImage($"{_cfg.HurtPrefix}1.png"); // Bị thương khi còn HP chỉ hiện 1 frame
            if (Advance(1, 0.3, false, dt)) SetState(FighterState.Idle); // Giật 0.3 giây rồi hồi phục
        }

        private void UpdateDead(double dt)
        {
            if (FeetY < GroundY) { _vy += Gravity * dt; Y += _vy * dt; } // Rớt xuống đất nếu đang nhảy
            else Y = GroundY - Height + _cfg.FootPadding;

            SetImage($"{_cfg.HurtPrefix}{_frame + 1}.png");
            Advance(_cfg.DeadFrames, 0.15, false, dt); // Lăn ra chết từ 1 đến 6 rồi dừng luôn
        }

        private bool Advance(int count, double time, bool loop, double dt)
        {
            _timer += dt;
            while (_timer >= time) { _timer -= time; _frame++; if (loop) _frame %= count; else if (_frame >= count) { _frame = count - 1; return true; } }
            return false;
        }

        private void SetState(FighterState s)
        {
            State = s; _frame = 0; _timer = 0; _lastHitFrame = -1; _combo = false;
            _hasThrown = false; // Thêm dòng này vào đây
        }
        private void SetImage(string f) { if (f == _currentFile) return; _currentFile = f; Sprite.Source = ImageCache.Get(f); }
        private void Draw() { Canvas.SetLeft(Sprite, X); Canvas.SetTop(Sprite, Y); _flip.ScaleX = FacingRight ? 1 : -1; }
    }
}