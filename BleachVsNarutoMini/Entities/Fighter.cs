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

        private const double DashSpeed = 1100;
        private const double DashDistance = 200;
        private const double DashCooldown = 0.25;

        private const double JumpChakraCost = 10;
        private const double DashChakraCost = 10;
        private const double ThrowChakraCost = 15;
        private const double Special1ChakraCost = 30;
        private const double Special2ChakraCost = 40;
        private const double UltChakraCost = 70;
        private const double ChakraRegenPerSec = 10;

        private const string ShurikenPrefix = "Assets/effect/khiNhanVatPhongPhiTieu/sskanhphitieu";

        private readonly FighterConfig _cfg;
        private Key _left, _right, _jump, _attack, _block, _throw, _ult, _dash;
        private readonly ScaleTransform _flip = new(1, 1);

        private double _vy, _timer, _stateHoldTimer;
        private double _dashedDistance, _dashCooldownTimer, _ultDashedDistance;
        private double _hurtDuration = 0.3;
        private int _frame;
        private int _lastHitFrame = -1;
        private bool _combo, _attackWasDown, _jumpWasDown, _throwWasDown, _ultWasDown, _dashWasDown;
        private bool _hasSpawnedSkillObj, _skillHasHit;
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
        public event Action? OnNotEnoughChakra;

        public event Action<DustEffect>? OnSpawnEffect;         // <-- thêm
        public Fighter? Opponent { get; set; }                  // <-- thêm
        public bool ShowGuardBar => State == FighterState.Block;
        public double MaxGuard { get; } = 100;
        public double Guard { get; private set; } = 100;

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

        private Rect SkillBox => FacingRight
            ? new Rect(BodyX + BodyW, BodyTop - 10, _cfg.UltimateReach, BodyH + 10)
            : new Rect(BodyX - _cfg.UltimateReach, BodyTop - 10, _cfg.UltimateReach, BodyH + 10);

        protected Fighter(double x, FighterConfig cfg,
                          Key left, Key right, Key jump, Key attack, Key block, Key throwSkill, Key ult, Key dash)
            : base(x, cfg.GroundBottom - cfg.Height + cfg.FootPadding, cfg.Width, cfg.Height, 260)
        {
            _cfg = cfg;
            _left = left; _right = right; _jump = jump; _attack = attack;
            _block = block; _throw = throwSkill; _ult = ult; _dash = dash;

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
            Panel.SetZIndex(Sprite, 1); // <-- chèn dòng này vào đây
            SetClipImage(_cfg.Idle, 0);
            Draw();
        }

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

            if (_dashCooldownTimer > 0)
                _dashCooldownTimer = Math.Max(0, _dashCooldownTimer - dt);

            bool attackDown = InputManager.IsKeyDown(_attack);
            bool attackPressed = attackDown && !_attackWasDown;
            _attackWasDown = attackDown;

            bool jumpDown = InputManager.IsKeyDown(_jump);
            bool jumpPressed = jumpDown && !_jumpWasDown;
            _jumpWasDown = jumpDown;

            bool throwDown = InputManager.IsKeyDown(_throw);
            bool throwPressed = throwDown && !_throwWasDown;
            _throwWasDown = throwDown;

            bool ultDown = InputManager.IsKeyDown(_ult);
            bool ultPressed = ultDown && !_ultWasDown;
            _ultWasDown = ultDown;

            bool dashDown = InputManager.IsKeyDown(_dash);
            bool dashPressed = dashDown && !_dashWasDown;
            _dashWasDown = dashDown;

            bool blockDown = InputManager.IsKeyDown(_block);
            int dir = (InputManager.IsKeyDown(_right) ? 1 : 0) - (InputManager.IsKeyDown(_left) ? 1 : 0);

            // 1. Điều khiển khi đứng / chạy
            if (State == FighterState.Idle || State == FighterState.Run)
            {
                if (blockDown)
                {
                    if (!TryTriggerBlockCombos(attackPressed, throwPressed, ultPressed))
                        SetState(FighterState.Block);
                }
                else if (ultPressed)
                {
                    if (TryUseChakra(UltChakraCost)) SetState(FighterState.Ultimate);
                    else OnNotEnoughChakra?.Invoke();
                }
                else if (throwPressed)
                {
                    if (TryUseChakra(ThrowChakraCost)) SetState(FighterState.Throw);
                    else OnNotEnoughChakra?.Invoke();
                }
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
                    else OnNotEnoughChakra?.Invoke();
                }
            }
            // 2. Điều khiển khi đang giữ phím Đỡ (S / Down): mở khóa Ngồi đánh & 2 Chiêu đặc biệt
            else if (State == FighterState.Block)
            {
                TryTriggerBlockCombos(attackPressed, throwPressed, ultPressed);
            }
            // 3. Điều khiển trên không: Nhảy đánh, Nhảy ném phi tiêu, Lướt trên không
            else if (State == FighterState.Jump)
            {
                if (dashPressed && _dashCooldownTimer <= 0)
                {
                    if (TryUseChakra(DashChakraCost))
                    {
                        if (dir != 0) FacingRight = dir > 0;
                        _dashCooldownTimer = DashCooldown;
                        SetState(FighterState.Dash);
                    }
                    else OnNotEnoughChakra?.Invoke();
                }
                else if (attackPressed && _cfg.JumpAttack.HasFrames)
                {
                    SetState(FighterState.JumpAttack);
                }
                else if (throwPressed)
                {
                    if (TryUseChakra(ThrowChakraCost))
                        SetState(_cfg.JumpThrow.HasFrames ? FighterState.JumpThrow : FighterState.Throw);
                    else OnNotEnoughChakra?.Invoke();
                }
            }
            // 4. Nối combo đánh thường
            else if (State == FighterState.Attack && attackPressed)
            {
                _combo = true;
            }

            // Di chuyển ngang khi đứng, chạy, nhảy hoặc đang ra chiêu trên không
            bool canMoveX = State == FighterState.Idle || State == FighterState.Run
                         || State == FighterState.Jump || State == FighterState.JumpAttack || State == FighterState.JumpThrow;
            if (canMoveX && dir != 0)
            {
                X = Math.Clamp(X + dir * Speed * dt, 20 - BodyOffsetX, StageWidth - 20 - BodyW - BodyOffsetX);
                if (State != FighterState.JumpAttack && State != FighterState.JumpThrow)
                    FacingRight = dir > 0;
            }

            bool isAirState = State == FighterState.Jump || State == FighterState.JumpAttack
                           || State == FighterState.JumpThrow || State == FighterState.Dash;
            if (!isAirState && !IsStandingOnSurface())
            {
                _vy = 0;
                SetState(FighterState.Jump);
            }

            switch (State)
            {
                case FighterState.Idle:
                case FighterState.Run: UpdateGround(dir, dt); break;
                case FighterState.Jump: UpdateJump(dt); break;
                case FighterState.Dash: UpdateDash(dt); break;
                case FighterState.Block: UpdateBlock(dt); break;
                case FighterState.Attack: UpdateAttack(dt); break;
                case FighterState.JumpAttack: UpdateAirAction(_cfg.JumpAttack, false, dt); break;
                case FighterState.JumpThrow: UpdateAirAction(_cfg.JumpThrow, true, dt); break;
                case FighterState.CrouchAttack: UpdateOneShotClip(_cfg.CrouchAttack, dt); break;
                case FighterState.Throw: UpdateThrow(dt); break;
                case FighterState.Special1: UpdateSpecial1(dt); break;
                case FighterState.Special2: UpdateOneShotClip(_cfg.Special2, dt); break;
                case FighterState.Ultimate: UpdateUltimate(dt); break;
                case FighterState.Hurt: UpdateHurt(dt); break;
                case FighterState.Knockdown: UpdateKnockdown(dt); break;
                case FighterState.GetUp: UpdateOneShotClip(_cfg.GetUp, dt); break;
            }

            Draw();
        }

        private bool TryTriggerBlockCombos(bool attackPressed, bool throwPressed, bool ultPressed)
        {
            if (ultPressed && _cfg.Special2.HasFrames)
            {
                if (TryUseChakra(Special2ChakraCost)) { SetState(FighterState.Special2); return true; }
                OnNotEnoughChakra?.Invoke();
                return true;
            }
            if (throwPressed && _cfg.Special1.HasFrames)
            {
                if (TryUseChakra(Special1ChakraCost)) { SetState(FighterState.Special1); return true; }
                OnNotEnoughChakra?.Invoke();
                return true;
            }
            if (attackPressed && _cfg.CrouchAttack.HasFrames)
            {
                SetState(FighterState.CrouchAttack);
                return true;
            }
            return false;
        }

        public void TryHit(Fighter target)
        {
            if (target.IsDead || target.State == FighterState.Knockdown || target.State == FighterState.GetUp)
                return;

            // Đánh thường nhiều nhịp theo bộ ảnh 17-18 frame
            if (State == FighterState.Attack)
            {
                bool hitFrame = _frame == 2 || (_combo && (_frame == 7 || _frame == 11 || _frame == 15));
                if (!hitFrame || _lastHitFrame == _frame) return;

                if (AttackBox.IntersectsWith(target.Bounds))
                {
                    bool isFinisher = _combo && _frame >= 15;
                    target.TakeDamage(_cfg.AttackDamage, isFinisher ? 0.5 : 0.25, isFinisher);
                    _lastHitFrame = _frame;
                }
            }
            // Nhảy đánh
            else if (State == FighterState.JumpAttack)
            {
                if (_skillHasHit || _frame < _cfg.JumpAttack.Count / 3) return;
                if (AttackBox.IntersectsWith(target.Bounds))
                {
                    target.TakeDamage(_cfg.JumpAttackDamage, 0.3, false);
                    _skillHasHit = true;
                }
            }
            // Ngồi đánh / Đánh đặc biệt (S + J)
            else if (State == FighterState.CrouchAttack)
            {
                if (_skillHasHit || _frame < _cfg.CrouchAttack.Count / 2) return;
                if (AttackBox.IntersectsWith(target.Bounds))
                {
                    target.TakeDamage(_cfg.CrouchAttackDamage, 0.35, false);
                    _skillHasHit = true;
                }
            }
            // Chiêu đặc biệt 1 dạng cận chiến (Naruto danhdacbiet; Sasuke phunlua bắn đạn riêng)
            else if (State == FighterState.Special1 && string.IsNullOrEmpty(_cfg.Special1ProjectilePrefix))
            {
                if (_skillHasHit || _frame < _cfg.Special1.Count / 3) return;
                if (SkillBox.IntersectsWith(target.Bounds))
                {
                    target.TakeDamage(_cfg.Special1Damage, 0.45, true);
                    _skillHasHit = true;
                }
            }
            // Chiêu đặc biệt 2 (Naruto chieudacbiet / Sasuke dungchieudacbiet)
            else if (State == FighterState.Special2)
            {
                if (_skillHasHit || _frame < _cfg.Special2.Count / 3) return;
                if (SkillBox.IntersectsWith(target.Bounds))
                {
                    target.TakeDamage(_cfg.Special2Damage, 0.5, true);
                    _skillHasHit = true;
                }
            }
            // Chiêu cuối (Rasengan / Chidori)
            else if (State == FighterState.Ultimate)
            {
                int chargeFrames = _cfg.UltimateChargeFrames > 0 ? _cfg.UltimateChargeFrames : _cfg.Ultimate.Count / 2;
                if (_frame < chargeFrames || _skillHasHit) return;

                if (SkillBox.IntersectsWith(target.Bounds))
                {
                    double remainingUltTime = Math.Max(0.5, (_cfg.Ultimate.Count - _frame) * _cfg.Ultimate.FrameTime);
                    target.TakeDamage(_cfg.UltimateDamage, remainingUltTime, true);
                    _skillHasHit = true;
                }
            }
        }

        public void TakeDamage(int amount, double stunDuration = 0.3, bool knockdown = false)
        {
            if (State == FighterState.Block || IsDead) return;
            Hp = Math.Max(0, Hp - amount);

            if (Hp == 0)
            {
                SetState(FighterState.Dead);
            }
            else if (knockdown && _cfg.Knockdown.HasFrames)
            {
                SetState(FighterState.Knockdown);
                _hurtDuration = Math.Max(0.35, stunDuration);
            }
            else
            {
                SetState(FighterState.Hurt);
                _hurtDuration = stunDuration;
            }
        }

        private bool IsStandingOnSurface() => Math.Abs(FeetY - GroundY) <= 2;

        private void UpdateGround(int dir, double dt)
        {
            var want = dir != 0 ? FighterState.Run : FighterState.Idle;
            if (want != State) SetState(want);

            var clip = State == FighterState.Run ? _cfg.Run : _cfg.Idle;
            SetClipImage(clip, _frame);
            Advance(clip.Count, clip.FrameTime, true, dt);
        }

        private void UpdateJump(double dt)
        {
            _vy += Gravity * dt;
            Y += _vy * dt;
            if (FeetY >= GroundY)
            {
                Y = GroundY - Height + _cfg.FootPadding;
                _vy = 0;
                SetState(FighterState.Idle);
                return;
            }

            double t = Math.Clamp((_vy - JumpPower) / (-2 * JumpPower), 0, 0.999);
            int idx = (int)(t * _cfg.Jump.Count);
            SetClipImage(_cfg.Jump, idx);
        }

        private void UpdateAirAction(AnimClip clip, bool spawnShuriken, double dt)
        {
            _vy += Gravity * dt;
            Y += _vy * dt;
            if (FeetY >= GroundY)
            {
                Y = GroundY - Height + _cfg.FootPadding;
                _vy = 0;
            }

            SetClipImage(clip, _frame);

            if (spawnShuriken && _frame >= clip.Count - 1 && !_hasSpawnedSkillObj)
            {
                _hasSpawnedSkillObj = true;
                SpawnShuriken();
            }

            if (Advance(clip.Count, clip.FrameTime, false, dt))
            {
                SetState(IsStandingOnSurface() ? FighterState.Idle : FighterState.Jump);
            }
        }

        private void UpdateDash(double dt)
        {
            double dashDir = FacingRight ? 1 : -1;
            double step = Math.Min(DashSpeed * dt, DashDistance - _dashedDistance);
            X = Math.Clamp(X + dashDir * step, 20 - BodyOffsetX, StageWidth - 20 - BodyW - BodyOffsetX);
            _dashedDistance += step;

            var dashClip = _cfg.Dash.HasFrames ? _cfg.Dash : _cfg.Run;
            SetClipImage(dashClip, _frame);
            Advance(dashClip.Count, dashClip.FrameTime, true, dt);

            if (_dashedDistance >= DashDistance)
            {
                if (IsStandingOnSurface()) SetState(FighterState.Idle);
                else { _vy = 0; SetState(FighterState.Jump); }
            }
        }

        private void UpdateBlock(double dt)
        {
            SetClipImage(_cfg.Block, _frame);
            if (_cfg.Block.Count > 1)
                Advance(_cfg.Block.Count, _cfg.Block.FrameTime, false, dt);

            if (!InputManager.IsKeyDown(_block))
                SetState(FighterState.Idle);
        }

        private void UpdateAttack(double dt)
        {
            int total = _combo ? _cfg.Attack.Count : Math.Min(_cfg.ComboSplitFrame, _cfg.Attack.Count);
            SetClipImage(_cfg.Attack, _frame);
            if (Advance(total, _cfg.Attack.FrameTime, false, dt))
                SetState(FighterState.Idle);
        }

        private void UpdateThrow(double dt)
        {
            SetClipImage(_cfg.Throw, _frame);

            if (_frame >= _cfg.Throw.Count - 1 && !_hasSpawnedSkillObj)
            {
                _hasSpawnedSkillObj = true;
                SpawnShuriken();
            }

            if (Advance(_cfg.Throw.Count, _cfg.Throw.FrameTime, false, dt))
                SetState(FighterState.Idle);
        }

        private void UpdateSpecial1(double dt)
        {
            SetClipImage(_cfg.Special1, _frame);

            // Nếu nhân vật có đạn hiệu ứng cho Special 1 (như chiêu Phun Lửa của Sasuke)
            if (!string.IsNullOrEmpty(_cfg.Special1ProjectilePrefix)
                && _frame >= _cfg.Special1SpawnFrame && !_hasSpawnedSkillObj)
            {
                _hasSpawnedSkillObj = true;
                double spawnX = FacingRight ? (BodyX + BodyW) : (BodyX - 85);
                var fireProj = new Projectile(
                    this, _cfg.Special1ProjectilePrefix,
                    spawnX, BodyTop + 10, 85, 65, 520, _cfg.Special1Damage, FacingRight,
                    _cfg.Special1ProjectileFrames, 0.06, knockdown: true);
                OnSpawnProjectile?.Invoke(fireProj);
            }

            if (Advance(_cfg.Special1.Count, _cfg.Special1.FrameTime, false, dt))
                SetState(FighterState.Idle);
        }

        private void UpdateUltimate(double dt)
        {
            SetClipImage(_cfg.Ultimate, _frame);

            int chargeFrames = _cfg.UltimateChargeFrames > 0 ? _cfg.UltimateChargeFrames : _cfg.Ultimate.Count / 2;
            if (_frame >= chargeFrames && !_skillHasHit && _ultDashedDistance < _cfg.UltimateDashDistance)
            {
                double dir = FacingRight ? 1 : -1;
                double step = Math.Min(_cfg.UltimateDashSpeed * dt, _cfg.UltimateDashDistance - _ultDashedDistance);
                X = Math.Clamp(X + dir * step, 20 - BodyOffsetX, StageWidth - 20 - BodyW - BodyOffsetX);
                _ultDashedDistance += step;
            }

            if (Advance(_cfg.Ultimate.Count, _cfg.Ultimate.FrameTime, false, dt))
                SetState(FighterState.Idle);
        }

        private void UpdateOneShotClip(AnimClip clip, double dt)
        {
            if (!clip.HasFrames) { SetState(FighterState.Idle); return; }
            SetClipImage(clip, _frame);
            if (Advance(clip.Count, clip.FrameTime, false, dt))
                SetState(FighterState.Idle);
        }

        private void UpdateHurt(double dt)
        {
            SetClipImage(_cfg.Hurt, _frame);
            Advance(_cfg.Hurt.Count, _cfg.Hurt.FrameTime, false, dt);

            _stateHoldTimer += dt;
            if (_stateHoldTimer >= _hurtDuration)
                SetState(FighterState.Idle);
        }

        private void UpdateKnockdown(double dt)
        {
            if (FeetY < GroundY) { _vy += Gravity * dt; Y += _vy * dt; }
            else { Y = GroundY - Height + _cfg.FootPadding; _vy = 0; }

            SetClipImage(_cfg.Knockdown, _frame);
            bool finishedAnim = Advance(_cfg.Knockdown.Count, _cfg.Knockdown.FrameTime, false, dt);

            _stateHoldTimer += dt;
            if (finishedAnim && _stateHoldTimer >= _hurtDuration)
                SetState(_cfg.GetUp.HasFrames ? FighterState.GetUp : FighterState.Idle);
        }

        private void UpdateDead(double dt)
        {
            if (FeetY < GroundY) { _vy += Gravity * dt; Y += _vy * dt; }
            else Y = GroundY - Height + _cfg.FootPadding;

            var deadClip = _cfg.Knockdown.HasFrames ? _cfg.Knockdown : _cfg.Hurt;
            SetClipImage(deadClip, _frame);
            Advance(deadClip.Count, 0.14, false, dt);
        }

        private void SpawnShuriken()
        {
            double spawnX = FacingRight ? (BodyX + BodyW) : (BodyX - 30);
            var proj = new Projectile(this, ShurikenPrefix, spawnX, BodyTop + 20, 30, 30, 600, _cfg.ThrowDamage, FacingRight, 5, 0.05);
            OnSpawnProjectile?.Invoke(proj);
        }

        private bool Advance(int count, double time, bool loop, double dt)
        {
            if (count <= 1) return true;
            _timer += dt;
            while (_timer >= time)
            {
                _timer -= time;
                _frame++;
                if (loop) _frame %= count;
                else if (_frame >= count)
                {
                    _frame = count - 1;
                    return true;
                }
            }
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
            State = s;
            _frame = 0;
            _timer = 0;
            _stateHoldTimer = 0;
            _lastHitFrame = -1;
            _combo = false;
            _hasSpawnedSkillObj = false;
            _skillHasHit = false;
            _ultDashedDistance = 0;
            _dashedDistance = 0;

            if (s == FighterState.Dash && _cfg.DashEffect is { HasFrames: true } dashFx)
                SpawnEffect(dashFx, FighterState.Dash);
            else if (s == FighterState.Ultimate && _cfg.UltimateBackEffect is { HasFrames: true } ultFx)
                SpawnEffect(ultFx, FighterState.Ultimate);
        }

        private void SpawnEffect(EffectConfig fx, FighterState activeState)
        {
            double dir = FacingRight ? 1 : -1;
            double centerX = X + Width / 2 + fx.OffsetX * dir;
            double targetY = (fx.IsCenterY ? (Y + Height / 2) : FeetY) + fx.OffsetY;

            var effect = new DustEffect(
                fx.Prefix, fx.Start, fx.Frames, fx.FrameTime,
                centerX, targetY, fx.Width, fx.Height,
                fx.IsCenterY, fx.GroundOffsetY,
                FacingRight, fx.ZIndex,
                fx.FollowOwner ? this : null, activeState, fx.OffsetX, fx.OffsetY, fx.Loop);

            OnSpawnEffect?.Invoke(effect);
        }
        private void SetClipImage(AnimClip clip, int frameIndex)
        {
            if (!clip.HasFrames) return;
            string f = clip.GetFile(frameIndex);
            if (f == _currentFile) return;
            _currentFile = f;
            Sprite.Source = ImageCache.Get(f);
        }

        private void Draw()
        {
            Canvas.SetLeft(Sprite, X);
            Canvas.SetTop(Sprite, Y);
            _flip.ScaleX = FacingRight ? 1 : -1;
        }
    }
}