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
        private const double RunFrameTime = 0.10;

        // [Phát thêm] Thông số kỹ năng Lướt (Dash)
        private const double DashSpeed = 1000;      // Tốc độ lướt (pixel/giây)
        private const double DashDistance = 200;    // [Phát thêm] Độ dài quãng đường lướt (pixel) - tăng/giảm số này để chỉnh độ dài
        private const double DashCooldown = 0;   // Thời gian chờ giữa 2 lần lướt để chống nhấn liên tục

        // Mức tiêu hao và tốc độ hồi Chakra
        private const double JumpChakraCost = 10;
        private const double DashChakraCost = 10;  // [Phát thêm] Lướt tốn 10 chakra
        private const double ThrowChakraCost = 15;
        private const double UltChakraCost = 70;
        private const double ChakraRegenPerSec = 8;

        private readonly FighterConfig _cfg;
        private Key _left, _right, _jump, _attack, _block, _throw, _ult, _dash; // [Phát thêm] Thêm phím _dash, [Nghĩa sửa] bỏ readonly để gán phím lại được
        private readonly ScaleTransform _flip = new(1, 1);

        private double _vy, _timer;
        private double _dashedDistance; // [Phát thêm] Quãng đường đã lướt trong lượt hiện tại
        private double _dashCooldownTimer;  // [Phát thêm] Bộ đếm thời gian hồi chiêu lướt
        private double _ultDashedDistance; // [Phát thêm] Quãng đường đã lướt trong lượt tung chiêu cuối hiện tại
        private double _hurtDuration = 0.3; // [Phát thêm] Thời gian bị choáng khi trúng đòn (mặc định 0.3s)
        private int _frame;
        private int _lastHitFrame = -1;
        private bool _combo, _attackWasDown, _jumpWasDown, _throwWasDown, _ultWasDown, _dashWasDown, _hasThrown, _ultHasHit; // [Phát thêm] Thêm _dashWasDown chặn giữ phím lướt liên tục
        private string _currentFile = "";

        public Image Sprite { get; }
        public FighterState State { get; private set; } = FighterState.Idle;
        public bool FacingRight { get; private set; }
        public int Hp { get; private set; } = 300;
        public double MaxChakra { get; } = 200;
        public double Chakra { get; private set; } = 200;
        public bool IsDead => Hp <= 0;

        public double StageWidth { get; set; } = 1024;
        public double GroundY { get; set; } = 620;
        public IReadOnlyList<Rect> Platforms { get; set; } = Array.Empty<Rect>();
        public event Action<Projectile>? OnSpawnProjectile;
        public event Action? OnNotEnoughChakra; // [Phát thêm] Sự kiện báo hiệu khi bấm chiêu lúc không đủ Chakra

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

        // [Phát thêm] Vùng gây sát thương sát trước mặt khi áp sát chiêu cuối (tính từ mép thân nhân vật)
        private Rect UltimateBox => FacingRight
            ? new Rect(BodyX + BodyW, BodyTop - 10, _cfg.UltimateReach, BodyH + 10)
            : new Rect(BodyX - _cfg.UltimateReach, BodyTop - 10, _cfg.UltimateReach, BodyH + 10);

        // [Phát thêm] Bổ sung tham số Key dash vào constructor
        protected Fighter(double x, FighterConfig cfg,
                          Key left, Key right, Key jump, Key attack, Key block, Key throwSkill, Key ult, Key dash)
            : base(x, cfg.GroundBottom - cfg.Height + cfg.FootPadding, cfg.Width, cfg.Height, 260)
        {
            _cfg = cfg;
            _left = left; _right = right; _jump = jump; _attack = attack;
            _block = block; _throw = throwSkill; _ult = ult; _dash = dash; // [Phát thêm] Gán phím lướt

            FacingRight = cfg.StartFacingRight;
            GroundY = cfg.GroundBottom;

            Sprite = new Image { Width = Width, Height = Height, RenderTransform = _flip, RenderTransformOrigin = new Point(0.5, 0.5) };
            RenderOptions.SetBitmapScalingMode(Sprite, BitmapScalingMode.NearestNeighbor);
            SetImage(_cfg.Idle); Draw();

        }
        //[Nghĩa thêm] thêm Assign Player để chọn được nhân vật, bỏ mặc định Nar trái Sa phải
        public void AssignPlayer(int playerIndex, double spawnX)
        {
            if (playerIndex == 1)
            {
                _left = Key.A; _right = Key.D; _jump = Key.K; _attack = Key.J;
                _block = Key.S; _throw = Key.U; _ult = Key.I; _dash = Key.L;
            }
            else
            {
                _left = Key.Left; _right = Key.Right; _jump = Key.D2; _attack = Key.D1;
                _block = Key.Down; _throw = Key.D4; _ult = Key.D5; _dash = Key.D3;
            }

            X = spawnX;
            FacingRight = playerIndex == 1;
            Draw();
        }
        public void UpdateLogic(double dt)
        {
            if (State == FighterState.Dead) { UpdateDead(dt); Draw(); return; }

            if (Chakra < MaxChakra && ChakraRegenPerSec > 0)
                Chakra = Math.Min(MaxChakra, Chakra + ChakraRegenPerSec * dt);

            // [Phát thêm] Giảm thời gian hồi chiêu của kỹ năng Lướt
            if (_dashCooldownTimer > 0)
                _dashCooldownTimer = Math.Max(0, _dashCooldownTimer - dt);

            bool attackDown = InputManager.IsKeyDown(_attack);
            bool attackPressed = attackDown && !_attackWasDown;
            _attackWasDown = attackDown;

            bool jumpDown = InputManager.IsKeyDown(_jump);
            bool jumpPressed = jumpDown && !_jumpWasDown;
            _jumpWasDown = jumpDown;

            // [Phát thêm] Bắt nhịp nhấn từng lần đối với phím ném phi tiêu, chiêu cuối và lướt
            bool throwDown = InputManager.IsKeyDown(_throw);
            bool throwPressed = throwDown && !_throwWasDown;
            _throwWasDown = throwDown;

            bool ultDown = InputManager.IsKeyDown(_ult);
            bool ultPressed = ultDown && !_ultWasDown;
            _ultWasDown = ultDown;

            bool dashDown = InputManager.IsKeyDown(_dash);
            bool dashPressed = dashDown && !_dashWasDown;
            _dashWasDown = dashDown;

            int dir = (InputManager.IsKeyDown(_right) ? 1 : 0) - (InputManager.IsKeyDown(_left) ? 1 : 0);

            if (State == FighterState.Idle || State == FighterState.Run)
            {
                if (InputManager.IsKeyDown(_block)) SetState(FighterState.Block);
                else if (InputManager.IsKeyDown(_ult))
                {
                    if (TryUseChakra(UltChakraCost)) SetState(FighterState.Ultimate);
                    else if (ultPressed) OnNotEnoughChakra?.Invoke(); // [Phát thêm] Báo đỏ + rung thanh Chakra khi thiếu mana dùng chiêu cuối
                }
                else if (InputManager.IsKeyDown(_throw))
                {
                    if (TryUseChakra(ThrowChakraCost)) SetState(FighterState.Throw);
                    else if (throwPressed) OnNotEnoughChakra?.Invoke(); // [Phát thêm] Báo đỏ + rung thanh Chakra khi thiếu mana ném phi tiêu
                }
                // [Phát thêm] Kích hoạt Lướt khi vừa bấm phím, đã hết hồi chiêu và đủ 10 chakra
                else if (dashPressed && _dashCooldownTimer <= 0)
                {
                    if (TryUseChakra(DashChakraCost))
                    {
                        if (dir != 0) FacingRight = dir > 0;
                        _dashCooldownTimer = DashCooldown;
                        SetState(FighterState.Dash);
                    }
                    else OnNotEnoughChakra?.Invoke();
                }
                else if (attackPressed) SetState(FighterState.Attack);
                else if (jumpPressed)
                {
                    if (TryUseChakra(JumpChakraCost)) { _vy = JumpPower; SetState(FighterState.Jump); }
                    else OnNotEnoughChakra?.Invoke(); // [Phát thêm] Báo đỏ + rung thanh Chakra khi thiếu mana nhảy
                }
            }
            // [Phát thêm] Cho phép lướt trên không khi đang nhảy (vẫn tuân thủ hồi chiêu và tốn 10 chakra)
            else if (State == FighterState.Jump && dashPressed && _dashCooldownTimer <= 0)
            {
                if (TryUseChakra(DashChakraCost))
                {
                    if (dir != 0) FacingRight = dir > 0;
                    _dashCooldownTimer = DashCooldown;
                    SetState(FighterState.Dash);
                }
                else OnNotEnoughChakra?.Invoke();
            }
            else if (State == FighterState.Attack && attackPressed) _combo = true;
            // [Phát thêm] Báo thiếu Chakra nếu người chơi bấm chiêu trong lúc đang thực hiện động tác khác
            else if ((ultPressed && Chakra < UltChakraCost) ||
                     (throwPressed && Chakra < ThrowChakraCost) ||
                     (jumpPressed && Chakra < JumpChakraCost) ||
                     (dashPressed && _dashCooldownTimer <= 0 && Chakra < DashChakraCost))
            {
                OnNotEnoughChakra?.Invoke();
            }

            bool canMoveX = State == FighterState.Idle || State == FighterState.Run || State == FighterState.Jump;
            if (canMoveX && dir != 0)
            {
                X = Math.Clamp(X + dir * Speed * dt, 20 - BodyOffsetX, StageWidth - 20 - BodyW - BodyOffsetX);
                FacingRight = dir > 0;
            }

            if (State != FighterState.Jump && State != FighterState.Dash && !IsStandingOnSurface()) { _vy = 0; SetState(FighterState.Jump); }

            switch (State)
            {
                case FighterState.Idle:
                case FighterState.Run: UpdateGround(dir, dt); break;
                case FighterState.Jump: UpdateJump(dt); break;
                case FighterState.Dash: UpdateDash(dt); break; // [Phát thêm] Cập nhật trạng thái lướt
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
            if (target.IsDead) return;

            // Đòn đánh thường và combo
            if (State == FighterState.Attack)
            {
                bool hitFrame = _frame == 1 || (_combo && _frame >= 5);
                if (!hitFrame || _lastHitFrame == _frame) return;

                if (AttackBox.IntersectsWith(target.Bounds))
                {
                    target.TakeDamage(_cfg.AttackDamage); // [Phát thêm] Lấy dame đánh thường riêng của từng nhân vật
                    _lastHitFrame = _frame;
                }
            }
            // [Phát thêm] Gây sát thương chiêu cuối sau khi gồng xong và lướt áp sát trúng đối phương
            else if (State == FighterState.Ultimate)
            {
                int chargeFrames = _cfg.UltimateChargeFrames > 0 ? _cfg.UltimateChargeFrames : _cfg.UltimateFrames / 2;
                if (_frame < chargeFrames || _ultHasHit) return;

                if (UltimateBox.IntersectsWith(target.Bounds))
                {
                    // [Phát thêm] Giữ đối phương choáng cho đến khi người tung chiêu hoàn tất toàn bộ hoạt ảnh chiêu cuối
                    double remainingUltTime = Math.Max(0.4, (_cfg.UltimateFrames - _frame) * 0.08 + 0.1);
                    target.TakeDamage(_cfg.UltimateDamage, remainingUltTime);
                    _ultHasHit = true; // [Phát thêm] Đánh dấu đã trúng để dừng lướt ngay trước mặt đối phương và không gây dame lặp lại
                }
            }
        }

        // [Phát thêm] Bổ sung tham số stunDuration (mặc định 0.3s) để chiêu cuối làm choáng lâu hơn đòn thường
        public void TakeDamage(int amount, double stunDuration = 0.3)
        {
            if (State == FighterState.Block || IsDead) return;
            Hp = Math.Max(0, Hp - amount);
            if (Hp == 0) SetState(FighterState.Dead);
            else
            {
                SetState(FighterState.Hurt);
                _hurtDuration = stunDuration; // [Phát thêm] Gán thời gian choáng tương ứng với chiêu thức vừa trúng
            }
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

        // [Phát thêm] Xử lý lướt nhanh về phía trước theo đúng quãng đường DashDistance
        private void UpdateDash(double dt)
        {
            double dashDir = FacingRight ? 1 : -1;
            double step = Math.Min(DashSpeed * dt, DashDistance - _dashedDistance); // [Phát thêm] Không lướt vượt quá DashDistance
            X = Math.Clamp(X + dashDir * step, 20 - BodyOffsetX, StageWidth - 20 - BodyW - BodyOffsetX);
            _dashedDistance += step; // [Phát thêm] Cộng dồn quãng đường đã lướt

            SetImage($"{_cfg.RunPrefix}{_cfg.RunStart + _frame}.png");
            Advance(_cfg.RunFrames, 0.04, true, dt);

            if (_dashedDistance >= DashDistance)
            {
                if (IsStandingOnSurface()) SetState(FighterState.Idle);
                else { _vy = 0; SetState(FighterState.Jump); }
            }
        }

        private void UpdateAttack(double dt)
        {
            int total = _combo ? _cfg.AttackFrames : _cfg.ComboSplitFrame;
            SetImage($"{_cfg.AttackPrefix}{_frame + 1}.png");
            if (Advance(total, _cfg.AttackFrameTime, false, dt)) SetState(FighterState.Idle);
        }

        private void UpdateThrow(double dt)
        {
            SetImage($"{_cfg.ThrowPrefix}{_frame + 1}.png");

            if (_frame >= _cfg.ThrowFrames - 1 && !_hasThrown)
            {
                _hasThrown = true;
                double spawnX = FacingRight ? (BodyX + BodyW) : (BodyX - 30);
                // [Phát thêm] Lấy dame phi tiêu riêng của từng nhân vật (_cfg.ThrowDamage)
                var proj = new Projectile(this, "sskanhphitieu", spawnX, BodyTop + 20, 30, 30, 600, _cfg.ThrowDamage, FacingRight);
                OnSpawnProjectile?.Invoke(proj);
            }

            if (Advance(_cfg.ThrowFrames, 0.08, false, dt)) SetState(FighterState.Idle);
        }

        private void UpdateUltimate(double dt)
        {
            SetImage($"{_cfg.UltimatePrefix}{_frame + 1}.png");

            // [Phát thêm] Sau khi gồng xong (qua số frame chuẩn bị), nhân vật chạy/lướt nhanh tới áp sát đối phương
            int chargeFrames = _cfg.UltimateChargeFrames > 0 ? _cfg.UltimateChargeFrames : _cfg.UltimateFrames / 2;
            if (_frame >= chargeFrames && !_ultHasHit && _ultDashedDistance < _cfg.UltimateDashDistance)
            {
                double dir = FacingRight ? 1 : -1;
                double step = Math.Min(_cfg.UltimateDashSpeed * dt, _cfg.UltimateDashDistance - _ultDashedDistance);
                X = Math.Clamp(X + dir * step, 20 - BodyOffsetX, StageWidth - 20 - BodyW - BodyOffsetX);
                _ultDashedDistance += step;
            }

            if (Advance(_cfg.UltimateFrames, 0.08, false, dt)) SetState(FighterState.Idle);
        }

        private void UpdateBlock()
        {
            SetImage(_cfg.BlockImage);
            if (!InputManager.IsKeyDown(_block)) SetState(FighterState.Idle);
        }

        private void UpdateHurt(double dt)
        {
            SetImage($"{_cfg.HurtPrefix}1.png");
            // [Phát thêm] Sử dụng _hurtDuration thay cho 0.3 cố định để khớp nhịp hồi phục sau chiêu cuối
            if (Advance(1, _hurtDuration, false, dt)) SetState(FighterState.Idle);
        }

        private void UpdateDead(double dt)
        {
            if (FeetY < GroundY) { _vy += Gravity * dt; Y += _vy * dt; }
            else Y = GroundY - Height + _cfg.FootPadding;

            SetImage($"{_cfg.HurtPrefix}{_frame + 1}.png");
            Advance(_cfg.DeadFrames, 0.15, false, dt);
        }

        private bool Advance(int count, double time, bool loop, double dt)
        {
            _timer += dt;
            while (_timer >= time) { _timer -= time; _frame++; if (loop) _frame %= count; else if (_frame >= count) { _frame = count - 1; return true; } }
            return false;
        }

        private bool TryUseChakra(double cost)
        {
            if (Chakra < cost) return false;
            Chakra = Math.Max(0, Chakra - cost);
            return true;
        }

        private void SetState(FighterState s)
        {
            State = s; _frame = 0; _timer = 0; _lastHitFrame = -1; _combo = false;
            _hasThrown = false;
            _ultHasHit = false; // [Phát thêm] Đặt lại cờ trúng đòn của chiêu cuối
            _ultDashedDistance = 0; // [Phát thêm] Đặt lại quãng đường lướt của chiêu cuối
            _dashedDistance = 0;
        }
        private void SetImage(string f) { if (f == _currentFile) return; _currentFile = f; Sprite.Source = ImageCache.Get(f); }
        private void Draw() { Canvas.SetLeft(Sprite, X); Canvas.SetTop(Sprite, Y); _flip.ScaleX = FacingRight ? 1 : -1; }
    }
}