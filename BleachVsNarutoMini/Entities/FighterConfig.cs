using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BleachVsNarutoMini.Entities
{
    public enum FighterState { Idle, Run, Attack, Jump, Block, Throw, Ultimate, Hurt, Dead }

    public class FighterConfig
    {
        public double BodyWidthRatio { get; init; } = 0.4;
        public double AttackReach { get; init; } = 30;
        public double Width { get; init; } = 150;
        public double Height { get; init; } = 150;
        public double GroundBottom { get; init; } = 330;
        public bool StartFacingRight { get; init; } = true;
        public double FootPadding { get; init; } = 0;

        public string Idle { get; init; } = "";

        public string RunPrefix { get; init; } = "";
        public int RunStart { get; init; } = 1;
        public int RunFrames { get; init; }

        public string JumpPrefix { get; init; } = "";
        public int JumpFrames { get; init; }

        public string AttackPrefix { get; init; } = "";
        public int AttackFrames { get; init; }
        public int ComboSplitFrame { get; init; } // Nhịp ngừng của đòn đánh thường (Ví dụ: Frame 4)
        public double AttackFrameTime { get; init; } = 0.08;

        public string ThrowPrefix { get; init; } = "";
        public int ThrowFrames { get; init; }

        public string UltimatePrefix { get; init; } = "";
        public int UltimateFrames { get; init; }

        public string BlockImage { get; init; } = ""; // Ảnh phòng thủ tĩnh

        public string HurtPrefix { get; init; } = "";
        public int DeadFrames { get; init; } // Số frame khi hết HP
    }
}