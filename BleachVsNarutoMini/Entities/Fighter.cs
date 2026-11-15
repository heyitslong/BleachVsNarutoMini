using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using InputManager = BleachVsNarutoMini.Engine.InputManager;

namespace BleachVsNarutoMini.Entities
{
    public enum FighterState { Idle, Run, Attack, Jump }

    public abstract class Fighter : GameObject
    {
        private const double Gravity = 1800;
        private const double JumpPower = -920;
        private const int AttackDamage = 5;
        private const double RunFrameTime = 0.10;

        private readonly FighterConfig _cfg;
        private readonly Key _left, _right, _jump, _attack;
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

        public double StageWidth { get; set; } = 1024;
        public double GroundY { get; set; } = 620;
        public IReadOnlyList<Rect> Platforms { get; set; } = Array.Empty<Rect>();

        // Hitbox thân thể thực tế của nhân vật (bỏ qua phần viền ảnh rỗng xung quanh)
        private double BodyW => Width * _cfg.BodyWidthRatio;
        private double BodyOffsetX => (Width - BodyW) / 2;
        private double BodyX => X + BodyOffsetX;
        private double BodyTop => Y + 18; // Bỏ phần khoảng trắng phía trên đầu trong khung ảnh
        private double FeetY => Y + Height - _cfg.FootPadding;
        private double BodyH => Math.Max(20, FeetY - BodyTop);

        public Rect Bounds => new(BodyX, BodyTop, BodyW, BodyH);

        private Rect AttackBox => FacingRight
            ? new Rect(BodyX + BodyW, BodyTop, _cfg.AttackReach, BodyH)
            : new Rect(BodyX - _cfg.AttackReach, BodyTop, _cfg.AttackReach, BodyH);

        protected Fighter(double x, FighterConfig cfg,
                          Key left, Key right, Key jump, Key attack)
            : base(x, cfg.GroundBottom - cfg.Height + cfg.FootPadding, cfg.Width, cfg.Height, 260)
        {
            _cfg = cfg;
            _left = left; _right = right; _jump = jump; _attack = attack;
            FacingRight = cfg.StartFacingRight;
            GroundY = cfg.GroundBottom;

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
                else if (InputManager.IsKeyDown(_jump))
                {
                    _vy = JumpPower;
                    SetState(FighterState.Jump);
                }
            }
            else if (State == FighterState.Attack && attackPressed && _cfg.ComboFrames > 0)
            {
                _combo = true;
            }

            // 1. DI CHUYỂN NGANG (TRỤC X) & CHẶN CẠNH TRÁI/PHẢI CỦA KHỐI BỤC
            if (State != FighterState.Attack && dir != 0)
            {
                double minX = 20 - BodyOffsetX;
                double maxX = StageWidth - 20 - BodyW - BodyOffsetX;
                X = Math.Clamp(X + dir * Speed * dt, minX, maxX);
                FacingRight = dir > 0;
                ResolveHorizontalCollisions(dir);
            }

            // Bước hụt ra khỏi mép bục -> tự động rơi xuống
            if (State != FighterState.Jump && !IsStandingOnSurface())
            {
                _vy = 0;
                SetState(FighterState.Jump);
            }

            // 2. CẬP NHẬT TRẠNG THÁI & DI CHUYỂN DỌC (TRỤC Y)
            switch (State)
            {
                case FighterState.Idle:
                case FighterState.Run: UpdateGround(dir, dt); break;
                case FighterState.Attack: UpdateAttack(dt); break;
                case FighterState.Jump: UpdateJump(dt); break;
            }
            Draw();
        }

        private void ResolveHorizontalCollisions(int dir)
        {
            Rect myBox = Bounds;

            foreach (var p in Platforms)
            {
                // Bỏ qua 4 tường viền bao quanh bản đồ (đã giới hạn bằng StageWidth/GroundY)
                if (p.Top <= 0 || p.Top >= GroundY || p.Left <= 0 || p.Right >= StageWidth) continue;

                // Kiểm tra giao nhau theo chiều dọc (thân nhân vật đang nằm ngang hàng với khối bục)
                bool overlapY = myBox.Bottom > p.Top + 2 && myBox.Top < p.Bottom - 2;
                if (!overlapY) continue;

                if (dir > 0 && myBox.Right > p.Left && myBox.Left < p.Left)
                {
                    // Đang đi sang phải -> đụng cạnh trái của khối -> ép sát mép trái
                    X = p.Left - BodyW - BodyOffsetX;
                    myBox = Bounds;
                }
                else if (dir < 0 && myBox.Left < p.Right && myBox.Right > p.Right)
                {
                    // Đang đi sang trái -> đụng cạnh phải của khối -> ép sát mép phải
                    X = p.Right - BodyOffsetX;
                    myBox = Bounds;
                }
            }
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

        private bool IsStandingOnSurface()
        {
            if (Math.Abs(FeetY - GroundY) <= 2) return true;

            foreach (var p in Platforms)
            {
                if (p.Top <= 0 || p.Top >= GroundY || p.Left <= 0 || p.Right >= StageWidth) continue;

                bool overlapX = (BodyX + BodyW) > p.Left + 2 && BodyX < p.Right - 2;
                bool onTop = Math.Abs(FeetY - p.Top) <= 3;
                if (overlapX && onTop) return true;
            }
            return false;
        }

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
            double prevFeetY = FeetY;
            double prevTopY = BodyTop;

            _vy += Gravity * dt;
            Y += _vy * dt;

            double currFeetY = FeetY;
            double currTopY = BodyTop;

            foreach (var p in Platforms)
            {
                if (p.Top <= 0 || p.Top >= GroundY || p.Left <= 0 || p.Right >= StageWidth) continue;

                bool overlapX = (BodyX + BodyW) > p.Left + 2 && BodyX < p.Right - 2;
                if (!overlapX) continue;

                if (_vy >= 0)
                {
                    // Đang rơi xuống -> đáp lên mặt trên khối (p.Top)
                    if (prevFeetY <= p.Top + 6 && currFeetY >= p.Top)
                    {
                        Y = p.Top - Height + _cfg.FootPadding;
                        _vy = 0;
                        SetState(FighterState.Idle);
                        SetImage(_cfg.Idle);
                        return;
                    }
                }
                else
                {
                    // Đang nhảy lên -> cụng đầu vào mặt đáy khối (p.Bottom) -> dội xuống
                    if (prevTopY >= p.Bottom - 6 && currTopY <= p.Bottom)
                    {
                        Y = p.Bottom - 18;
                        _vy = 0;
                        break;
                    }
                }
            }

            if (currFeetY >= GroundY)
            {
                Y = GroundY - Height + _cfg.FootPadding;
                _vy = 0;
                SetState(FighterState.Idle);
                SetImage(_cfg.Idle);
                return;
            }

            double t = Math.Clamp((_vy - JumpPower) / (-2 * JumpPower), 0, 0.999);
            int f = (int)(t * _cfg.JumpFrames) + 1;
            SetImage($"{_cfg.JumpPrefix}{f}.png");
        }

        private bool Advance(int count, double frameTime, bool loop, double dt)
        {
            _timer += dt;
            while (_timer >= frameTime)
            {
                _timer -= frameTime;
                _frame++;
                if (loop) _frame %= count;
                else if (_frame >= count) { _frame = count - 1; return true; }
            }
            return false;
        }

        private void SetState(FighterState s)
        {
            State = s; _frame = 0; _timer = 0; _lastHitFrame = -1; _combo = false;
        }

        private void SetImage(string file)
        {
            if (file == _currentFile) return;
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