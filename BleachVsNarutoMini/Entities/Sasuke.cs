using System.Windows.Input;

namespace BleachVsNarutoMini.Entities
{
    public class Sasuke : Fighter
    {
        private static readonly FighterConfig Config = new()
        {
            Idle = "sskdung1.png",
            RunPrefix = "sskchay",
            RunFrames = 6,
            JumpPrefix = "ssknhay",
            JumpFrames = 6,
            AttackPrefix = "sskdanh",
            AttackFrames = 3,
            ComboFrames = 7,
            Width = 100,
            Height = 110,
            BodyWidthRatio = 0.5,
            AttackReach = 25,
            GroundBottom = 330,
            StartFacingRight = false            // quay sang trái nhìn Naruto
        };

        public Sasuke() : base(600, Config, Key.Left, Key.Right, Key.D2, Key.D1) { }
    }
}