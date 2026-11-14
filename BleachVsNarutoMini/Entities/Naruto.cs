using System.Windows.Input;

namespace BleachVsNarutoMini.Entities
{
    public class Naruto : Fighter
    {
        private static readonly FighterConfig Config = new()
        {
            Idle = "dung1.png",
            RunPrefix = "dung",
            RunStart = 2,
            RunFrames = 2,
            JumpPrefix = "nhay",
            JumpFrames = 4,
            AttackPrefix = "danh",
            AttackFrames = 3,
            Width = 170,
            Height = 170,
            BodyWidthRatio = 0.5,
            AttackReach = 25,
            GroundBottom = 360,
            StartFacingRight = true,
            AttackFrameTime = 0.06

        };

        public Naruto() : base(100, Config, Key.A, Key.D, Key.K, Key.J) { }
    }
}