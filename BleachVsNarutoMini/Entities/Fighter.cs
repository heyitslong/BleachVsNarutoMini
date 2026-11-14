using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BleachVsNarutoMini.Engine;
using InputManager = BleachVsNarutoMini.Engine.InputManager;

namespace BleachVsNarutoMini.Entities
{
    public enum FighterState { Idle, Run, Attack, Jump }

    public abstract class Fighter : GameObject
    {
        // Tất cả tính theo GIÂY và PIXEL/GIÂY
        private const double Gravity = 1800;
        private const double JumpPower = -600;
        private const int AttackDamage = 5;
        
        private const double RunFrameTime = 0.10;     // giây cho mỗi ảnh chạy
          // giây cho mỗi ảnh đánh

        private readonly FighterConfig _cfg;
        private readonly Key _left, _right, _jump, _attack;
        private readonly double _groundY;
        private readonly ScaleTransform _flip = new(1, 1);

        private double _vy, _timer;
        private int _frame;
        private int _lastHitFrame = -1;
        private bool _combo, _attackWasDown;
        private string _currentFile = "";

        public Image Sprite { get; }
        public FighterState State { get; private set; } = FighterState.Idle;
        public bool FacingRight { get; private set; }
        public int Hp { get; private set; } = 100;
        public bool IsDead => Hp <= 0;


        public double StageWidth { get; set; } = 800;

        private double BodyW => Width * _cfg.BodyWidthRatio;
        private double BodyX => X + (Width - BodyW) / 2;

        public Rect Bounds => new(BodyX, Y, BodyW, Height);

        private Rect AttackBox => FacingRight
            ? new Rect(BodyX + BodyW, Y, _cfg.AttackReach, Height)
            : new Rect(BodyX - _cfg.AttackReach, Y, _cfg.AttackReach, Height);

        protected Fighter(double x, FighterConfig cfg,
                          Key left, Key right, Key jump, Key attack)
            : base(x, cfg.GroundBottom - cfg.Height, cfg.Width, cfg.Height, 220)
        {
            _cfg = cfg;
            _groundY = Y;
            _left = left; _right = right; _jump = jump; _attack = attack;
            FacingRight = cfg.StartFacingRight;

            Sprite = new Image
            {
                Width = Width,
                Height = Height,
                RenderTransform = _flip,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            RenderOptions.SetBitmapScalingMode(Sprite, BitmapScalingMode.NearestNeighbor);
            SetImage(_cfg.Idle);
            Draw();
        }

        // dt = số giây từ khung hình trước
        public void UpdateLogic(double dt)
        {
            if (IsDead) return;

            bool attackDown = InputManager.IsKeyDown(_attack);
            bool attackPressed = attackDown && !_attackWasDown;
            _attackWasDown = attackDown;

            int dir = (InputManager.IsKeyDown(_right) ? 1 : 0)
                    - (InputManager.IsKeyDown(_left) ? 1 : 0);

            if (State == FighterState.Idle || State == FighterState.Run)
            {
                if (attackPressed) SetState(FighterState.Attack);
                else if (InputManager.IsKeyDown(_jump)) { _vy = JumpPower; SetState(FighterState.Jump); }
            }
            else if (State == FighterState.Attack && attackPressed && _cfg.ComboFrames > 0)
            {
                _combo = true;
            }

            if (State != FighterState.Attack && dir != 0)
            {
                X = Math.Clamp(X + dir * Speed * dt, 0, StageWidth - Width);
                FacingRight = dir > 0;
            }

            switch (State)
            {
                case FighterState.Idle:
                case FighterState.Run: UpdateGround(dir, dt); break;
                case FighterState.Attack: UpdateAttack(dt); break;
                case FighterState.Jump: UpdateJump(dt); break;
            }
            Draw();
        }

        public void TryHit(Fighter target)
        {
            if (State != FighterState.Attack || target.IsDead) return;

            bool hitFrame = _frame == 1 || (_combo && _frame == 4);
            if (!hitFrame || _lastHitFrame == _frame) return;

            if (AttackBox.IntersectsWith(target.Bounds))
            {
                target.TakeDamage(AttackDamage);
                _lastHitFrame = _frame;
            }
        }

        public void TakeDamage(int amount) => Hp = Math.Max(0, Hp - amount);

        // ---------------- phần nội bộ ----------------
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

        private void UpdateAttack(double dt)
        {
            int total = _combo ? _cfg.ComboFrames : _cfg.AttackFrames;
            SetImage($"{_cfg.AttackPrefix}{_frame + 1}.png");
            if (Advance(total, _cfg.AttackFrameTime, false, dt)) SetState(FighterState.Idle);
        }

        private void UpdateJump(double dt)
        {
            _vy += Gravity * dt;
            Y += _vy * dt;
            if (Y >= _groundY)
            {
                Y = _groundY; _vy = 0;
                SetState(FighterState.Idle);
                SetImage(_cfg.Idle);
                return;
            }
            double t = Math.Clamp((_vy - JumpPower) / (-2 * JumpPower), 0, 0.999);
            int f = (int)(t * _cfg.JumpFrames) + 1;
            SetImage($"{_cfg.JumpPrefix}{f}.png");
        }

        // Cộng dồn thời gian, đủ frameTime thì sang ảnh kế. Trả về true khi animation không lặp đã xong.
        private bool Advance(int count, double frameTime, bool loop, double dt)
        {
            _timer += dt;
            while (_timer >= frameTime)
            {
                _timer -= frameTime;
                _frame++;
                if (_frame >= count)
                {
                    if (loop) _frame = 0;
                    else { _frame = count - 1; return true; }
                }
            }
            return false;
        }

        private void SetState(FighterState s)
        {
            State = s; _frame = 0; _timer = 0; _lastHitFrame = -1; _combo = false;
        }

        private void SetImage(string file)
        {
            if (file == _currentFile) return;      // ảnh không đổi thì khỏi gán lại
            _currentFile = file;
            Sprite.Source = ImageCache.Get(file);
        }

        private void Draw()
        {
            Canvas.SetLeft(Sprite, X);
            Canvas.SetTop(Sprite, Y);
            _flip.ScaleX = FacingRight ? 1 : -1;
        }
    }
}