using System.Windows.Input;

namespace BleachVsNarutoMini.Entities
{
    public class Sasuke : Fighter
    {
        private static readonly FighterConfig Config = new()
        {
            Idle = "sskdung1.png",

            RunPrefix = "sskchay",
            RunStart = 1,
            RunFrames = 6,

            JumpPrefix = "ssknhay",
            JumpFrames = 6,

            // Logic Combo: Nhấp Z 1 lần đánh tới frame 3, nhấp liên tục ra combo tới frame 7
            AttackPrefix = "sskdanh",
            ComboSplitFrame = 3,
            AttackFrames = 7,
            AttackFrameTime = 0.08,

            // Ném phi tiêu: Dùng ảnh ngồi dậy (chỉ có 2 frame)
            ThrowPrefix = "sskngoiday",
            ThrowFrames = 2,

            // Chiêu cuối (Chidori/Lửa)
            UltimatePrefix = "sskchieu",
            UltimateFrames = 22,

            // Phòng thủ
            BlockImage = "sskphongthu.png",

            // Bị thương & Chết
            HurtPrefix = "sskbidanh",
            DeadFrames = 8, // Chết lăn ra đủ 8 frame

            // [Phát thêm] Chỉ số sát thương riêng của Sasuke (Đánh thường và phi tiêu nhỉnh hơn Naruto)
            AttackDamage = 5,
            ThrowDamage = 18,
            UltimateDamage = 32,
            UltimateReach = 30,

            // [Phát thêm] Gồng Chidori 11 frame đầu, sau đó lao cực nhanh tới áp sát đối phương
            UltimateChargeFrames = 11,
            UltimateDashSpeed = 1150,
            UltimateDashDistance = 460,

            Width = 100,
            Height = 110,
            BodyWidthRatio = 0.5,
            AttackReach = 25,
            GroundBottom = 620,
            FootPadding = 21,
            StartFacingRight = false
        };


        // Thứ tự: Trái, Phải, Nhảy, Đánh, Đỡ, Ném, Chiêu cuối, Lướt(3), Tốc biến(6)
        // [Phát thêm] Truyền thêm Key.D3 làm nút lướt và Key.D6 làm nút tốc biến của Sasuke
        public Sasuke() : base(830, Config, Key.Left, Key.Right, Key.D2, Key.D1, Key.Down, Key.D4, Key.D5, Key.D3, Key.D6) { }
    }
}