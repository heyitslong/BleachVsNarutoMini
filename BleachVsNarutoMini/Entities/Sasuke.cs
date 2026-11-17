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

            Width = 100,
            Height = 110,
            BodyWidthRatio = 0.5,
            AttackReach = 25,
            GroundBottom = 620,
            FootPadding = 21,
            StartFacingRight = false
        };

        // Gán 9 phím: Trái(Left), Phải(Right), Nhảy(Up), Đánh(Z), Đỡ(Down), Ném(C), Chiêu cuối(X)
        // Tui gán nút Nhảy là Mũi tên Lên (Up) cho tiện tay kết hợp với nút Xuống (Down) đỡ đòn nha.
        public Sasuke() : base(830, Config, Key.Left, Key.Right, Key.Up, Key.Z, Key.Down, Key.C, Key.X) { }
    }
}