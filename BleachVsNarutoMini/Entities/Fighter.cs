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
        private const double StageMargin = 35;               // [Phát thêm] Khoảng cách lề hai bên tường để nhân vật đứng sát viền mà không lẹm hình

        // [Phát thêm] Thông số Đánh thường (Attack) chia 4 đợt combo
        private const double AttackCooldown = 0.17;          // Thời gian chờ sau khi tung đòn hất tung kết thúc chuỗi combo (giây)
        private const double AttackComboResetTime = 0.25;     // Thời gian giữ chuỗi combo giữa 2 lần bấm J (0.36 giây)
        private const double AttackStepDistance = 2;         // Độ nhúc nhích tiến lên phía trước ở mỗi tư thế đánh (pixel)

        // [Phát thêm] Thông số Lướt (Dash)
        private const double DashSpeed = 1100;
        private const double DashDistance = 200;
        private const double DashCooldown = 0.25;

        // Mức tiêu hao và tốc độ hồi Chakra
        private const double JumpChakraCost = 10;
        private const double DashChakraCost = 10;
        private const double TeleportChakraCost = 20;        // [Phát thêm] Tốc biến tốn 20 Chakra
        private const double TeleportCooldown = 1.5;         // [Phát thêm] Hồi chiêu tốc biến (1.5 giây)
        private const double ThrowChakraCost = 10;
        private const int MaxConsecutiveThrows = 2;          // [Phát thêm] Số lần ném phi tiêu liên tiếp tối đa
        private const double ThrowCooldown = 0.5;            // [Phát thêm] Thời gian chờ sau khi ném 2 lần liên tiếp (0.5 giây)
        private const double Special1ChakraCost = 140;
        private const double Special1Cooldown = 0.5;         // [Phát thêm] Thời gian chờ hồi Chiêu đặc biệt 1 (giây)
        private const double Special2ChakraCost = 140;
        private const double Special2Cooldown = 0.5;         // [Phát thêm] Thời gian chờ hồi Chiêu đặc biệt 2 (giây)
        private const double UltChakraCost = 100;
        private const double UltCooldown = 2.0;              // [Phát thêm] Thời gian chờ hồi chiêu cuối (2 giây)
        private const double ChakraRegenPerSec = 10;

        // [Phát thêm] Thông số thanh Chống đỡ (Guard)
        private const double MaxGuardValue = 45;             // Sức chịu đựng tối đa của thanh chống đỡ (45 điểm)
        private const double GuardRegenPerSec = 12;          // Tốc độ hồi thanh chống đỡ mỗi giây khi không giữ nút đỡ
        private const double GuardRegenDelay = 0.8;          // Thời gian chờ sau khi đỡ đòn mới bắt đầu hồi lại (giây)
        private const double GuardVisibleDuration = 1.5;     // Thời gian thanh chống đỡ hiện lên sau khi đỡ trúng đòn (giây)

        // [Phát thêm] Thông số đẩy lùi & hất tung khi trúng đòn
        private const double LightHitKnockbackSpeed = 30;    // Vận tốc xê dịch nhẹ về sau ở các nhịp đánh trước frame cuối
        private const double HitKnockbackSpeed = 160;        // Vận tốc bật lùi khi trúng phi tiêu / vỡ giáp
        private const double SasukeCrouchAttackKnockbackSpeed = 600; // [Phát thêm] Vận tốc đẩy lùi vừa phải khi dứt chiêu Down + 1 / S + J của Sasuke (~80px)
        private const double LaunchKnockbackSpeed = 180;     // Vận tốc bay văng ngang ra sau khi bị hất lên trời ở frame cuối
        private const double LaunchUpVelocity = -920;        // Vận tốc hất tung cao lên trời ở frame cuối đánh thường (< 0 hướng lên)
        private const double LaunchApexHoldDuration = 0.25;  // Thời gian dừng lơ lửng trên đỉnh quỹ đạo trước khi rơi xuống (giây)
        private const double UltKnockbackSpeed = 240;        // Vận tốc bật lùi mạnh khi trúng chiêu cuối/chiêu đặc biệt
        private const double KnockbackFriction = 1800;       // Lực ma sát giảm tốc độ đẩy lùi mỗi giây
        private const double KnockdownGroundDuration = 1.25; // Thời gian nằm dưới đất trước khi đứng dậy (giây)

        // [Phát thêm] Thông số không chiến khi trúng đòn nhảy đánh (JumpAttack) trên không
        private const double AirHitKnockbackSpeed = 60;      // Vận tốc đẩy lùi một ít nhỏ trên không ở các nhịp trước frame cuối
        private const double AirSuspendDuration = 0.25;      // Thời gian 2 nhân vật đứng trên không nếu đòn tiếp theo không trúng (giây)
        private const double AirSlamDownVelocity = 1150;     // Vận tốc rớt nhanh xuống đất ở tư thế nằm khi trúng frame cuối

        // [Phát thêm] Thông số chiêu liên hoàn trên không S + I của Naruto (Special2 - 15 frames)
        private const double Sp2ChaseUpVelocity = -1160;     // Vận tốc nhảy đuổi theo đối thủ (nhanh hơn LaunchUpVelocity)
        private const double Sp2ExtraRiseDistance = 55;      // Đoạn bay vọt lên cao thêm sau khi ngang độ cao đối thủ (pixel)
        private const double Sp2ExtraRiseSpeed = -270;       // Vận tốc bay thêm đoạn ngắn trên đỉnh
        private const int Sp2SpinRepeats = 6;                // Số lần lặp frame 11 tạo hiệu ứng xoay
        private const double Sp2SpinStepTime = 0.042;        // Thời gian mỗi nhịp xoay của frame 11 (giây)
        private const double Sp2AirHoverDuration = 0.2;      // Thời gian đứng lơ lửng trên không sau khi hết 15 frame (giây)

        // [Phát thêm] Thông số chiêu liên hoàn S + I của Sasuke (Special2 - 23 frames)
        private const double SasukeSp2HitPauseDuration = 0.10; // Thời gian ngắt giữa mỗi nhịp mất máu trên không (0.1s)
        private const double SasukeSp2EndAirDelay = 0.20;      // Thời gian Sasuke đợi trên không sau khi đạp đối thủ xuống (0.2s)

        private const string ShurikenPrefix = "Assets/effect/khiNhanVatPhongPhiTieu/sskanhphitieu";

        private readonly FighterConfig _cfg;
        private Key _left, _right, _jump, _attack, _block, _throw, _ult, _dash, _teleport;
        // [Phát thêm] Phím phụ hỗ trợ cụm phím số bên phải (Numpad 1-6) cho Player 2
        private Key _jumpAlt = Key.None, _attackAlt = Key.None, _throwAlt = Key.None, _ultAlt = Key.None, _dashAlt = Key.None, _teleportAlt = Key.None;
        private readonly ScaleTransform _flip = new(1, 1);
        private readonly RotateTransform _spinRotate = new(0);

        private double _vy, _timer, _stateHoldTimer;
        private double _attackCooldownTimer;       // [Phát thêm] Bộ đếm hồi chiêu sau khi đánh hết chuỗi combo
        private int _attackComboStage;             // [Phát thêm] Đợt đánh hiện tại trong chuỗi combo (0..3)
        private double _attackComboResetTimer;     // [Phát thêm] Bộ đếm 0.5s giữ chuỗi combo; hết giờ sẽ quay về đợt 1
        private bool _queuedNextAttackStage;       // [Phát thêm] Đánh dấu đã bấm J nối tiếp đợt kế trong lúc đang ra đòn
        private int _lastAttackSteppedFrame = -1;  // [Phát thêm] Ghi nhớ frame đánh vừa nhúc nhích
        private double _dashedDistance, _dashCooldownTimer, _ultDashedDistance;
        private double _teleportCooldownTimer;     // [Phát thêm] Bộ đếm hồi chiêu tốc biến
        private bool _isTeleportAnim;              // [Phát thêm] Đánh dấu đang chạy animation đứng dịch chuyển sau khi tốc biến
        private double _throwCooldownTimer;        // [Phát thêm] Bộ đếm hồi sau 2 lần ném phi tiêu liên tiếp
        private double _throwComboResetTimer;      // [Phát thêm] Bộ đếm đặt lại chuỗi ném nếu chỉ ném 1 lần rồi nghỉ
        private int _consecutiveThrows;            // [Phát thêm] Đếm số lần đã ném phi tiêu liên tiếp
        private double _special1CooldownTimer;    // [Phát thêm] Bộ đếm hồi Chiêu đặc biệt 1
        private double _special2CooldownTimer;    // [Phát thêm] Bộ đếm hồi Chiêu đặc biệt 2
        private double _ultCooldownTimer;          // [Phát thêm] Bộ đếm hồi chiêu cuối
        private double _guardRegenTimer;           // [Phát thêm] Bộ đếm thời gian trễ trước khi hồi thanh chống đỡ
        private double _guardVisibleTimer;         // [Phát thêm] Bộ đếm thời gian hiển thị thanh chống đỡ
        private double _hurtDuration = 0.3;
        private double _knockbackVelocity;         // [Phát thêm] Vận tốc đẩy lùi hiện tại
        private double _pendingKnockbackVelocity;  // [Phát thêm] Vận tốc đẩy lùi chờ kích hoạt cuối chiêu
        private double _knockbackDelayTimer;       // [Phát thêm] Thời gian giữ đối phương đứng chịu đòn trước khi bật lùi
        private bool _pendingKnockdownAfterDelay;  // [Phát thêm] Chờ hết chiêu cuối mới chuyển sang trạng thái ngã
        private bool _isAirHurt;                   // [Phát thêm] Đánh dấu đang bay trên không ở tư thế bị đánh (không nằm)
        private double _airHurtHoldTimer;          // [Phát thêm] Bộ đếm thời gian dừng lơ lửng tại đỉnh quỹ đạo
        private bool _hasHeldAtApex;               // [Phát thêm] Đánh dấu đã dừng tại đỉnh quỹ đạo trong lượt bay hiện tại
        private double _airSuspendTimer;           // [Phát thêm] Bộ đếm giữ 2 nhân vật đứng trên không khi trúng nhảy đánh (0.25s)
        private bool _slamDustOnLand;              // [Phát thêm] Đánh dấu bung bụi khi chạm đất sau đòn đập từ trên không

        // [Phát thêm] Biến điều khiển chuỗi chiêu liên hoàn S + I của Naruto (Special2 - 15 frames) & khóa mục tiêu
        private bool _isComboLocked;               // Khóa không cho nhân vật bị trúng chiêu chạy thoát giữa combo
        private double _comboLockTimer;            // Thời gian bảo vệ tự mở khóa tránh kẹt trạng thái
        private int _sp2Phase;                     // 0: mặc định, 1..5: các giai đoạn combo Naruto
        private double _sp2ExtraRisen;             // Quãng đường đã bay thêm trên đỉnh ở phase 4
        private int _sp2SpinCount;                 // Số nhịp đã xoay bằng frame 11
        private double _sp2SpinTimer;              // Bộ đếm thời gian mỗi nhịp xoay frame 11
        private int _sp2DealtDamage;               // Tổng sát thương đã gây ra trong các nhịp trước cú đạp cuối
        private bool _sp2HasLaunched;              // Đã đá đối thủ bay lên ở phase 2 chưa
        private bool _sp2HasSlammed;               // Đã tung cú đạp kết thúc ở phase 5 chưa
        private bool _sp2HoveringAfterClip;        // Đánh dấu đã chạy hết 15 frame và đang đứng chờ trên không
        private double _sp2AirHoverTimer;          // Bộ đếm thời gian đứng trên không trước khi rơi xuống

        // [Phát thêm] Biến điều khiển chuỗi chiêu liên hoàn S + I của Sasuke (Special2 - 23 frames)
        private int _sasukeSp2Phase;               // 0: không dùng, 1: f1-5 đá hất, 2: f6 tốc biến lên không, 3: f7-11 đánh 1, 4: f12-15 đánh 2, 5: f16-18 đánh 3, 6: rơi 1 nửa & f6 tốc biến sau lưng, 7: f19-23 đập xuống & đợi 0.2s rơi
        private int _sasukeSp2SubStep;             // Bước con trong các pha tốc biến frame 6 (vị trí cũ -> vị trí mới)
        private double _sasukeSp2Timer;            // Bộ đếm thời gian chuyển bước tốc biến / đợi 0.2s trước khi rơi
        private double _sasukeSp2HitPauseTimer;    // Bộ đếm ngắt 0.1s sau mỗi nhịp gây sát thương trên không
        private double _sasukeSp2HalfwayFeetY;     // Tọa độ Y giữa quãng rơi để Sasuke tốc biến ra sau lưng
        private bool _sasukeSp2HasSlammed;         // Đã tung cú đập kết thúc ở frame 20 chưa
        private bool _sasukeSp2HoverAfterClip;     // Đã chạy xong frame 23 và đang đợi 0.2s trên không

        private int _frame;
        private int _lastHitFrame = -1;
        private bool _attackWasDown, _jumpWasDown, _throwWasDown, _ultWasDown, _dashWasDown, _teleportWasDown;
        private bool _hasSpawnedSkillObj, _skillHasHit;
        private string _currentFile = "";
        private double _currentOffsetY;            // [Phát thêm] Độ bù chiều cao riêng của frame đang hiển thị

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

        public event Action<DustEffect>? OnSpawnEffect;
        public Fighter? Opponent { get; set; }
        public double MaxGuard { get; } = MaxGuardValue;
        public double Guard { get; private set; } = MaxGuardValue;
        // [Phát thêm] Chỉ hiện thanh chống đỡ khi đỡ mà bị đánh trúng hoặc trong lúc thanh đang hồi phục
        public bool ShowGuardBar => _guardVisibleTimer > 0 || Guard < MaxGuard;

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
                          Key left, Key right, Key jump, Key attack, Key block, Key throwSkill, Key ult, Key dash, Key teleport = Key.None)
            : base(x, cfg.GroundBottom - cfg.Height + cfg.FootPadding, cfg.Width, cfg.Height, 260)
        {
            _cfg = cfg;
            _left = left; _right = right; _jump = jump; _attack = attack;
            _block = block; _throw = throwSkill; _ult = ult; _dash = dash; _teleport = teleport;

            FacingRight = cfg.StartFacingRight;
            GroundY = cfg.GroundBottom;

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(_spinRotate);
            transformGroup.Children.Add(_flip);

            Sprite = new Image
            {
                Width = Width,
                Height = Height,
                RenderTransform = transformGroup,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            RenderOptions.SetBitmapScalingMode(Sprite, BitmapScalingMode.NearestNeighbor);
            Panel.SetZIndex(Sprite, 1);
            SetClipImage(_cfg.Idle, 0);
            Draw();
        }

        public void AssignPlayer(int playerIndex, double spawnX)
        {
            if (playerIndex == 1)
            {
                _left = Key.A; _right = Key.D; _jump = Key.K; _attack = Key.J;
                _block = Key.S; _throw = Key.U; _ult = Key.I; _dash = Key.L; _teleport = Key.O;
                _jumpAlt = _attackAlt = _throwAlt = _ultAlt = _dashAlt = _teleportAlt = Key.None;
            }
            else
            {
                // [Phát thêm] Hỗ trợ cả dãy số trên (D1-D6) lẫn cụm phím số bên phải (NumPad1-NumPad6)
                _left = Key.Left; _right = Key.Right; _block = Key.Down;
                _jump = Key.D2; _jumpAlt = Key.NumPad2;
                _attack = Key.D1; _attackAlt = Key.NumPad1;
                _throw = Key.D4; _throwAlt = Key.NumPad4;
                _ult = Key.D5; _ultAlt = Key.NumPad5;
                _dash = Key.D3; _dashAlt = Key.NumPad3;
                _teleport = Key.D6; _teleportAlt = Key.NumPad6;
            }

            X = spawnX;
            FacingRight = playerIndex == 1;
            Draw();
        }

        private static bool IsActionDown(Key primary, Key secondary = Key.None) =>
            InputManager.IsKeyDown(primary) || (secondary != Key.None && InputManager.IsKeyDown(secondary));

        public void UpdateLogic(double dt)
        {
            if (State == FighterState.Dead) { UpdateDead(dt); Draw(); return; }

            // [Phát thêm] Kiểm tra trạng thái bị khóa cứng trong combo S + I của đối thủ
            if (_isComboLocked)
            {
                _comboLockTimer -= dt;
                if (_comboLockTimer <= 0 || Opponent == null || Opponent.IsDead || Opponent.State != FighterState.Special2)
                {
                    _isComboLocked = false;
                    _comboLockTimer = 0;
                }
            }

            // [Phát thêm] Cập nhật vật lý đẩy lùi và nhịp chờ cuối chiêu
            UpdateKnockback(dt);

            // [Phát thêm] Giảm thời gian giữ đứng trên không khi giao tranh bằng nhảy đánh
            if (_airSuspendTimer > 0)
                _airSuspendTimer = Math.Max(0, _airSuspendTimer - dt);

            if (Chakra < MaxChakra && ChakraRegenPerSec > 0)
                Chakra = Math.Min(MaxChakra, Chakra + ChakraRegenPerSec * dt);

            // [Phát thêm] Giảm thời gian chờ hồi thanh chống đỡ và tự hồi khi không giữ nút đỡ
            if (_guardRegenTimer > 0)
                _guardRegenTimer = Math.Max(0, _guardRegenTimer - dt);
            else if (State != FighterState.Block && Guard < MaxGuard && GuardRegenPerSec > 0)
                Guard = Math.Min(MaxGuard, Guard + GuardRegenPerSec * dt);

            // [Phát thêm] Giảm thời gian hiển thị thanh chống đỡ
            if (_guardVisibleTimer > 0)
                _guardVisibleTimer = Math.Max(0, _guardVisibleTimer - dt);

            // [Phát thêm] Giảm thời gian hồi chiêu các kỹ năng
            if (_attackCooldownTimer > 0)
                _attackCooldownTimer = Math.Max(0, _attackCooldownTimer - dt);
            if (_dashCooldownTimer > 0)
                _dashCooldownTimer = Math.Max(0, _dashCooldownTimer - dt);
            if (_teleportCooldownTimer > 0)
                _teleportCooldownTimer = Math.Max(0, _teleportCooldownTimer - dt);
            if (_throwCooldownTimer > 0)
                _throwCooldownTimer = Math.Max(0, _throwCooldownTimer - dt);
            if (_special1CooldownTimer > 0)
                _special1CooldownTimer = Math.Max(0, _special1CooldownTimer - dt);
            if (_special2CooldownTimer > 0)
                _special2CooldownTimer = Math.Max(0, _special2CooldownTimer - dt);
            if (_ultCooldownTimer > 0)
                _ultCooldownTimer = Math.Max(0, _ultCooldownTimer - dt);

            // [Phát thêm] Đếm ngược 0.5s giữ chuỗi combo đánh thường; quá 0.5s không bấm J tiếp sẽ quay lại đấm ban đầu
            if (State != FighterState.Attack && _attackComboStage > 0)
            {
                _attackComboResetTimer -= dt;
                if (_attackComboResetTimer <= 0)
                {
                    _attackComboStage = 0;
                    _attackComboResetTimer = 0;
                }
            }

            // [Phát thêm] Đặt lại chuỗi ném phi tiêu nếu chỉ ném 1 lần rồi nghỉ quá 0.5s
            if (State != FighterState.Throw && State != FighterState.JumpThrow && _consecutiveThrows > 0 && _throwCooldownTimer <= 0)
            {
                _throwComboResetTimer -= dt;
                if (_throwComboResetTimer <= 0) _consecutiveThrows = 0;
            }

            bool attackDown = IsActionDown(_attack, _attackAlt);
            bool attackPressed = attackDown && !_attackWasDown;
            _attackWasDown = attackDown;

            bool jumpDown = IsActionDown(_jump, _jumpAlt);
            bool jumpPressed = jumpDown && !_jumpWasDown;
            _jumpWasDown = jumpDown;

            bool throwDown = IsActionDown(_throw, _throwAlt);
            bool throwPressed = throwDown && !_throwWasDown;
            _throwWasDown = throwDown;

            bool ultDown = IsActionDown(_ult, _ultAlt);
            bool ultPressed = ultDown && !_ultWasDown;
            _ultWasDown = ultDown;

            bool dashDown = IsActionDown(_dash, _dashAlt);
            bool dashPressed = dashDown && !_dashWasDown;
            _dashWasDown = dashDown;

            bool teleportDown = IsActionDown(_teleport, _teleportAlt);
            bool teleportPressed = teleportDown && !_teleportWasDown;
            _teleportWasDown = teleportDown;

            bool blockDown = InputManager.IsKeyDown(_block);
            int dir = (InputManager.IsKeyDown(_right) ? 1 : 0) - (InputManager.IsKeyDown(_left) ? 1 : 0);

            // [Phát thêm] Nếu đang bị khóa trong combo S + I thì vô hiệu hóa toàn bộ nút điều khiển, chỉ chạy vật lý bị đánh
            if (_isComboLocked)
            {
                attackPressed = jumpPressed = throwPressed = ultPressed = dashPressed = teleportPressed = blockDown = false;
                dir = 0;
            }

            // [Phát thêm] Kích hoạt Tốc biến (O/6) khi đứng, chạy, nhảy HOẶC đang tung Chiêu cuối (I/5)
            bool isUltTeleport = State == FighterState.Ultimate && !_skillHasHit;
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

            // 1. Điều khiển khi đứng / chạy trên mặt đất
            if (State == FighterState.Idle || State == FighterState.Run)
            {
                if (blockDown)
                {
                    if (!TryTriggerBlockCombos(attackPressed, throwPressed, ultPressed) && Guard > 0)
                        SetState(FighterState.Block);
                }
                else if (ultPressed)
                {
                    if (_ultCooldownTimer <= 0)
                    {
                        if (TryUseChakra(UltChakraCost)) SetState(FighterState.Ultimate);
                        else OnNotEnoughChakra?.Invoke();
                    }
                }
                else if (throwPressed)
                {
                    if (_throwCooldownTimer <= 0)
                    {
                        if (TryUseChakra(ThrowChakraCost))
                        {
                            _consecutiveThrows++;
                            SetState(FighterState.Throw);
                        }
                        else OnNotEnoughChakra?.Invoke();
                    }
                }
                // [Phát thêm] Chỉ cho Lướt (Dash) khi đang ở trên mặt đất (không cho lướt trên không)
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
                // [Phát thêm] Bấm J trong vòng 0.5s sẽ đánh tiếp đợt kế; quá 0.5s tự quay về đợt 1
                else if (attackPressed && _attackCooldownTimer <= 0)
                {
                    if (_attackComboResetTimer <= 0)
                        _attackComboStage = 0;
                    SetState(FighterState.Attack);
                }
                else if (jumpPressed)
                {
                    if (TryUseChakra(JumpChakraCost))
                    {
                        _vy = JumpPower;
                        SetState(FighterState.Jump);
                        SpawnFootDust();
                    }
                    else OnNotEnoughChakra?.Invoke();
                }
            }
            // 2. Điều khiển khi đang giữ phím Đỡ (S / Down): mở khóa Ngồi đánh & 2 Chiêu đặc biệt
            else if (State == FighterState.Block)
            {
                TryTriggerBlockCombos(attackPressed, throwPressed, ultPressed);
            }
            // 3. Điều khiển trên không: Nhảy đánh, Nhảy ném phi tiêu (đã khóa Lướt trên không)
            else if (State == FighterState.Jump)
            {
                if (attackPressed && _cfg.JumpAttack.HasFrames && _attackCooldownTimer <= 0)
                {
                    SetState(FighterState.JumpAttack);
                }
                else if (throwPressed && _throwCooldownTimer <= 0)
                {
                    if (TryUseChakra(ThrowChakraCost))
                    {
                        _consecutiveThrows++;
                        SetState(_cfg.JumpThrow.HasFrames ? FighterState.JumpThrow : FighterState.Throw);
                    }
                    else OnNotEnoughChakra?.Invoke();
                }
                else if (canTeleport && teleportPressed && _teleportCooldownTimer <= 0 && Chakra < TeleportChakraCost)
                {
                    OnNotEnoughChakra?.Invoke();
                }
            }
            // 4. Nối combo đánh thường sang đợt tiếp theo (tối đa 4 đợt)
            else if (State == FighterState.Attack && attackPressed && _attackComboStage < 3)
            {
                GetAttackStageRange(_attackComboStage, out int curStart, out _, out _);
                if (_frame > curStart)
                    _queuedNextAttackStage = true;
            }
            // Báo thiếu Chakra khi bấm chiêu trong lúc đang ở trạng thái khác
            else if ((ultPressed && _ultCooldownTimer <= 0 && Chakra < UltChakraCost) ||
                     (throwPressed && _throwCooldownTimer <= 0 && Chakra < ThrowChakraCost) ||
                     (canTeleport && teleportPressed && _teleportCooldownTimer <= 0 && Chakra < TeleportChakraCost))
            {
                OnNotEnoughChakra?.Invoke();
            }

            // Di chuyển ngang khi đứng, chạy, nhảy hoặc đang ra chiêu trên không (tạm khóa khi đang đứng giữ nhịp nhảy đánh)
            bool canMoveX = State == FighterState.Idle || State == FighterState.Run
                         || (State == FighterState.Jump && _airSuspendTimer <= 0)
                         || (State == FighterState.JumpAttack && _airSuspendTimer <= 0)
                         || State == FighterState.JumpThrow;
            if (canMoveX && dir != 0)
            {
                X = ClampStageX(X + dir * Speed * dt);
                if (State != FighterState.JumpAttack && State != FighterState.JumpThrow)
                    FacingRight = dir > 0;
            }

            bool isAirState = State == FighterState.Jump || State == FighterState.JumpAttack
                           || State == FighterState.JumpThrow || State == FighterState.Dash
                           || State == FighterState.Special2
                           || State == FighterState.Knockdown || State == FighterState.Dead
                           || (State == FighterState.Hurt && _isAirHurt);
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
                case FighterState.Special2: UpdateSpecial2(dt); break;
                case FighterState.Ultimate: UpdateUltimate(dt); break;
                case FighterState.Hurt: UpdateHurt(dt); break;
                case FighterState.Knockdown: UpdateKnockdown(dt); break;
                case FighterState.GetUp: UpdateOneShotClip(_cfg.GetUp, dt); break;
            }

            Draw();
        }

        // [Phát thêm] Tốc biến ra sau lưng đối thủ, thả hiệu ứng 2 đầu và chạy animation dịch chuyển (_cfg.Dash)
        private void PerformTeleport(bool keepUltimate = false)
        {
            if (Opponent == null) return;

            SpawnTeleportEffect();

            double oppCenterX = Opponent.BodyX + Opponent.BodyW / 2;
            double behindOffset = (Opponent.BodyW + BodyW) / 2 + 25;
            double rawTargetX = Opponent.FacingRight
                ? (oppCenterX - behindOffset - BodyW / 2)
                : (oppCenterX + behindOffset - BodyW / 2);

            X = ClampStageX(rawTargetX);

            Y = Math.Min(GroundY - Height + _cfg.FootPadding, Opponent.FeetY - Height + _cfg.FootPadding);
            double newCenterX = BodyX + BodyW / 2;
            FacingRight = oppCenterX >= newCenterX;

            SpawnTeleportEffect();

            if (keepUltimate)
            {
                int chargeFrames = _cfg.UltimateChargeFrames > 0 ? _cfg.UltimateChargeFrames : _cfg.Ultimate.Count / 2;
                if (_frame < chargeFrames)
                {
                    _frame = chargeFrames;
                    _timer = 0;
                }
                _ultDashedDistance = 0;
                return;
            }

            _vy = 0;
            if (_cfg.Dash.HasFrames)
            {
                SetState(FighterState.Dash);
                _isTeleportAnim = true;
            }
            else
            {
                SetState(IsStandingOnSurface() ? FighterState.Idle : FighterState.Jump);
            }
        }

        // [Phát thêm] Sinh hiệu ứng dịch chuyển riêng của nhân vật (hoặc bụi mặc định) tại vị trí hiện tại
        private void SpawnTeleportEffect()
        {
            if (_cfg.DashEffect is { HasFrames: true } fx)
            {
                double dir = FacingRight ? 1 : -1;
                double centerX = X + Width / 2 + fx.OffsetX * dir;
                double targetY = (fx.IsCenterY ? (Y + Height / 2) : FeetY) + fx.OffsetY;
                OnSpawnEffect?.Invoke(new DustEffect(
                    fx.Prefix, fx.Start, fx.Frames, fx.FrameTime,
                    centerX, targetY, fx.Width, fx.Height,
                    fx.IsCenterY, fx.GroundOffsetY, FacingRight, fx.ZIndex,
                    null, FighterState.Idle, 0, 0, false));
            }
            else
            {
                SpawnFootDust();
            }
        }

        private void SpawnFootDust(double scale = 1.0)
        {
            double footCenterX = BodyX + BodyW / 2;
            OnSpawnEffect?.Invoke(new DustEffect(footCenterX, FeetY, false, scale));
        }

        private double ClampStageX(double nextX) =>
            Math.Clamp(nextX, StageMargin - BodyOffsetX, StageWidth - StageMargin - BodyW - BodyOffsetX);

        private bool TryTriggerBlockCombos(bool attackPressed, bool throwPressed, bool ultPressed)
        {
            if (ultPressed && _cfg.Special2.HasFrames && _special2CooldownTimer <= 0)
            {
                if (TryUseChakra(Special2ChakraCost)) { SetState(FighterState.Special2); return true; }
                OnNotEnoughChakra?.Invoke();
                return true;
            }
            if (throwPressed && _cfg.Special1.HasFrames && _special1CooldownTimer <= 0)
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

            double myCenterX = BodyX + BodyW / 2;

            // Đánh thường: đợt 1..3 đánh choáng & xê dịch nhẹ; đợt 4 (frame cuối) hất bay đối thủ lên trời
            if (State == FighterState.Attack)
            {
                GetAttackStageRange(_attackComboStage, out _, out _, out int hitFrame);
                bool isFinisher = _attackComboStage >= 3 && _frame == hitFrame;
                if (_frame != hitFrame || _lastHitFrame == _frame) return;

                if (AttackBox.IntersectsWith(target.Bounds))
                {
                    target.TakeDamage(
                        _cfg.AttackDamage,
                        stunDuration: isFinisher ? 0.8 : 0.55,
                        knockdown: false,
                        sourceX: myCenterX,
                        isUltimate: false,
                        launchUp: isFinisher,
                        lightHit: !isFinisher);
                    _lastHitFrame = _frame;
                }
            }
            // [Phát thêm] Nhảy đánh: nếu đang ở trên không thì 2 bên đứng trên không, mục tiêu lùi nhẹ;
            // nếu đòn kế không trúng thì 0.25s sau cả 2 rơi xuống; trúng tới frame cuối thì mục tiêu rớt nhanh ở tư thế nằm + có bụi
            else if (State == FighterState.JumpAttack)
            {
                int lastFrame = _cfg.JumpAttack.Count - 1;
                bool isFinisher = _frame == lastFrame;
                bool hitFrame = isFinisher || (_frame >= 1 && _frame < lastFrame && (_frame % 2 == 1 || _cfg.JumpAttack.Count <= 3));
                if (!hitFrame || _lastHitFrame == _frame) return;

                if (AttackBox.IntersectsWith(target.Bounds))
                {
                    _lastHitFrame = _frame;
                    bool targetInAir = !target.IsStandingOnSurface() || target._isAirHurt;

                    if (targetInAir)
                    {
                        if (isFinisher)
                        {
                            _airSuspendTimer = 0;
                            target.TakeDamage(
                                _cfg.JumpAttackDamage,
                                stunDuration: KnockdownGroundDuration,
                                knockdown: true,
                                sourceX: myCenterX,
                                airSlam: true);
                        }
                        else
                        {
                            _vy = 0;
                            _airSuspendTimer = AirSuspendDuration;
                            target.TakeDamage(
                                Math.Max(2, _cfg.JumpAttackDamage / 2),
                                stunDuration: AirSuspendDuration + 0.05,
                                knockdown: false,
                                sourceX: myCenterX,
                                airHit: true);
                        }
                    }
                    else
                    {
                        target.TakeDamage(_cfg.JumpAttackDamage, 0.3, isFinisher, myCenterX);
                    }
                }
            }
            // Ngồi đánh / Đánh đặc biệt (S + J / Down + 1)
            else if (State == FighterState.CrouchAttack)
            {
                bool isSasukeCrouch = _cfg.CrouchAttack.Count >= 8
                    && _cfg.CrouchAttack.Prefix.StartsWith("Sasuke/", StringComparison.OrdinalIgnoreCase);

                if (isSasukeCrouch)
                {
                    int finishFrame = _cfg.CrouchAttack.Count - 2; // Frame 7/8 (index 6): nhịp dứt chiêu

                    // Nhịp 1 (frame 3..6): chém trúng và giữ đối thủ đứng choáng tại chỗ chờ đòn dứt chiêu
                    if (!_skillHasHit && _frame >= 2 && _frame < finishFrame)
                    {
                        if (AttackBox.IntersectsWith(target.Bounds))
                        {
                            int firstHitDmg = Math.Max(1, _cfg.CrouchAttackDamage / 2);
                            double holdTime = (finishFrame - _frame + 1) * _cfg.CrouchAttack.FrameTime + 0.1;
                            target.TakeDamage(firstHitDmg, holdTime, false, myCenterX, lightHit: true);
                            _skillHasHit = true;
                            _lastHitFrame = _frame;
                        }
                    }
                    // Nhịp 2 (frame 7): dứt chiêu khiến đối thủ bật lùi ra xa vừa phải
                    else if (_frame == finishFrame && _lastHitFrame != _frame)
                    {
                        if (AttackBox.IntersectsWith(target.Bounds))
                        {
                            int finishDmg = _skillHasHit
                                ? Math.Max(1, _cfg.CrouchAttackDamage - (_cfg.CrouchAttackDamage / 2))
                                : _cfg.CrouchAttackDamage;
                            target.TakeDamage(finishDmg, 0.38, false, myCenterX);
                            if (!target.IsDead && target.State == FighterState.Hurt)
                                target.ApplyKnockback(myCenterX, target.BodyX + target.BodyW / 2, SasukeCrouchAttackKnockbackSpeed);
                            _skillHasHit = true;
                            _lastHitFrame = _frame;
                        }
                    }
                    return;
                }

                if (_skillHasHit || _frame < _cfg.CrouchAttack.Count / 2) return;
                if (AttackBox.IntersectsWith(target.Bounds))
                {
                    target.TakeDamage(_cfg.CrouchAttackDamage, 0.35, false, myCenterX);
                    _skillHasHit = true;
                }
            }
            // Chiêu đặc biệt 1 dạng cận chiến (Naruto danhdacbiet; Sasuke phunlua bắn đạn riêng)
            else if (State == FighterState.Special1 && string.IsNullOrEmpty(_cfg.Special1ProjectilePrefix))
            {
                if (_skillHasHit || _frame < _cfg.Special1.Count / 3) return;
                if (SkillBox.IntersectsWith(target.Bounds))
                {
                    target.TakeDamage(_cfg.Special1Damage, 0.45, true, myCenterX);
                    _skillHasHit = true;
                }
            }
            // Chiêu đặc biệt 2 (S + I): nếu là chuỗi đặc biệt (Naruto 15f / Sasuke 23f) thì xử lý trong UpdateSpecial2; ngược lại đánh 1 nhịp chuẩn
            else if (State == FighterState.Special2 && _sp2Phase == 0 && _sasukeSp2Phase == 0)
            {
                if (_skillHasHit || _frame < _cfg.Special2.Count / 3) return;
                if (SkillBox.IntersectsWith(target.Bounds))
                {
                    target.TakeDamage(_cfg.Special2Damage, 0.5, true, myCenterX);
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
                    target.TakeDamage(_cfg.UltimateDamage, remainingUltTime, true, myCenterX, isUltimate: true);
                    _skillHasHit = true;
                }
            }
        }

        // [Phát thêm] Xử lý đỡ đòn, xê dịch nhẹ khi trúng đòn thường, hất bay lên trời và giữ/đập từ trên không
        public void TakeDamage(
            int amount, double stunDuration = 0.3, bool knockdown = false,
            double? sourceX = null, bool isUltimate = false, bool launchUp = false, bool lightHit = false,
            bool airHit = false, bool airSlam = false)
        {
            if (IsDead) return;

            double myCenterX = BodyX + BodyW / 2;
            double? attackerCenterX = sourceX ?? (Opponent != null ? Opponent.BodyX + Opponent.BodyW / 2 : null);

            bool isHitFromBehind = attackerCenterX.HasValue &&
                ((FacingRight && attackerCenterX.Value < myCenterX) ||
                 (!FacingRight && attackerCenterX.Value > myCenterX));

            // Đỡ thành công khi đang giữ thế đỡ, còn điểm chống đỡ và đòn đánh đến từ phía trước mặt
            if (State == FighterState.Block && Guard > 0 && !isHitFromBehind)
            {
                _guardRegenTimer = GuardRegenDelay;
                _guardVisibleTimer = GuardVisibleDuration;

                if (Guard >= amount)
                {
                    Guard -= amount;
                    if (Guard <= 0)
                    {
                        Guard = 0;
                        ApplyKnockback(attackerCenterX, myCenterX, HitKnockbackSpeed * 0.8);
                        SetState(FighterState.Hurt);
                        _hurtDuration = 0.25;
                    }
                    return;
                }
                else
                {
                    int remainingDamage = (int)Math.Ceiling(amount - Guard);
                    Guard = 0;
                    amount = remainingDamage;
                }
            }

            Hp = Math.Max(0, Hp - amount);
            if (Hp == 0)
            {
                _isComboLocked = false;
                ApplyKnockback(attackerCenterX, myCenterX, UltKnockbackSpeed);
                SetState(FighterState.Dead);
                return;
            }

            // Chống ngắt chiêu giữa chừng khi nhân vật đang thi triển Chiêu cuối hoặc đã vào nhịp combo S + I
            if (State == FighterState.Ultimate || (State == FighterState.Special2 && (_sp2Phase > 1 || _sasukeSp2Phase > 1))) return;

            // [Phát thêm] Frame cuối của nhảy đánh trên không / cú đạp cuối S + I: rớt nhanh xuống đất ở tư thế nằm và bung bụi khi chạm đất
            if (airSlam)
            {
                _isComboLocked = false;
                ApplyKnockback(attackerCenterX, myCenterX, 40);
                if (_cfg.Knockdown.HasFrames)
                {
                    SetState(FighterState.Knockdown);
                    _frame = Math.Min(2, _cfg.Knockdown.Count - 1);
                    _hurtDuration = Math.Max(KnockdownGroundDuration, stunDuration);
                }
                else
                {
                    SetState(FighterState.Hurt);
                    _hurtDuration = stunDuration;
                }
                _vy = AirSlamDownVelocity;
                _slamDustOnLand = true;
                return;
            }

            // Khi trúng Chiêu cuối: đứng choáng hứng trọn chiêu, đến frame cuối mới bị đẩy lùi 2 frame rồi nằm xuống
            bool isUltimateHit = isUltimate && stunDuration > 0.35;
            double knockSpeed = launchUp ? LaunchKnockbackSpeed
                              : airHit ? AirHitKnockbackSpeed
                              : (isUltimate || knockdown) ? UltKnockbackSpeed
                              : lightHit ? LightHitKnockbackSpeed
                              : HitKnockbackSpeed;
            double delayBeforeKnock = isUltimateHit ? Math.Max(0, stunDuration - 0.12) : 0;
            ApplyKnockback(attackerCenterX, myCenterX, knockSpeed, delayBeforeKnock, triggerKnockdownAfterDelay: isUltimateHit && knockdown);

            if (knockdown && _cfg.Knockdown.HasFrames && delayBeforeKnock <= 0)
            {
                SetState(FighterState.Knockdown);
                _hurtDuration = Math.Max(KnockdownGroundDuration, stunDuration);
            }
            else
            {
                bool keepAirborne = launchUp || airHit || !IsStandingOnSurface() || _isAirHurt;
                SetState(FighterState.Hurt);
                _hurtDuration = stunDuration;

                if (airHit)
                {
                    _isAirHurt = true;
                    _hasHeldAtApex = true;
                    _airHurtHoldTimer = 0;
                    _airSuspendTimer = AirSuspendDuration;
                    _vy = 0; // Đứng trên không trong 0.25s chờ nhịp đánh tiếp theo
                }
                else if (launchUp)
                {
                    _isAirHurt = true;
                    _hasHeldAtApex = false;
                    _airHurtHoldTimer = 0;
                    _airSuspendTimer = 0;
                    _vy = LaunchUpVelocity; // Hất tung bay cao lên trời
                }
                else if (keepAirborne)
                {
                    _isAirHurt = true;
                    _hasHeldAtApex = false;
                    _airHurtHoldTimer = 0;
                    _airSuspendTimer = 0;
                    _vy = Math.Min(_vy, -220);
                }
            }
        }

        private void ApplyKnockback(double? attackerCenterX, double myCenterX, double speed, double delay = 0, bool triggerKnockdownAfterDelay = false)
        {
            double velocity;
            if (attackerCenterX.HasValue && Math.Abs(attackerCenterX.Value - myCenterX) > 1)
            {
                bool attackerOnRight = attackerCenterX.Value > myCenterX;
                FacingRight = attackerOnRight;                 // Tự động xoay mặt về phía đối phương
                velocity = attackerOnRight ? -speed : speed;   // Bật lùi về phía sau lưng
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
                _pendingKnockdownAfterDelay = triggerKnockdownAfterDelay;
            }
            else
            {
                _knockbackVelocity = velocity;
                _pendingKnockbackVelocity = 0;
                _knockbackDelayTimer = 0;
                _pendingKnockdownAfterDelay = false;
            }
        }

        private void UpdateKnockback(double dt)
        {
            if (_knockbackDelayTimer > 0)
            {
                _knockbackDelayTimer -= dt;
                if (_knockbackDelayTimer <= 0)
                {
                    _knockbackDelayTimer = 0;
                    double vel = _pendingKnockbackVelocity;
                    bool shouldKnockdown = _pendingKnockdownAfterDelay;
                    _pendingKnockbackVelocity = 0;
                    _pendingKnockdownAfterDelay = false;

                    if (shouldKnockdown && _cfg.Knockdown.HasFrames)
                    {
                        SetState(FighterState.Knockdown);
                        _hurtDuration = KnockdownGroundDuration;
                    }
                    _knockbackVelocity = vel;
                }
                return;
            }

            // [Phát thêm] Khi đang dừng lơ lửng tại đỉnh quỹ đạo thì tạm dừng luôn chuyển động ngang
            if (State == FighterState.Hurt && _isAirHurt && _airHurtHoldTimer > 0)
                return;

            // [Phát thêm] Khi ngã/gục sang frame thứ 3 (_frame >= 2) thì dừng trượt ngang để nhân vật nằm yên trên mặt đất
            if ((State == FighterState.Knockdown || State == FighterState.Dead) && _frame >= 2)
            {
                _knockbackVelocity = 0;
                return;
            }

            if (Math.Abs(_knockbackVelocity) <= 0.1)
            {
                _knockbackVelocity = 0;
                return;
            }

            X = ClampStageX(X + _knockbackVelocity * dt);

            // Giảm ma sát ở 2 frame đầu của Knockdown và khi đang bay Hurt trên không để tạo vòng cung mượt
            bool lowFriction = (State == FighterState.Knockdown && _frame < 2) || (State == FighterState.Hurt && _isAirHurt && _airSuspendTimer <= 0);
            double friction = lowFriction ? KnockbackFriction * 0.35 : KnockbackFriction;
            double decel = friction * dt;
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

            var clip = State == FighterState.Run ? _cfg.Run : _cfg.Idle;
            SetClipImage(clip, _frame);

            if (State == FighterState.Run)
                AdvanceRun(clip, dt);
            else
                Advance(clip.Count, clip.FrameTime, true, dt);
        }

        // [Phát thêm] Riêng Sasuke: tua nhanh đoạn lấy đà frame 1 -> 6, sau đó chỉ loop vòng chạy từ frame 6 -> 11
        private void AdvanceRun(AnimClip clip, double dt)
        {
            if (clip.Count == 11 && clip.Prefix.StartsWith("Sasuke/", StringComparison.OrdinalIgnoreCase))
            {
                const int loopStartIdx = 5;            // Frame 6 (sasukechay6.png -> index 5)
                const double startupFrameTime = 0.024; // Tua nhanh các frame khởi động 1 -> 5 (tổng ~0.12s)

                _timer += dt;
                while (true)
                {
                    double stepTime = _frame < loopStartIdx ? startupFrameTime : clip.FrameTime;
                    if (_timer < stepTime) break;

                    _timer -= stepTime;
                    _frame++;
                    if (_frame >= clip.Count)
                        _frame = loopStartIdx;
                }
                return;
            }

            Advance(clip.Count, clip.FrameTime, true, dt);
        }

        private void UpdateJump(double dt)
        {
            // [Phát thêm] Giữ đứng trên không nếu vẫn còn thời gian chờ nhịp đánh tiếp theo
            if (_airSuspendTimer > 0)
            {
                _vy = 0;
                SetClipImage(_cfg.Jump, _cfg.Jump.Count / 2);
                return;
            }

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
            // [Phát thêm] Đứng trên không 0.25s khi đánh trúng mục tiêu bằng nhảy đánh
            if (_airSuspendTimer > 0)
            {
                _vy = 0;
            }
            else
            {
                _vy += Gravity * dt;
                Y += _vy * dt;
                if (FeetY >= GroundY)
                {
                    Y = GroundY - Height + _cfg.FootPadding;
                    _vy = 0;
                }
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
            // [Phát thêm] Nếu vừa tốc biến bằng phím O/6 thì đứng tại chỗ chạy hết các frame dịch chuyển
            if (_isTeleportAnim)
            {
                SetClipImage(_cfg.Dash, _frame);
                if (Advance(_cfg.Dash.Count, _cfg.Dash.FrameTime, false, dt))
                {
                    _isTeleportAnim = false;
                    if (IsStandingOnSurface()) SetState(FighterState.Idle);
                    else { _vy = 0; SetState(FighterState.Jump); }
                }
                return;
            }

            double dashDir = FacingRight ? 1 : -1;
            double step = Math.Min(DashSpeed * dt, DashDistance - _dashedDistance);
            X = ClampStageX(X + dashDir * step);
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
            // [Phát thêm] Cho phép bấm trái/phải để xoay hướng đỡ khi đối thủ tốc biến ra sau lưng
            int dir = (InputManager.IsKeyDown(_right) ? 1 : 0) - (InputManager.IsKeyDown(_left) ? 1 : 0);
            if (dir != 0) FacingRight = dir > 0;

            SetClipImage(_cfg.Block, _frame);
            if (_cfg.Block.Count > 1)
                Advance(_cfg.Block.Count, _cfg.Block.FrameTime, false, dt);

            if (!InputManager.IsKeyDown(_block) || Guard <= 0)
                SetState(FighterState.Idle);
        }

        // [Phát thêm] Chia chuỗi frame đánh thường thành 4 đợt:
        // Đợt 1 (f1..5) -> Đợt 2 (f6..9) -> Đợt 3 (f10..13) -> Đợt 4 (f14..cuối: hất tung lên trời)
        private void GetAttackStageRange(int stage, out int startFrame, out int endFrameExclusive, out int hitFrame)
        {
            int total = _cfg.Attack.Count;
            int s1 = Math.Clamp(_cfg.ComboSplitFrame, 1, Math.Max(1, total - 3));
            int remaining = Math.Max(3, total - s1);
            int step = Math.Max(1, remaining / 3);
            int s2 = Math.Min(total - 2, s1 + step);
            int s3 = Math.Min(total - 1, s2 + step);

            switch (stage)
            {
                case 0:
                    startFrame = 0;
                    endFrameExclusive = s1;
                    hitFrame = Math.Min(endFrameExclusive - 1, 2);
                    break;
                case 1:
                    startFrame = s1;
                    endFrameExclusive = s2;
                    hitFrame = Math.Min(endFrameExclusive - 1, startFrame + 2);
                    break;
                case 2:
                    startFrame = s2;
                    endFrameExclusive = s3;
                    hitFrame = Math.Min(endFrameExclusive - 1, startFrame + 2);
                    break;
                default:
                    startFrame = s3;
                    endFrameExclusive = total;
                    hitFrame = total - 1;
                    break;
            }
        }

        private void UpdateAttack(double dt)
        {
            // [Phát thêm] Nhúc nhích tiến lên phía trước một chút ở mỗi tư thế (frame) đánh
            if (_frame != _lastAttackSteppedFrame)
            {
                _lastAttackSteppedFrame = _frame;
                double stepDir = FacingRight ? 1 : -1;
                X = ClampStageX(X + stepDir * AttackStepDistance);
            }

            GetAttackStageRange(_attackComboStage, out _, out int endFrameExclusive, out _);
            SetClipImage(_cfg.Attack, _frame);

            _timer += dt;
            while (_timer >= _cfg.Attack.FrameTime)
            {
                _timer -= _cfg.Attack.FrameTime;
                _frame++;

                if (_frame >= endFrameExclusive)
                {
                    // Nếu đã bấm J nối tiếp trong lúc đang ra đòn -> chuyển sang đúng đợt đánh kế tiếp
                    if (_queuedNextAttackStage && _attackComboStage < 3)
                    {
                        _attackComboStage++;
                        _queuedNextAttackStage = false;
                        _lastHitFrame = -1;
                        _lastAttackSteppedFrame = -1;
                        GetAttackStageRange(_attackComboStage, out int nextStart, out _, out _);
                        _frame = nextStart;
                        SetClipImage(_cfg.Attack, _frame);
                        return;
                    }

                    // Kết thúc đợt hiện tại: xong đợt 4 thì reset chuỗi & áp dụng cooldown; chưa hết thì mở cửa sổ 0.5s
                    if (_attackComboStage >= 3)
                    {
                        _attackComboStage = 0;
                        _attackComboResetTimer = 0;
                        _attackCooldownTimer = AttackCooldown;
                    }
                    else
                    {
                        _attackComboStage++;
                        _attackComboResetTimer = AttackComboResetTime;
                    }

                    SetState(FighterState.Idle);
                    return;
                }
            }
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

        // [Phát thêm] Điều hướng chiêu kết hợp S + I (Special2):
        // - Sasuke (23 frames): UpdateSasukeSpecial2 (hất tung -> tốc biến frame 6 lên không -> 3 nhịp đánh ngắt 0.1s -> rơi 1 nửa tốc biến frame 6 sau lưng -> đập xuống đất + đợi 0.2s rơi)
        // - Naruto (15 frames): chuỗi liên hoàn nhảy đuổi & xoay frame 11
        private void UpdateSpecial2(double dt)
        {
            var clip = _cfg.Special2;
            if (!clip.HasFrames) { SetState(FighterState.Idle); return; }

            if (_sasukeSp2Phase > 0)
            {
                UpdateSasukeSpecial2(dt);
                return;
            }

            if (_sp2Phase == 0)
            {
                UpdateOneShotClip(clip, dt);
                return;
            }

            double dir = FacingRight ? 1 : -1;
            double myCenterX = BodyX + BodyW / 2;

            // --- PHASE 1: Frame 1 -> 5 (index 0..4) — Lao tới đánh liên hoàn & khóa mục tiêu ---
            if (_sp2Phase == 1)
            {
                if (_frame != _lastAttackSteppedFrame)
                {
                    _lastAttackSteppedFrame = _frame;
                    X = ClampStageX(X + dir * 4);
                }

                if ((_frame == 1 || _frame == 3) && _lastHitFrame != _frame && Opponent != null && !Opponent.IsDead)
                {
                    Rect reachBox = FacingRight
                        ? new Rect(BodyX + BodyW - 6, BodyTop - 10, Math.Max(_cfg.AttackReach + 18, _cfg.UltimateReach), BodyH + 20)
                        : new Rect(BodyX - Math.Max(_cfg.AttackReach + 18, _cfg.UltimateReach), BodyTop - 10, Math.Max(_cfg.AttackReach + 18, _cfg.UltimateReach) + 6, BodyH + 20);

                    if (reachBox.IntersectsWith(Opponent.Bounds))
                    {
                        _lastHitFrame = _frame;
                        _skillHasHit = true;

                        int hitDmg = Math.Max(3, _cfg.Special2Damage / 6);
                        Opponent.TakeDamage(hitDmg, stunDuration: 0.55, knockdown: false, sourceX: myCenterX, lightHit: true);
                        _sp2DealtDamage += hitDmg;

                        if (!Opponent.IsDead)
                        {
                            Opponent._isComboLocked = true;
                            Opponent._comboLockTimer = 3.4;
                            Opponent._knockbackVelocity = 0;

                            double lockTargetX = FacingRight
                                ? (BodyX + BodyW + 8 - Opponent.BodyOffsetX)
                                : (BodyX - 8 - Opponent.BodyW - Opponent.BodyOffsetX);
                            Opponent.X = Opponent.ClampStageX(lockTargetX);
                        }
                    }
                }

                SetClipImage(clip, _frame);
                _timer += dt;
                while (_timer >= clip.FrameTime)
                {
                    _timer -= clip.FrameTime;
                    _frame++;
                    if (_frame >= 5)
                    {
                        if (!_skillHasHit || Opponent == null || Opponent.IsDead)
                        {
                            if (Opponent != null) Opponent._isComboLocked = false;
                            SetState(IsStandingOnSurface() ? FighterState.Idle : FighterState.Jump);
                            return;
                        }

                        _frame = 5;
                        _sp2Phase = 2;
                        _lastHitFrame = -1;
                        break;
                    }
                }
                return;
            }

            // --- PHASE 2: Frame 6 -> 10 (index 5..9) — Đá bay đối thủ lên trời bằng LaunchUpVelocity ---
            if (_sp2Phase == 2)
            {
                if (Opponent != null && !Opponent.IsDead)
                {
                    Opponent._isComboLocked = true;
                    Opponent._comboLockTimer = 3.0;

                    if (_frame == 7 && !_sp2HasLaunched)
                    {
                        _sp2HasLaunched = true;
                        int launchDmg = Math.Max(4, _cfg.Special2Damage / 5);
                        Opponent.TakeDamage(launchDmg, stunDuration: 1.6, knockdown: false, sourceX: myCenterX, launchUp: true);
                        _sp2DealtDamage += launchDmg;

                        if (!Opponent.IsDead)
                        {
                            Opponent._isComboLocked = true;
                            Opponent._comboLockTimer = 3.0;
                            Opponent._knockbackVelocity = dir * 45;
                        }
                    }
                }

                SetClipImage(clip, _frame);
                _timer += dt;
                while (_timer >= clip.FrameTime)
                {
                    _timer -= clip.FrameTime;
                    _frame++;
                    if (_frame >= 10)
                    {
                        if (Opponent == null || Opponent.IsDead)
                        {
                            SetState(IsStandingOnSurface() ? FighterState.Idle : FighterState.Jump);
                            return;
                        }

                        _sp2Phase = 3;
                        _vy = Sp2ChaseUpVelocity;
                        _timer = 0;
                        SpawnFootDust();
                        return;
                    }
                }
                return;
            }

            // --- PHASE 3: Naruto dùng frame nhảy đuổi theo lên không (nhảy sau nhưng nhanh hơn) ---
            if (_sp2Phase == 3)
            {
                if (Opponent == null || Opponent.IsDead)
                {
                    _sp2Phase = 5;
                    _frame = 13;
                    _vy = 650;
                    return;
                }

                Opponent._isComboLocked = true;
                Opponent._comboLockTimer = 2.5;

                _vy += Gravity * 0.45 * dt;
                Y = Math.Max(15, Y + _vy * dt);

                double desiredX = FacingRight
                    ? (Opponent.BodyX - BodyW - 4 - BodyOffsetX)
                    : (Opponent.BodyX + Opponent.BodyW + 4 - BodyOffsetX);
                X = ClampStageX(X + (desiredX - X) * Math.Min(1.0, 16.0 * dt));

                var jumpClip = _cfg.Jump.HasFrames ? _cfg.Jump : clip;
                int jumpIdx = Math.Min(jumpClip.Count - 1, Math.Max(0, jumpClip.Count / 2 - 1));
                SetClipImage(jumpClip, jumpIdx);

                if (Y <= Opponent.Y || _vy >= 0)
                {
                    Y = Math.Max(15, Opponent.Y);
                    _vy = Sp2ExtraRiseSpeed;
                    _sp2Phase = 4;
                    _sp2ExtraRisen = 0;
                    _sp2SpinCount = 0;
                    _sp2SpinTimer = 0;
                    _frame = 10;

                    Opponent._vy = 0;
                    Opponent._knockbackVelocity = 0;
                    Opponent._airSuspendTimer = 1.0;
                    Opponent._hasHeldAtApex = true;
                }
                return;
            }

            // --- PHASE 4: Bay thêm một đoạn ngắn & lặp lại frame 11 nhiều lần tạo hiệu ứng xoay ---
            if (_sp2Phase == 4)
            {
                if (Opponent == null || Opponent.IsDead)
                {
                    _spinRotate.Angle = 0;
                    _sp2Phase = 5;
                    _frame = 13;
                    _vy = 650;
                    return;
                }

                Opponent._isComboLocked = true;
                Opponent._comboLockTimer = 2.0;
                Opponent._vy = 0;
                Opponent._airSuspendTimer = 1.0;

                if (_sp2ExtraRisen < Sp2ExtraRiseDistance)
                {
                    double step = Math.Min(Math.Abs(Sp2ExtraRiseSpeed) * dt, Sp2ExtraRiseDistance - _sp2ExtraRisen);
                    Y = Math.Max(15, Y - step);
                    _sp2ExtraRisen += step;
                }

                double hoverX = FacingRight
                    ? (Opponent.BodyX - BodyW * 0.45 - BodyOffsetX)
                    : (Opponent.BodyX + Opponent.BodyW - BodyW * 0.55 - BodyOffsetX);
                X = ClampStageX(X + (hoverX - X) * Math.Min(1.0, 18.0 * dt));

                SetClipImage(clip, 10);
                _sp2SpinTimer += dt;
                while (_sp2SpinTimer >= Sp2SpinStepTime)
                {
                    _sp2SpinTimer -= Sp2SpinStepTime;
                    _sp2SpinCount++;
                    _spinRotate.Angle = (_sp2SpinCount * 90) % 360;

                    if (_sp2SpinCount >= Sp2SpinRepeats)
                    {
                        _spinRotate.Angle = 0;
                        _sp2Phase = 5;
                        _frame = 11;
                        _timer = 0;
                        _vy = 0;
                        return;
                    }
                }
                return;
            }

            // --- PHASE 5: Frame 12 -> 15 (index 11..14) — Đạp đối thủ xuống đất, đứng trên không rồi rơi ---
            _spinRotate.Angle = 0;

            if (_sp2HoveringAfterClip)
            {
                if (_sp2AirHoverTimer > 0)
                {
                    _sp2AirHoverTimer = Math.Max(0, _sp2AirHoverTimer - dt);
                    _vy = 0;
                    SetClipImage(clip, clip.Count - 1);
                    return;
                }

                _vy += Gravity * dt;
                Y += _vy * dt;
                if (_cfg.Jump.HasFrames)
                    SetClipImage(_cfg.Jump, _cfg.Jump.Count - 1);

                if (FeetY >= GroundY)
                {
                    Y = GroundY - Height + _cfg.FootPadding;
                    _vy = 0;
                    SetState(FighterState.Idle);
                }
                return;
            }

            _vy = 0;
            SetClipImage(clip, _frame);

            if (_frame == 12 && !_sp2HasSlammed)
            {
                _sp2HasSlammed = true;
                if (Opponent != null && !Opponent.IsDead)
                {
                    Opponent._isComboLocked = false;
                    Opponent._airSuspendTimer = 0;

                    int slamDmg = Math.Max(10, _cfg.Special2Damage - _sp2DealtDamage);
                    Opponent.TakeDamage(
                        slamDmg,
                        stunDuration: KnockdownGroundDuration,
                        knockdown: true,
                        sourceX: myCenterX,
                        airSlam: true);
                }
            }

            if (Advance(clip.Count, clip.FrameTime, false, dt))
            {
                _sp2HoveringAfterClip = true;
                _sp2AirHoverTimer = Sp2AirHoverDuration;
            }
        }

        // [Phát thêm] Chiêu liên hoàn S + I của Sasuke (Special2 - 23 frames: sasukedacbiet1 -> sasukedacbiet23):
        // Sát thương toàn chuỗi tự động chia tỷ lệ từ _cfg.Special2Damage trong Sasuke.cs
        private void UpdateSasukeSpecial2(double dt)
        {
            var clip = _cfg.Special2;
            double dir = FacingRight ? 1 : -1;
            double myCenterX = BodyX + BodyW / 2;

            // --- PHASE 1: Frame 1 -> 5 (index 0..4) — Đá hất bay đối thủ lên trời cao hơn ---
            if (_sasukeSp2Phase == 1)
            {
                if (_frame != _lastAttackSteppedFrame && _frame < 4)
                {
                    _lastAttackSteppedFrame = _frame;
                    X = ClampStageX(X + dir * 4);
                }

                // Tại frame 4 (index 3): tung cú đá hất đối thủ bay vút lên cao
                if (_frame == 3 && !_skillHasHit && Opponent != null && !Opponent.IsDead)
                {
                    Rect reachBox = FacingRight
                        ? new Rect(BodyX + BodyW - 6, BodyTop - 10, Math.Max(_cfg.AttackReach + 20, _cfg.UltimateReach), BodyH + 20)
                        : new Rect(BodyX - Math.Max(_cfg.AttackReach + 20, _cfg.UltimateReach), BodyTop - 10, Math.Max(_cfg.AttackReach + 20, _cfg.UltimateReach) + 6, BodyH + 20);

                    if (reachBox.IntersectsWith(Opponent.Bounds))
                    {
                        _skillHasHit = true;
                        int launchDmg = Math.Max(2, (int)Math.Round(_cfg.Special2Damage * 0.07));
                        Opponent.TakeDamage(launchDmg, stunDuration: 2.2, knockdown: false, sourceX: myCenterX, launchUp: true);
                        _sp2DealtDamage += launchDmg;

                        if (!Opponent.IsDead)
                        {
                            Opponent._isComboLocked = true;
                            Opponent._comboLockTimer = 4.5;
                            Opponent._vy = -1080;                  // Bay lên cao hơn mức mặc định (-920)
                            Opponent._knockbackVelocity = dir * 35;
                        }
                    }
                }

                SetClipImage(clip, _frame);
                _timer += dt;
                while (_timer >= clip.FrameTime)
                {
                    _timer -= clip.FrameTime;
                    _frame++;
                    if (_frame >= 5)
                    {
                        if (!_skillHasHit || Opponent == null || Opponent.IsDead)
                        {
                            if (Opponent != null) Opponent._isComboLocked = false;
                            SetState(IsStandingOnSurface() ? FighterState.Idle : FighterState.Jump);
                            return;
                        }

                        // Đợi đối thủ bay lên cao hơn rồi mới tốc biến bằng frame 6
                        _frame = 4;
                        _sasukeSp2Phase = 2;
                        _sasukeSp2SubStep = 0;
                        _sasukeSp2Timer = 0.23;
                        return;
                    }
                }
                return;
            }

            // --- PHASE 2: Tốc biến bằng Frame 6 (index 5) từ vị trí cũ lên trên không cao hơn đối thủ một chút ---
            if (_sasukeSp2Phase == 2)
            {
                if (Opponent == null || Opponent.IsDead)
                {
                    SetState(IsStandingOnSurface() ? FighterState.Idle : FighterState.Jump);
                    return;
                }

                Opponent._isComboLocked = true;
                Opponent._comboLockTimer = 4.0;

                if (_sasukeSp2SubStep == 0)
                {
                    SetClipImage(clip, 4);
                    _sasukeSp2Timer -= dt;
                    if (_sasukeSp2Timer <= 0)
                    {
                        Opponent._vy = 0;
                        Opponent._knockbackVelocity = 0;
                        Opponent._airSuspendTimer = 2.5;
                        Opponent._hasHeldAtApex = true;

                        _frame = 5;
                        SetClipImage(clip, 5);
                        SpawnSasukeTeleportBlur(X + Width / 2, FeetY, 0.15);

                        _sasukeSp2SubStep = 1;
                        _sasukeSp2Timer = 0.075;
                    }
                    return;
                }

                if (_sasukeSp2SubStep == 1)
                {
                    SetClipImage(clip, 5);
                    _sasukeSp2Timer -= dt;
                    if (_sasukeSp2Timer <= 0)
                    {
                        double oppCenterX = Opponent.BodyX + Opponent.BodyW / 2;
                        double inFrontX = FacingRight
                            ? (Opponent.BodyX - BodyW - 6 - BodyOffsetX)
                            : (Opponent.BodyX + Opponent.BodyW + 6 - BodyOffsetX);
                        X = ClampStageX(inFrontX);
                        Y = Math.Max(15, Opponent.Y - 26); // Cao hơn đối thủ một chút
                        FacingRight = oppCenterX >= (BodyX + BodyW / 2);
                        _vy = 0;

                        _frame = 5;
                        SetClipImage(clip, 5);

                        _sasukeSp2SubStep = 2;
                        _sasukeSp2Timer = 0.085;
                    }
                    return;
                }

                SetClipImage(clip, 5);
                _sasukeSp2Timer -= dt;
                if (_sasukeSp2Timer <= 0)
                {
                    _sasukeSp2Phase = 3;
                    _frame = 6;
                    _timer = 0;
                    _lastHitFrame = -1;
                    _sasukeSp2HitPauseTimer = 0;
                }
                return;
            }

            // --- PHASE 3, 4, 5: Chuỗi 3 đòn trên không (f7-11 | f12-15 | f16-18), ngắt 0.1s mỗi lần mất máu ---
            if (_sasukeSp2Phase >= 3 && _sasukeSp2Phase <= 5)
            {
                if (Opponent == null || Opponent.IsDead)
                {
                    _sasukeSp2Phase = 7;
                    _frame = 18;
                    _timer = 0;
                    return;
                }

                _vy = 0;
                Opponent._isComboLocked = true;
                Opponent._comboLockTimer = 3.5;
                Opponent._vy = 0;
                Opponent._airSuspendTimer = 2.5;

                if (_sasukeSp2HitPauseTimer > 0)
                {
                    _sasukeSp2HitPauseTimer -= dt;
                    SetClipImage(clip, _frame);
                    if (_sasukeSp2HitPauseTimer <= 0)
                    {
                        _sasukeSp2HitPauseTimer = 0;
                        if (_sasukeSp2Phase == 3)
                        {
                            _sasukeSp2Phase = 4;
                            _frame = 11;
                            _timer = 0;
                            _lastHitFrame = -1;
                        }
                        else if (_sasukeSp2Phase == 4)
                        {
                            _sasukeSp2Phase = 5;
                            _frame = 15;
                            _timer = 0;
                            _lastHitFrame = -1;
                        }
                        else
                        {
                            Opponent._airSuspendTimer = 0;
                            Opponent._airHurtHoldTimer = 0;
                            Opponent._hasHeldAtApex = true;
                            Opponent._vy = 240;

                            double fallStartFeetY = Opponent.FeetY;
                            _sasukeSp2HalfwayFeetY = fallStartFeetY + Math.Max(35, (GroundY - fallStartFeetY) * 0.5);

                            _sasukeSp2Phase = 6;
                            _sasukeSp2SubStep = 0;
                            _frame = 17;
                        }
                    }
                    return;
                }

                int hitFrameIdx = _sasukeSp2Phase == 3 ? 9 : (_sasukeSp2Phase == 4 ? 13 : 16);
                double ratio = _sasukeSp2Phase == 4 ? 0.10 : 0.12;
                int hitDamage = Math.Max(3, (int)Math.Round(_cfg.Special2Damage * ratio));
                int endFrameExclusive = _sasukeSp2Phase == 3 ? 11 : (_sasukeSp2Phase == 4 ? 15 : 18);

                if (_frame == hitFrameIdx && _lastHitFrame != _frame)
                {
                    _lastHitFrame = _frame;
                    Opponent.TakeDamage(hitDamage, stunDuration: 0.6, knockdown: false, sourceX: myCenterX, airHit: true);
                    _sp2DealtDamage += hitDamage;

                    if (!Opponent.IsDead)
                    {
                        Opponent._isComboLocked = true;
                        Opponent._comboLockTimer = 3.5;
                        Opponent._vy = 0;
                        Opponent._airSuspendTimer = 2.5;
                        Opponent._knockbackVelocity = 0;
                        Opponent.X = Opponent.ClampStageX(Opponent.X + dir * 6);
                    }
                }

                SetClipImage(clip, _frame);
                _timer += dt;
                while (_timer >= 0.06)
                {
                    _timer -= 0.06;
                    _frame++;
                    if (_frame >= endFrameExclusive)
                    {
                        _frame = endFrameExclusive - 1;
                        _sasukeSp2HitPauseTimer = SasukeSp2HitPauseDuration;
                        break;
                    }
                }
                return;
            }

            // --- PHASE 6: Đối thủ rơi xuống 1 nửa -> Sasuke dùng Frame 6 tốc biến ra sau lưng đối thủ ---
            if (_sasukeSp2Phase == 6)
            {
                if (Opponent == null || Opponent.IsDead)
                {
                    _sasukeSp2Phase = 7;
                    _frame = 18;
                    _timer = 0;
                    return;
                }

                _vy = 0;
                Opponent._isComboLocked = true;
                Opponent._comboLockTimer = 2.5;

                if (_sasukeSp2SubStep == 0)
                {
                    SetClipImage(clip, 17);
                    if (Opponent.FeetY >= _sasukeSp2HalfwayFeetY || Opponent.IsStandingOnSurface())
                    {
                        Opponent._vy = 0;
                        Opponent._airSuspendTimer = 2.0;

                        _frame = 5;
                        SetClipImage(clip, 5);
                        SpawnSasukeTeleportBlur(X + Width / 2, FeetY, 0.15);

                        _sasukeSp2SubStep = 1;
                        _sasukeSp2Timer = 0.075;
                    }
                    return;
                }

                if (_sasukeSp2SubStep == 1)
                {
                    SetClipImage(clip, 5);
                    _sasukeSp2Timer -= dt;
                    if (_sasukeSp2Timer <= 0)
                    {
                        double oppCenterX = Opponent.BodyX + Opponent.BodyW / 2;
                        double behindOffset = (Opponent.BodyW + BodyW) / 2 + 12;
                        double behindX = Opponent.FacingRight
                            ? (oppCenterX - behindOffset - BodyW / 2)
                            : (oppCenterX + behindOffset - BodyW / 2);

                        X = ClampStageX(behindX);
                        Y = Math.Max(15, Opponent.Y - 18);
                        FacingRight = oppCenterX >= (BodyX + BodyW / 2); // Frame 6 hướng mặt vào lưng đối thủ

                        _frame = 5;
                        SetClipImage(clip, 5);

                        _sasukeSp2SubStep = 2;
                        _sasukeSp2Timer = 0.085;
                    }
                    return;
                }

                // Bắt đầu frame 19 -> 23: đảo dấu FacingRight vì ảnh frame 19..23 được vẽ quay sang trái
                SetClipImage(clip, 5);
                _sasukeSp2Timer -= dt;
                if (_sasukeSp2Timer <= 0)
                {
                    double oppCenterX = Opponent.BodyX + Opponent.BodyW / 2;
                    bool opponentOnRight = oppCenterX >= (BodyX + BodyW / 2);
                    FacingRight = !opponentOnRight;

                    _sasukeSp2Phase = 7;
                    _frame = 18;
                    _timer = 0;
                    _sasukeSp2HasSlammed = false;
                    _sasukeSp2HoverAfterClip = false;
                }
                return;
            }

            // --- PHASE 7: Frame 19 -> 23 (index 18..22) — Đập đối thủ rơi nhanh xuống đất (khói bụi), Sasuke đợi 0.2s sau mới rơi ---
            if (_sasukeSp2HoverAfterClip)
            {
                if (_sasukeSp2Timer > 0)
                {
                    _sasukeSp2Timer = Math.Max(0, _sasukeSp2Timer - dt);
                    _vy = 0;
                    SetClipImage(clip, clip.Count - 1);
                    return;
                }

                _vy += Gravity * dt;
                Y += _vy * dt;
                if (_cfg.Jump.HasFrames)
                    SetClipImage(_cfg.Jump, _cfg.Jump.Count - 1);

                if (FeetY >= GroundY)
                {
                    Y = GroundY - Height + _cfg.FootPadding;
                    _vy = 0;
                    if (Opponent != null)
                        FacingRight = (Opponent.BodyX + Opponent.BodyW / 2) >= (BodyX + BodyW / 2);
                    SetState(FighterState.Idle);
                }
                return;
            }

            _vy = 0;
            SetClipImage(clip, _frame);

            if (_frame == 19 && !_sasukeSp2HasSlammed)
            {
                _sasukeSp2HasSlammed = true;
                if (Opponent != null && !Opponent.IsDead)
                {
                    Opponent._isComboLocked = false;
                    Opponent._airSuspendTimer = 0;

                    int slamDmg = Math.Max(10, _cfg.Special2Damage - _sp2DealtDamage);
                    Opponent.TakeDamage(
                        slamDmg,
                        stunDuration: KnockdownGroundDuration,
                        knockdown: true,
                        sourceX: myCenterX,
                        airSlam: true);
                }
            }

            if (Advance(clip.Count, clip.FrameTime, false, dt))
            {
                _sasukeSp2HoverAfterClip = true;
                _sasukeSp2Timer = SasukeSp2EndAirDelay;
            }
        }

        // [Phát thêm] Tạo bóng mờ frame 6 (sasukedacbiet6.png) tại vị trí cũ khi Sasuke tốc biến
        private void SpawnSasukeTeleportBlur(double centerX, double feetY, double duration)
        {
            if (!_cfg.Special2.HasFrames) return;
            OnSpawnEffect?.Invoke(new DustEffect(
                _cfg.Special2.Prefix, 6, 1, duration,
                centerX, feetY, Width, Height,
                false, _cfg.FootPadding, FacingRight, 0,
                null, FighterState.Idle, 0, 0, false));
        }

        private void UpdateUltimate(double dt)
        {
            SetClipImage(_cfg.Ultimate, _frame);

            int chargeFrames = _cfg.UltimateChargeFrames > 0 ? _cfg.UltimateChargeFrames : _cfg.Ultimate.Count / 2;
            if (_frame >= chargeFrames && !_skillHasHit && _ultDashedDistance < _cfg.UltimateDashDistance)
            {
                double dir = FacingRight ? 1 : -1;
                double step = Math.Min(_cfg.UltimateDashSpeed * dt, _cfg.UltimateDashDistance - _ultDashedDistance);
                X = ClampStageX(X + dir * step);
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
            Advance(_cfg.Hurt.Count, _cfg.Hurt.FrameTime, _isAirHurt, dt);

            // [Phát thêm] Xử lý trên không: đứng lơ lửng 0.25s khi trúng nhảy đánh, hoặc dừng tại đỉnh quỹ đạo khi bị hất lên trời
            if (_isAirHurt)
            {
                if (_airSuspendTimer > 0)
                {
                    _vy = 0;
                    return;
                }

                if (_airHurtHoldTimer > 0)
                {
                    _airHurtHoldTimer = Math.Max(0, _airHurtHoldTimer - dt);
                    return;
                }

                double prevVy = _vy;
                _vy += Gravity * dt;
                Y += _vy * dt;

                if (!_hasHeldAtApex && prevVy < 0 && _vy >= 0)
                {
                    _vy = 0;
                    _hasHeldAtApex = true;
                    _airHurtHoldTimer = LaunchApexHoldDuration;
                    return;
                }

                if (_vy > 0 && FeetY >= GroundY)
                {
                    Y = GroundY - Height + _cfg.FootPadding;
                    _vy = 0;
                    _isAirHurt = false;
                    _hasHeldAtApex = false;
                    _knockbackVelocity = 0;
                    SetState(FighterState.Idle);
                }
                return;
            }

            _stateHoldTimer += dt;
            if (_stateHoldTimer >= _hurtDuration)
                SetState(FighterState.Idle);
        }

        private void UpdateKnockdown(double dt)
        {
            bool onGround;
            if (FeetY < GroundY)
            {
                _vy += Gravity * dt;
                Y += _vy * dt;
                onGround = FeetY >= GroundY;
            }
            else
            {
                onGround = true;
            }

            if (onGround)
            {
                Y = GroundY - Height + _cfg.FootPadding;
                _vy = 0;
                if (_slamDustOnLand)
                {
                    _slamDustOnLand = false;
                    SpawnFootDust(1.45);
                }
                _stateHoldTimer += dt;
            }

            SetClipImage(_cfg.Knockdown, _frame);
            bool finishedAnim = Advance(_cfg.Knockdown.Count, _cfg.Knockdown.FrameTime, false, dt);

            if (finishedAnim && onGround && _stateHoldTimer >= _hurtDuration)
                SetState(_cfg.GetUp.HasFrames ? FighterState.GetUp : FighterState.Idle);
        }

        private void UpdateDead(double dt)
        {
            UpdateKnockback(dt);
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
            if (State == FighterState.Special1 && s != FighterState.Special1)
                _special1CooldownTimer = Special1Cooldown;

            if (State == FighterState.Special2 && s != FighterState.Special2)
            {
                _special2CooldownTimer = Special2Cooldown;
                if (Opponent != null) Opponent._isComboLocked = false;
            }

            if (State == FighterState.Ultimate && s != FighterState.Ultimate)
                _ultCooldownTimer = UltCooldown;

            if ((State == FighterState.Throw || State == FighterState.JumpThrow) &&
                s != FighterState.Throw && s != FighterState.JumpThrow)
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

            if (s == FighterState.Dead)
            {
                _guardVisibleTimer = 0;
                _isComboLocked = false;
            }

            if (s == FighterState.Dash || s == FighterState.Ultimate || s == FighterState.Special2)
            {
                _knockbackVelocity = 0;
                _pendingKnockbackVelocity = 0;
                _knockbackDelayTimer = 0;
                _pendingKnockdownAfterDelay = false;
            }

            if (s != FighterState.Hurt)
            {
                _isAirHurt = false;
                _airHurtHoldTimer = 0;
                _hasHeldAtApex = false;
            }

            if (s != FighterState.Knockdown)
                _slamDustOnLand = false;

            if (s != FighterState.Jump && s != FighterState.JumpAttack && s != FighterState.Hurt && s != FighterState.Special2)
                _airSuspendTimer = 0;

            _spinRotate.Angle = 0;
            bool isSasukeSp2 = s == FighterState.Special2 && _cfg.Special2.Count >= 23;
            bool isNarutoSp2 = s == FighterState.Special2 && _cfg.Special2.Count >= 15 && !isSasukeSp2;

            _sp2Phase = isNarutoSp2 ? 1 : 0;
            _sp2ExtraRisen = 0;
            _sp2SpinCount = 0;
            _sp2SpinTimer = 0;
            _sp2DealtDamage = 0;
            _sp2HasLaunched = false;
            _sp2HasSlammed = false;
            _sp2HoveringAfterClip = false;
            _sp2AirHoverTimer = 0;

            _sasukeSp2Phase = isSasukeSp2 ? 1 : 0;
            _sasukeSp2SubStep = 0;
            _sasukeSp2Timer = 0;
            _sasukeSp2HitPauseTimer = 0;
            _sasukeSp2HalfwayFeetY = 0;
            _sasukeSp2HasSlammed = false;
            _sasukeSp2HoverAfterClip = false;

            _isTeleportAnim = false;
            State = s;
            _frame = 0;
            _timer = 0;
            _stateHoldTimer = 0;
            _lastHitFrame = -1;
            _lastAttackSteppedFrame = -1;
            _queuedNextAttackStage = false;
            _hasSpawnedSkillObj = false;
            _skillHasHit = false;
            _ultDashedDistance = 0;
            _dashedDistance = 0;

            if (s == FighterState.Attack)
            {
                GetAttackStageRange(_attackComboStage, out int startFrame, out _, out _);
                _frame = startFrame;
            }
            else if (s != FighterState.Idle && s != FighterState.Run)
            {
                // Bị đánh trúng, đỡ đòn, nhảy, lướt hoặc dùng chiêu khác sẽ ngắt chuỗi combo về đợt 1
                _attackComboStage = 0;
                _attackComboResetTimer = 0;
            }

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
            _currentOffsetY = clip.GetOffsetY(frameIndex);
            string f = clip.GetFile(frameIndex);
            if (f == _currentFile) return;
            _currentFile = f;
            Sprite.Source = ImageCache.Get(f);
        }

        private void Draw()
        {
            Canvas.SetLeft(Sprite, X);
            Canvas.SetTop(Sprite, Y + _currentOffsetY);
            _flip.ScaleX = FacingRight ? 1 : -1;
        }
    }
}