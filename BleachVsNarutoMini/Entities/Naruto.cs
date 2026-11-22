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

            // [Phát thêm] Chỉ số sát thương riêng của Naruto (Chiêu cuối mạnh và tầm rộng hơn)
            AttackDamage = 5,
            ThrowDamage = 12,
            UltimateDamage = 40,
            UltimateReach = 35,

            // [Phát thêm] Gồng Rasengan 16 frame đầu, sau đó chạy tới áp sát đối phương
            UltimateChargeFrames = 16,
            UltimateDashSpeed = 950,
            UltimateDashDistance = 420,

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

        // Gán phím: Trái(A), Phải(D), Nhảy(K), Đánh(J), Đỡ(S), Phi tiêu(U), Chiêu cuối(I), Lướt(L), Tốc biến(O)
        // [Phát thêm] Truyền thêm Key.L làm nút lướt và Key.O làm nút tốc biến của Naruto
        public Naruto() : base(100, Config, Key.A, Key.D, Key.K, Key.J, Key.S, Key.U, Key.I, Key.L, Key.O) { }
    }
}