using System.Windows.Input;

namespace BleachVsNarutoMini.Entities
{
    public class Sasuke : Fighter
    {
        private static readonly FighterConfig Config = new()
        {
            Idle = new AnimClip { Prefix = "Sasuke/dung/sasukedung", Start = 1, Frames = 4, FrameTime = 0.12 },
            Run = new AnimClip { Prefix = "Sasuke/chay/sasukechay", Start = 1, Frames = 11, FrameTime = 0.06 },
            Jump = new AnimClip { Prefix = "Sasuke/nhay/sasukenhay", Start = 1, Frames = 4, FrameTime = 0.10 },
            Dash = new AnimClip { Prefix = "Sasuke/dichchuyen/sasukedichchuyen", Start = 1, Frames = 2, FrameTime = 0.06 },
            Block = new AnimClip { Prefix = "Sasuke/thuthe/sasukethuthe.png", Frames = 1 },

            Attack = new AnimClip { Prefix = "Sasuke/danhthuong/sasukedanh", Start = 1, Frames = 18, FrameTime = 0.06 },
            ComboSplitFrame = 5,
            JumpAttack = new AnimClip { Prefix = "Sasuke/nhaydanh/sasukenhaydanh", Start = 1, Frames = 7, FrameTime = 0.06 },
            CrouchAttack = new AnimClip { Prefix = "Sasuke/danhdacbiet/sasukedanhdacbiet", Start = 1, Frames = 8, FrameTime = 0.06 },

            Throw = new AnimClip { Prefix = "Sasuke/phongphitieu/sasukephongphitieu", Start = 1, Frames = 3, FrameTime = 0.07 },

            Special1 = new AnimClip { Prefix = "Sasuke/phunlua/sasukephunlua", Start = 1, Frames = 20, FrameTime = 0.06 },
            Special1ProjectilePrefix = "Assets/effect/HieuUngSasuke/sasukePhunLua/sasukelua",
            Special1ProjectileFrames = 10,
            Special1SpawnFrame = 8,

            Special2 = new AnimClip { Prefix = "Sasuke/dungchieudacbiet/sasukedacbiet", Start = 1, Frames = 23, FrameTime = 0.06 },
            Ultimate = new AnimClip { Prefix = "Sasuke/chidori/sasukechidori", Start = 1, Frames = 54, FrameTime = 0.06 },

            Hurt = new AnimClip { Prefix = "Sasuke/bidanh/sasukenga", Start = 2, Frames = 2, FrameTime = 0.10 },
            Knockdown = new AnimClip { Prefix = "Sasuke/khibidanhlientiep/sasukenga", Start = 2, Frames = 6, FrameTime = 0.10 },
            GetUp = new AnimClip
            {
                CustomFiles = new[]
                {
                    "Sasuke/ngoiday/sasukenga7-2.png",
                    "Sasuke/ngoiday/sasukedung.png"
                },
                FrameTime = 0.12
            },
            DashEffect = new EffectConfig
            {
                Prefix = "Assets/effect/khiNhanVatChayNhanh/bui",
                Frames = 7,
                FrameTime = 0.05,
                Width = 160,
                Height = 55,
                IsCenterY = false,
                GroundOffsetY = 20,
                ZIndex = 0
            },
            UltimateBackEffect = new EffectConfig
            {
                Prefix = "Assets/effect/HieuUngSasuke/sét/sasukeset",
                Frames = 4,
                FrameTime = 0.07,
                Width = 190,
                Height = 160,
                IsCenterY = true,
                OffsetX = -15,
                OffsetY = -10,
                ZIndex = -1,
                FollowOwner = true,
                Loop = true
            },
            AttackDamage = 6,
            CrouchAttackDamage = 12,
            JumpAttackDamage = 8,
            ThrowDamage = 18,
            Special1Damage = 25,
            Special2Damage = 28,
            UltimateDamage = 32,
            UltimateReach = 30,

            UltimateChargeFrames = 20,
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

        public Sasuke() : base(830, Config, Key.Left, Key.Right, Key.D2, Key.D1, Key.Down, Key.D4, Key.D5, Key.D3) { }
    }
}