using System.Windows.Input;

namespace BleachVsNarutoMini.Entities
{
    public class Naruto : Fighter
    {
        private static readonly FighterConfig Config = new()
        {
            Idle = "narutodung.png",
            RunPrefix = "narutochay",
            RunStart = 1,
            RunFrames = 6,
            JumpPrefix = "narutonhay",
            JumpFrames = 5,

            // Logic Combo: Đánh thường đến frame 4, nếu bấm tiếp sẽ chạy đến 12
            AttackPrefix = "narutodanh",
            ComboSplitFrame = 4,
            AttackFrames = 12,
            AttackFrameTime = 0.06,

            ThrowPrefix = "narutonemvukhi",
            ThrowFrames = 4,

            UltimatePrefix = "narutochuanbitungchieu",
            UltimateFrames = 31,

            BlockImage = "narutophongthu.png",

            HurtPrefix = "narutobithuong",
            DeadFrames = 6,

            // Các thông số giữ nguyên...

            // Sửa lại đoạn kích thước này:
            Width = 100,           // Cũ là 170, sửa thành 100 bằng Sasuke
            Height = 110,          // Cũ là 170, sửa thành 110 bằng Sasuke
            BodyWidthRatio = 0.5,
            AttackReach = 25,      // Tầm đánh cũng nên giảm xuống cho cân bằng
            GroundBottom = 620,
            FootPadding = 15,      // Cũ là 48, hạ xuống tầm 15-20 (tùy bạn test thấy chân vừa chạm đất)
            StartFacingRight = true
        };

        // Gán phím: Trái(A), Phải(D), Nhảy(K), Đánh(J), Đỡ(S), Phi tiêu(U), Chiêu cuối(I)
        public Naruto() : base(100, Config, Key.A, Key.D, Key.K, Key.J, Key.S, Key.U, Key.I) { }
    }
}