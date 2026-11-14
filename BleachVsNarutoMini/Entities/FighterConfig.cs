using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BleachVsNarutoMini.Entities
{
    // Mô tả bộ ảnh và kích thước của một nhân vật
    public class FighterConfig
    {
        public double BodyWidthRatio { get; init; } = 0.4;  // thân chiếm bao nhiêu phần bề rộng ảnh
        public double AttackReach { get; init; } = 30;      // đòn đánh vươn ra khỏi thân bao nhiêu pixel
        public string Idle { get; init; } = "";          // ví dụ "dung1.png"

        public double AttackFrameTime { get; init; } = 0.08;   // giây cho mỗi ảnh đánh (nhỏ = nhanh)
        public string RunPrefix { get; init; } = "";
        public int RunStart { get; init; } = 1;          // ảnh chạy bắt đầu từ số mấy
        public int RunFrames { get; init; }

        public string JumpPrefix { get; init; } = "";
        public int JumpFrames { get; init; }

        public string AttackPrefix { get; init; } = "";
        public int AttackFrames { get; init; }           // đòn thường
        public int ComboFrames { get; init; }            // đòn combo (0 = không có combo)

        public double Width { get; init; } = 150;
        public double Height { get; init; } = 150;
        public double GroundBottom { get; init; } = 330; // vị trí chân đứng trên Canvas
        public bool StartFacingRight { get; init; } = true;
    }
}