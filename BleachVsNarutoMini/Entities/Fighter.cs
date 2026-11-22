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
        private const double StageMargin = 65;      // [Phát thêm] Khoảng cách lề hai bên tường để nhân vật không chạm viền màn hình

        // [Phát thêm] Thông số kỹ năng Đánh thường (Attack)
        private const double AttackCooldown = 0.16;      // Thời gian chờ sau khi chạy hết chuỗi frame đánh (1 giây)
        private const double AttackStepDistance = 4;    // Độ nhúc nhích tiến lên phía trước ở mỗi tư thế đánh (pixel)

        // [Phát thêm] Thông số kỹ năng Lướt (Dash)
        private const double DashSpeed = 1000;          // Tốc độ lướt (pixel/giây)
        private const double DashDistance = 200;        // [Phát thêm] Độ dài quãng đường lướt (pixel) - tăng/giảm số này để chỉnh độ dài
        private const double DashCooldown = 0;          // Thời gian chờ giữa 2 lần lướt để chống nhấn liên tục

        // Mức tiêu hao và tốc độ hồi Chakra
        private const double JumpChakraCost = 10;
        private const double DashChakraCost = 10;       // [Phát thêm] Lướt tốn 10 chakra
        private const double TeleportChakraCost = 20;   // [Phát thêm] Tốc biến tốn 20 chakra
        private const double TeleportCooldown = 1.5;    // [Phát thêm] Thời gian chờ hồi chiêu tốc biến (1.5 giây)
        private const double ThrowChakraCost = 15;
        private const int MaxConsecutiveThrows = 2;     // [Phát thêm] Số lần ném phi tiêu liên tiếp tối đa
        private const double ThrowCooldown = 0.8;       // [Phát thêm] Thời gian chờ sau khi ném 2 lần liên tiếp (0.8 giây)
        private const double UltChakraCost = 70;
        private const double UltCooldown = 3.0;         // [Phát thêm] Thời gian chờ hồi chiêu cuối (3 giây)
        private const double ChakraRegenPerSec = 8;

        // [Phát thêm] Thông số thanh Chống đỡ (Guard)
        private const double MaxGuardValue = 45;        // [Phát thêm] Sức chịu đựng tối đa của thanh chống đỡ (45 điểm)
        private const double GuardRegenPerSec = 5;      // Tốc độ hồi thanh chống đỡ mỗi giây khi không đỡ
        private const double GuardRegenDelay = 1.0;     // Thời gian chờ sau khi đỡ đòn mới bắt đầu hồi lại (giây)

        // [Phát thêm] Thông số đẩy lùi khi trúng đòn (Knockback)
        private const double HitKnockbackSpeed = 320;   // Vận tốc bật lùi khi trúng đòn thường/phi tiêu (pixel/giây)
        private const double UltKnockbackSpeed = 480;   // Vận tốc bật lùi mạnh hơn khi trúng chiêu cuối (pixel/giây)
        private const double KnockbackFriction = 1400;  // Lực ma sát giảm tốc độ đẩy lùi mỗi giây

        private readonly FighterConfig _cfg;
        private Key _left, _right, _jump, _attack, _block, _throw, _ult, _dash, _teleport; // [Phát thêm] Thêm phím _dash và _teleport, [Nghĩa sửa] bỏ readonly để gán phím lại được
        private readonly ScaleTransform _flip = new(1, 1);

        private double _vy, _timer;
        private double _attackCooldownTimer;     // [Phát thêm] Bộ đếm thời gian hồi sau khi chạy hết frame đánh (1 giây)
        private int _lastAttackSteppedFrame = -1;// [Phát thêm] Ghi nhớ frame đánh vừa nhúc nhích để mỗi tư thế chỉ bước 1 lần
        private double _dashedDistance;          // [Phát thêm] Quãng đường đã lướt trong lượt hiện tại
        private double _dashCooldownTimer;       // [Phát thêm] Bộ đếm thời gian hồi chiêu lướt
        private double _teleportCooldownTimer;   // [Phát thêm] Bộ đếm thời gian hồi chiêu tốc biến
        private double _throwCooldownTimer;      // [Phát thêm] Bộ đếm thời gian hồi sau 2 lần ném phi tiêu liên tiếp (0.5s)
        private double _throwComboResetTimer;    // [Phát thêm] Bộ đếm đặt lại chuỗi ném nếu chỉ ném 1 lần rồi nghỉ
        private int _consecutiveThrows;          // [Phát thêm] Đếm số lần đã ném phi tiêu liên tiếp
        private double _ultCooldownTimer;        // [Phát thêm] Bộ đếm thời gian hồi chiêu cuối (3 giây)
        private double _ultDashedDistance;       // [Phát thêm] Quãng đường đã lướt trong lượt tung chiêu cuối hiện tại
        private double _guardRegenTimer;         // [Phát thêm] Bộ đếm thời gian trễ trước khi hồi thanh chống đỡ
        private double _hurtDuration = 0.3;      // [Phát thêm] Thời gian bị choáng khi trúng đòn (mặc định 0.3s)
        private double _knockbackVelocity;       // [Phát thêm] Vận tốc đẩy lùi hiện tại (> 0 đẩy sang phải, < 0 đẩy sang trái)
        private double _pendingKnockbackVelocity;// [Phát thêm] Vận tốc đẩy lùi chờ kích hoạt ở nhịp kết thúc chiêu cuối
        private double _knockbackDelayTimer;     // [Phát thêm] Thời gian giữ đối phương đứng chịu đòn trước khi bật lùi
        private int _frame;
        private int _lastHitFrame = -1;
        private bool _combo, _attackWasDown, _jumpWasDown, _throwWasDown, _ultWasDown, _dashWasDown, _teleportWasDown, _hasThrown, _ultHasHit; // [Phát thêm] Thêm _dashWasDown và _teleportWasDown
        private string _currentFile = "";

        public Image Sprite { get; }
        public FighterState State { get; private set; } = FighterState.Idle;
        public bool FacingRight { get; private set; }
        public int Hp { get; private set; } = 300;
        public double MaxChakra { get; } = 200;
        public double Chakra { get; private set; } = 200;
        public double MaxGuard { get; } = MaxGuardValue;           // [Phát thêm] Giá trị tối đa thanh chống đỡ
        public double Guard { get; private set; } = MaxGuardValue; // [Phát thêm] Giá trị hiện tại thanh chống đỡ
        public bool IsDead => Hp <= 0;

        public double StageWidth { get; set; } = 1024;
        public double GroundY { get; set; } = 620;
        public IReadOnlyList<Rect> Platforms { get; set; } = Array.Empty<Rect>();
        public Fighter? Opponent { get; set; }           // [Phát thêm] Tham chiếu đối thủ để xác định vị trí sau lưng khi tốc biến
        public event Action<Projectile>? OnSpawnProjectile;
        public event Action<DustEffect>? OnSpawnEffect; // [Phát thêm] Sự kiện sinh hiệu ứng bụi/khói
        public event Action? OnNotEnoughChakra;         // [Phát thêm] Sự kiện báo hiệu khi bấm chiêu lúc không đủ Chakra

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

        // [Phát thêm] Bổ sung tham số Key dash và Key teleport vào constructor
        protected Fighter(double x, FighterConfig cfg,
                          Key left, Key right, Key jump, Key attack, Key block, Key throwSkill, Key ult, Key dash, Key teleport = Key.None)
            : base(x, cfg.GroundBottom - cfg.Height + cfg.FootPadding, cfg.Width, cfg.Height, 260)
        {
            _cfg = cfg;
            _left = left; _right = right; _jump = jump; _attack = attack;
            _block = block; _throw = throwSkill; _ult = ult; _dash = dash; _teleport = teleport; // [Phát thêm] Gán phím lướt & tốc biến

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
                _block = Key.S; _throw = Key.U; _ult = Key.I; _dash = Key.L; _teleport = Key.O; // [Phát thêm] Nút O tốc biến cho P1
            }
            else
            {
                _left = Key.Left; _right = Key.Right; _jump = Key.D2; _attack = Key.D1;
                _block = Key.Down; _throw = Key.D4; _ult = Key.D5; _dash = Key.D3; _teleport = Key.D6; // [Phát thêm] Nút 6 tốc biến cho P2
            }

            X = spawnX;
            FacingRight = playerIndex == 1;
            Draw();
        }

        public void UpdateLogic(double dt)
        {
            if (State == FighterState.Dead) { UpdateDead(dt); Draw(); return; }

            // [Phát thêm] Cập nhật nhịp chờ đẩy lùi của chiêu cuối và lực trượt lùi
            UpdateKnockback(dt);

            if (Chakra < MaxChakra && ChakraRegenPerSec > 0)
                Chakra = Math.Min(MaxChakra, Chakra + ChakraRegenPerSec * dt);

            // [Phát thêm] Giảm thời gian chờ hồi thanh chống đỡ và tự hồi khi không giữ nút đỡ
            if (_guardRegenTimer > 0)
                _guardRegenTimer = Math.Max(0, _guardRegenTimer - dt);
            else if (State != FighterState.Block && Guard < MaxGuard && GuardRegenPerSec > 0)
                Guard = Math.Min(MaxGuard, Guard + GuardRegenPerSec * dt);

            // [Phát thêm] Giảm thời gian hồi chiêu của Đánh thường, Lướt, Tốc biến, Ném phi tiêu và Chiêu cuối
            if (_attackCooldownTimer > 0)
                _attackCooldownTimer = Math.Max(0, _attackCooldownTimer - dt);
            if (_dashCooldownTimer > 0)
                _dashCooldownTimer = Math.Max(0, _dashCooldownTimer - dt);
            if (_teleportCooldownTimer > 0)
                _teleportCooldownTimer = Math.Max(0, _teleportCooldownTimer - dt);
            if (_throwCooldownTimer > 0)
                _throwCooldownTimer = Math.Max(0, _throwCooldownTimer - dt);
            if (_ultCooldownTimer > 0)
                _ultCooldownTimer = Math.Max(0, _ultCooldownTimer - dt);

            // [Phát thêm] Nếu đã ném 1 lần nhưng đợi quá 0.5s không ném tiếp thì đặt lại bộ đếm về 0
            if (State != FighterState.Throw && _consecutiveThrows > 0 && _throwCooldownTimer <= 0)
            {
                _throwComboResetTimer -= dt;
                if (_throwComboResetTimer <= 0) _consecutiveThrows = 0;
            }

            bool attackDown = InputManager.IsKeyDown(_attack);
            bool attackPressed = attackDown && !_attackWasDown;
            _attackWasDown = attackDown;

            bool jumpDown = InputManager.IsKeyDown(_jump);
            bool jumpPressed = jumpDown && !_jumpWasDown;
            _jumpWasDown = jumpDown;

            // [Phát thêm] Bắt nhịp nhấn từng lần đối với phím ném phi tiêu, chiêu cuối, lướt và tốc biến
            bool throwDown = InputManager.IsKeyDown(_throw);
            bool throwPressed = throwDown && !_throwWasDown;
            _throwWasDown = throwDown;

            bool ultDown = InputManager.IsKeyDown(_ult);
            bool ultPressed = ultDown && !_ultWasDown;
            _ultWasDown = ultDown;

            bool dashDown = InputManager.IsKeyDown(_dash);
            bool dashPressed = dashDown && !_dashWasDown;
            _dashWasDown = dashDown;

            bool teleportDown = InputManager.IsKeyDown(_teleport);
            bool teleportPressed = teleportDown && !_teleportWasDown;
            _teleportWasDown = teleportDown;

            int dir = (InputManager.IsKeyDown(_right) ? 1 : 0) - (InputManager.IsKeyDown(_left) ? 1 : 0);

            // [Phát thêm] Kích hoạt Tốc biến (O/6) khi đứng, chạy, nhảy HOẶC đang thi triển Chiêu cuối (I/5) để đánh úp sau lưng
            bool isUltTeleport = State == FighterState.Ultimate && !_ultHasHit;
            bool canTeleport = State == FighterState.Idle || State == FighterState.Run || State == FighterState.Jump || isUltTeleport;
            if (canTeleport && teleportPressed && _teleportCooldownTimer <= 0 && Opponent != null && !Opponent.IsDead)
            {
                if (TryUseChakra(TeleportChakraCost))
                {
                    _teleportCooldownTimer = TeleportCooldown;
                    PerformTeleport(isUltTeleport);
                    Draw();
                    return;
                }
                else OnNotEnoughChakra?.Invoke();
            }

            if (State == FighterState.Idle || State == FighterState.Run)
            {
                // [Phát thêm] Chỉ cho đỡ đòn khi thanh chống đỡ còn điểm (Guard > 0)
                if (InputManager.IsKeyDown(_block) && Guard > 0) SetState(FighterState.Block);
                else if (InputManager.IsKeyDown(_ult))
                {
                    // [Phát thêm] Chỉ cho kích hoạt chiêu cuối khi đã hết 3 giây hồi chiêu
                    if (_ultCooldownTimer <= 0)
                    {
                        if (TryUseChakra(UltChakraCost)) SetState(FighterState.Ultimate);
                        else if (ultPressed) OnNotEnoughChakra?.Invoke(); // [Phát thêm] Báo đỏ + rung thanh Chakra khi thiếu mana dùng chiêu cuối
                    }
                }
                else if (InputManager.IsKeyDown(_throw))
                {
                    // [Phát thêm] Chỉ cho ném phi tiêu khi chưa vượt quá 2 lần liên tiếp (đã hết 0.5s hồi chiêu)
                    if (_throwCooldownTimer <= 0)
                    {
                        if (TryUseChakra(ThrowChakraCost))
                        {
                            _consecutiveThrows++;
                            SetState(FighterState.Throw);
                        }
                        else if (throwPressed) OnNotEnoughChakra?.Invoke(); // [Phát thêm] Báo đỏ + rung thanh Chakra khi thiếu mana ném phi tiêu
                    }
                }
                // [Phát thêm] Kích hoạt Lướt khi đang đứng/chạy trên mặt đất (không cho lướt khi đang nhảy)
                else if (dashPressed && _dashCooldownTimer <= 0)
                {
                    if (TryUseChakra(DashChakraCost))
                    {
                        if (dir != 0) FacingRight = dir > 0;
                        _dashCooldownTimer = DashCooldown;
                        SetState(FighterState.Dash);
                        SpawnDustEffect(); // [Phát thêm] Tạo hiệu ứng bụi dưới chân khi lướt
                    }
                    else OnNotEnoughChakra?.Invoke();
                }
                // [Phát thêm] Chỉ cho đánh tiếp sau khi hết 1 giây hồi chiêu của chuỗi đánh trước
                else if (attackPressed && _attackCooldownTimer <= 0) SetState(FighterState.Attack);
                else if (jumpPressed)
                {
                    if (TryUseChakra(JumpChakraCost))
                    {
                        _vy = JumpPower;
                        SetState(FighterState.Jump);
                        SpawnDustEffect(); // [Phát thêm] Tạo hiệu ứng bụi dưới chân khi nhảy
                    }
                    else OnNotEnoughChakra?.Invoke(); // [Phát thêm] Báo đỏ + rung thanh Chakra khi thiếu mana nhảy
                }
            }
            else if (State == FighterState.Attack && attackPressed) _combo = true;
            // [Phát thêm] Báo thiếu Chakra nếu người chơi bấm chiêu trong lúc đang thực hiện động tác khác
            else if ((ultPressed && _ultCooldownTimer <= 0 && Chakra < UltChakraCost) ||
                     (throwPressed && _throwCooldownTimer <= 0 && Chakra < ThrowChakraCost) ||
                     (jumpPressed && Chakra < JumpChakraCost) ||
                     (canTeleport && teleportPressed && _teleportCooldownTimer <= 0 && Chakra < TeleportChakraCost) ||
                     (State != FighterState.Jump && dashPressed && _dashCooldownTimer <= 0 && Chakra < DashChakraCost))
            {
                OnNotEnoughChakra?.Invoke();
            }

            bool canMoveX = State == FighterState.Idle || State == FighterState.Run || State == FighterState.Jump;
            if (canMoveX && dir != 0)
            {
                X = ClampStageX(X + dir * Speed * dt); // [Phát thêm] Giới hạn theo StageMargin để không chạm tường
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

        // [Phát thêm] Tốc biến ra sau lưng đối thủ, thả khói 2 đầu và giữ nguyên Chiêu cuối nếu đang kết hợp I + O
        private void PerformTeleport(bool keepUltimate = false)
        {
            if (Opponent == null) return;

            // 1. Tạo làn khói ngay dưới chân tại vị trí cũ trước khi biến mất
            SpawnDustEffect();

            // 2. Tính tọa độ phía sau lưng đối thủ dựa theo hướng mặt đối thủ đang nhìn
            double oppCenterX = Opponent.BodyX + Opponent.BodyW / 2;
            double behindOffset = (Opponent.BodyW + BodyW) / 2 + 25;
            double rawTargetX = Opponent.FacingRight
                ? (oppCenterX - behindOffset - BodyW / 2)   // Đối thủ nhìn sang phải -> xuất hiện bên trái (sau lưng)
                : (oppCenterX + behindOffset - BodyW / 2);  // Đối thủ nhìn sang trái -> xuất hiện bên phải (sau lưng)

            X = ClampStageX(rawTargetX);

            // 3. Đồng bộ độ cao với đối thủ và quay mặt về phía đối thủ
            Y = Math.Min(GroundY - Height + _cfg.FootPadding, Opponent.FeetY - Height + _cfg.FootPadding);
            double newCenterX = BodyX + BodyW / 2;
            FacingRight = oppCenterX >= newCenterX;

            // 4. Tạo làn khói ngay dưới chân tại vị trí mới vừa xuất hiện
            SpawnDustEffect();

            // 5. Nếu đang dùng Chiêu cuối (I/5), nhảy thẳng sang frame tung chiêu để đánh trúng ngay sau lưng đối thủ
            if (keepUltimate)
            {
                int chargeFrames = _cfg.UltimateChargeFrames > 0 ? _cfg.UltimateChargeFrames : _cfg.UltimateFrames / 2;
                if (_frame < chargeFrames)
                {
                    _frame = chargeFrames;
                    _timer = 0;
                }
                _ultDashedDistance = 0;
                return;
            }

            // 6. Tốc biến thường: đặt lại trạng thái đứng/rơi mượt mà
            _vy = 0;
            SetState(IsStandingOnSurface() ? FighterState.Idle : FighterState.Jump);
        }

        // [Phát thêm] Giới hạn tọa độ X theo StageMargin để nhân vật dừng sát mép tường trong của bản đồ
        private double ClampStageX(double nextX) =>
            Math.Clamp(nextX, StageMargin - BodyOffsetX, StageWidth - StageMargin - BodyW - BodyOffsetX);

        // [Phát thêm] Tạo đám bụi/khói ngay dưới chân nhân vật tại thời điểm nhảy, lướt hoặc tốc biến
        private void SpawnDustEffect()
        {
            double footCenterX = BodyX + BodyW / 2;
            OnSpawnEffect?.Invoke(new DustEffect(footCenterX, FeetY));
        }

        public void TryHit(Fighter target)
        {
            if (target.IsDead) return;

            double myCenterX = BodyX + BodyW / 2; // [Phát thêm] Tọa độ tâm của người tấn công để tính hướng đẩy lùi

            // Đòn đánh thường và combo
            if (State == FighterState.Attack)
            {
                bool hitFrame = _frame == 1 || (_combo && _frame == 5);
                if (!hitFrame || _lastHitFrame == _frame) return;

                if (AttackBox.IntersectsWith(target.Bounds))
                {
                    target.TakeDamage(_cfg.AttackDamage, 0.3, myCenterX); // [Phát thêm] Lấy dame đánh thường riêng và truyền hướng va chạm
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
                    target.TakeDamage(_cfg.UltimateDamage, remainingUltTime, myCenterX);
                    _ultHasHit = true; // [Phát thêm] Đánh dấu đã trúng để dừng lướt ngay trước mặt đối phương và không gây dame lặp lại
                }
            }
        }

        // [Phát thêm] Xử lý đỡ đòn (chỉ đỡ phía trước; bị đánh sau lưng sẽ xuyên đỡ, tự xoay mặt và bật lùi)
        public void TakeDamage(int amount, double stunDuration = 0.3, double? sourceX = null)
        {
            if (IsDead) return;

            double myCenterX = BodyX + BodyW / 2;
            double? attackerCenterX = sourceX ?? (Opponent != null ? Opponent.BodyX + Opponent.BodyW / 2 : null);

            // [Phát thêm] Kiểm tra xem đòn đánh có đến từ phía sau lưng hay không
            bool isHitFromBehind = attackerCenterX.HasValue &&
                ((FacingRight && attackerCenterX.Value < myCenterX) ||
                 (!FacingRight && attackerCenterX.Value > myCenterX));

            // Chỉ đỡ được đòn khi đang giữ thế đỡ, còn điểm chống đỡ và đòn đánh đến từ phía trước mặt
            if (State == FighterState.Block && Guard > 0 && !isHitFromBehind)
            {
                _guardRegenTimer = GuardRegenDelay;

                if (Guard >= amount)
                {
                    // Thanh chống đỡ đủ sức chặn toàn bộ đòn đánh
                    Guard -= amount;
                    if (Guard <= 0)
                    {
                        Guard = 0;
                        ApplyKnockback(attackerCenterX, myCenterX, HitKnockbackSpeed * 0.8);
                        SetState(FighterState.Hurt);
                        _hurtDuration = 0.25; // Vỡ chống đỡ gây choáng nhẹ
                    }
                    return;
                }
                else
                {
                    // Thanh chống đỡ gần hết: hấp thụ phần sát thương bằng số điểm còn lại, phần dư trừ vào máu
                    int remainingDamage = (int)Math.Ceiling(amount - Guard);
                    Guard = 0;
                    amount = remainingDamage;
                }
            }

            Hp = Math.Max(0, Hp - amount);
            if (Hp == 0)
            {
                ApplyKnockback(attackerCenterX, myCenterX, UltKnockbackSpeed);
                SetState(FighterState.Dead);
                return;
            }

            // [Phát thêm] Nếu nhân vật đang tung Chiêu cuối thì không bị ngắt chiêu hay bật lùi giữa chừng
            if (State == FighterState.Ultimate) return;

            // [Phát thêm] Nếu trúng Chiêu cuối: giữ đối phương đứng chịu đòn đến frame cuối mới bật văng lùi về sau
            bool isUltimateHit = stunDuration > 0.35;
            double knockSpeed = isUltimateHit ? UltKnockbackSpeed : HitKnockbackSpeed;
            double delayBeforeKnock = isUltimateHit ? Math.Max(0, stunDuration - 0.12) : 0;
            ApplyKnockback(attackerCenterX, myCenterX, knockSpeed, delayBeforeKnock);

            SetState(FighterState.Hurt);
            _hurtDuration = stunDuration; // [Phát thêm] Gán thời gian choáng tương ứng với chiêu thức vừa trúng
        }

        // [Phát thêm] Quay mặt nhân vật về phía nguồn sát thương và đặt vận tốc đẩy lùi ngược hướng đối phương
        private void ApplyKnockback(double? attackerCenterX, double myCenterX, double speed, double delay = 0)
        {
            double velocity;
            if (attackerCenterX.HasValue && Math.Abs(attackerCenterX.Value - myCenterX) > 1)
            {
                bool attackerOnRight = attackerCenterX.Value > myCenterX;
                FacingRight = attackerOnRight;                 // Tự động xoay về hướng đối phương
                velocity = attackerOnRight ? -speed : speed;   // Bật lùi về phía sau lưng nhân vật
            }
            else
            {
                velocity = FacingRight ? -speed : speed;
            }

            if (delay > 0)
            {
                _knockbackVelocity = 0;
                _pendingKnockbackVelocity = velocity;
                _knockbackDelayTimer = delay;
            }
            else
            {
                _knockbackVelocity = velocity;
                _pendingKnockbackVelocity = 0;
                _knockbackDelayTimer = 0;
            }
        }

        // [Phát thêm] Cập nhật nhịp chờ bật lùi cuối chiêu và quãng đường trượt lùi theo ma sát
        private void UpdateKnockback(double dt)
        {
            if (_knockbackDelayTimer > 0)
            {
                _knockbackDelayTimer -= dt;
                if (_knockbackDelayTimer <= 0)
                {
                    _knockbackDelayTimer = 0;
                    _knockbackVelocity = _pendingKnockbackVelocity;
                    _pendingKnockbackVelocity = 0;
                }
                return;
            }

            if (Math.Abs(_knockbackVelocity) <= 0.1)
            {
                _knockbackVelocity = 0;
                return;
            }

            X = ClampStageX(X + _knockbackVelocity * dt);

            double decel = KnockbackFriction * dt;
            if (Math.Abs(_knockbackVelocity) <= decel)
                _knockbackVelocity = 0;
            else
                _knockbackVelocity -= Math.Sign(_knockbackVelocity) * decel;
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
            X = ClampStageX(X + dashDir * step); // [Phát thêm] Giới hạn theo StageMargin để không lướt xuyên tường
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
            // [Phát thêm] Mỗi khi chuyển sang tư thế (frame) đánh mới thì nhúc nhích tiến lên phía trước một chút
            if (_frame != _lastAttackSteppedFrame)
            {
                _lastAttackSteppedFrame = _frame;
                double stepDir = FacingRight ? 1 : -1;
                X = ClampStageX(X + stepDir * AttackStepDistance);
            }

            int total = _combo ? _cfg.AttackFrames : _cfg.ComboSplitFrame;
            SetImage($"{_cfg.AttackPrefix}{_frame + 1}.png");
            if (Advance(total, _cfg.AttackFrameTime, false, dt))
            {
                _attackCooldownTimer = AttackCooldown; // [Phát thêm] Chạy hết frame đánh thì đợi 1 giây mới được đánh tiếp
                SetState(FighterState.Idle);
            }
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
                X = ClampStageX(X + dir * step); // [Phát thêm] Giới hạn theo StageMargin để không lao xuyên tường
                _ultDashedDistance += step;
            }

            if (Advance(_cfg.UltimateFrames, 0.08, false, dt)) SetState(FighterState.Idle);
        }

        private void UpdateBlock()
        {
            // [Phát thêm] Cho phép xoay hướng đỡ trái/phải trong lúc đang giữ nút đỡ
            int dir = (InputManager.IsKeyDown(_right) ? 1 : 0) - (InputManager.IsKeyDown(_left) ? 1 : 0);
            if (dir != 0) FacingRight = dir > 0;

            SetImage(_cfg.BlockImage);
            // [Phát thêm] Nhả nút đỡ hoặc hết thanh chống đỡ sẽ thoát trạng thái phòng thủ
            if (!InputManager.IsKeyDown(_block) || Guard <= 0) SetState(FighterState.Idle);
        }

        private void UpdateHurt(double dt)
        {
            SetImage($"{_cfg.HurtPrefix}1.png");
            // [Phát thêm] Sử dụng _hurtDuration thay cho 0.3 cố định để khớp nhịp hồi phục sau chiêu cuối
            if (Advance(1, _hurtDuration, false, dt)) SetState(FighterState.Idle);
        }

        private void UpdateDead(double dt)
        {
            UpdateKnockback(dt); // [Phát thêm] Nhân vật bật lùi về sau khi dính đòn kết liễu
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
            // [Phát thêm] Khi kết thúc (hoặc rời khỏi) trạng thái Chiêu cuối, bắt đầu tính thời gian hồi 3 giây
            if (State == FighterState.Ultimate && s != FighterState.Ultimate)
                _ultCooldownTimer = UltCooldown;

            // [Phát thêm] Khi kết thúc động tác ném phi tiêu: đủ 2 lần liên tiếp thì khóa 0.5s, chưa đủ thì mở cửa sổ 0.5s để nối lần 2
            if (State == FighterState.Throw && s != FighterState.Throw)
            {
                if (_consecutiveThrows >= MaxConsecutiveThrows)
                {
                    _throwCooldownTimer = ThrowCooldown;
                    _consecutiveThrows = 0;
                }
                else
                {
                    _throwComboResetTimer = ThrowCooldown;
                }
            }

            // [Phát thêm] Xóa vận tốc đẩy lùi khi chủ động lướt hoặc bắt đầu trạng thái mới (giữ lại quán tính trượt cuối chiêu khi chuyển về Idle)
            if (s == FighterState.Dash || s == FighterState.Ultimate)
            {
                _knockbackVelocity = 0;
                _pendingKnockbackVelocity = 0;
                _knockbackDelayTimer = 0;
            }

            State = s; _frame = 0; _timer = 0; _lastHitFrame = -1; _lastAttackSteppedFrame = -1; _combo = false;
            _hasThrown = false;
            _ultHasHit = false; // [Phát thêm] Đặt lại cờ trúng đòn của chiêu cuối
            _ultDashedDistance = 0; // [Phát thêm] Đặt lại quãng đường lướt của chiêu cuối
            _dashedDistance = 0;
        }
        private void SetImage(string f) { if (f == _currentFile) return; _currentFile = f; Sprite.Source = ImageCache.Get(f); }
        private void Draw() { Canvas.SetLeft(Sprite, X); Canvas.SetTop(Sprite, Y); _flip.ScaleX = FacingRight ? 1 : -1; }
    }
}