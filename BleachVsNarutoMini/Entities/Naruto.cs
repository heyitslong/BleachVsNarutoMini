using System.Windows.Input;

namespace BleachVsNarutoMini.Entities
{
    public class Naruto : Fighter
    {
        private static readonly FighterConfig Config = new()
        {
            Idle = new AnimClip { Prefix = "Naruto/dung/narutodung", Start = 1, Frames = 6, FrameTime = 0.12 },
            Run = new AnimClip { Prefix = "Naruto/chay/narutochay", Start = 1, Frames = 6, FrameTime = 0.08 },
            Jump = new AnimClip { Prefix = "Naruto/nhay/narutonhay", Start = 1, Frames = 5, FrameTime = 0.10 },
            Dash = new AnimClip { Prefix = "Naruto/dichchuyen/narutodichchuyen", Start = 1, Frames = 2, FrameTime = 0.06 },
            Block = new AnimClip { Prefix = "Naruto/thuthe/narutongoi", Start = 1, Frames = 4, FrameTime = 0.08 },

            Attack = new AnimClip { Prefix = "Naruto/danhthuong/narutodanh", Start = 1, Frames = 17, FrameTime = 0.06 },
            ComboSplitFrame = 5,
            JumpAttack = new AnimClip { Prefix = "Naruto/nhaydanh/narutonhaydanh", Start = 1, Frames = 9, FrameTime = 0.06 },
            CrouchAttack = new AnimClip { Prefix = "Naruto/ngoidanh/narutongoidanh", Start = 1, Frames = 4, FrameTime = 0.07 },

            Throw = new AnimClip { Prefix = "Naruto/nemphitieu/narutonemphitieu", Start = 1, Frames = 4, FrameTime = 0.07 },
            JumpThrow = new AnimClip { Prefix = "Naruto/nhaynemphitieu/narutonhaynemphitieu", Start = 1, Frames = 3, FrameTime = 0.07 },

            Special1 = new AnimClip { Prefix = "Naruto/danhdacbiet/narutodanhdacbiet", Start = 1, Frames = 18, FrameTime = 0.06 },
            Special2 = new AnimClip { Prefix = "Naruto/chieudacbiet/narutodacbiet", Start = 1, Frames = 15, FrameTime = 0.07 },
            Ultimate = new AnimClip { Prefix = "Naruto/rasengan/narutorasengan", Start = 1, Frames = 45, FrameTime = 0.06 },

            Hurt = new AnimClip { Prefix = "Naruto/bidanh/narutobidanh", Start = 1, Frames = 2, FrameTime = 0.10 },
            Knockdown = new AnimClip { Prefix = "Naruto/bidanhlientiep/narutobidanh", Start = 1, Frames = 4, FrameTime = 0.10 },
            GetUp = new AnimClip { Prefix = "Naruto/ngoiday/narutongoiday", Start = 1, Frames = 3, FrameTime = 0.10 },
            DashEffect = new EffectConfig
            {
                Prefix = "Assets/effect/HieuUngNaruto/narutoHUdichchuyen",
                Frames = 4,
                FrameTime = 0.06,
                Width = 110,
                Height = 110,
                IsCenterY = true,
                ZIndex = 2
            },
            AttackDamage = 5,
            CrouchAttackDamage = 10,
            JumpAttackDamage = 8,
            ThrowDamage = 12,
            Special1Damage = 24,
            Special2Damage = 28,
            UltimateDamage = 40,
            UltimateReach = 35,

            UltimateChargeFrames = 16,
            UltimateDashSpeed = 950,
            UltimateDashDistance = 420,

            Width = 100,
            Height = 110,
            BodyWidthRatio = 0.5,
            AttackReach = 25,
            GroundBottom = 620,
            FootPadding = 15,
            StartFacingRight = true
        };

        public Naruto() : base(100, Config, Key.A, Key.D, Key.K, Key.J, Key.S, Key.U, Key.I, Key.L) { }
    }
}