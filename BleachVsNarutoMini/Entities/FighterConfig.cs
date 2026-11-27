using System;

namespace BleachVsNarutoMini.Entities
{
    public enum FighterState
    {
        Idle, Run, Jump, Dash, Block,
        Attack, JumpAttack, CrouchAttack,
        Throw, JumpThrow,
        Special1, Special2, Ultimate, Transform,
        Hurt, Knockdown, GetUp, Dead
    }

    public class AnimClip
    {
        public string Prefix { get; init; } = "";
        public int Start { get; init; } = 1;
        public int Frames { get; init; } = 0;
        public double FrameTime { get; init; } = 0.08;
        public string[]? CustomFiles { get; init; }
        public double[]? FrameOffsetsY { get; init; } // [Phát thêm] Độ dịch chuyển trục Y riêng cho từng frame (> 0 hạ thấp xuống)
        public int Count => CustomFiles != null ? CustomFiles.Length : Frames;
        public bool HasFrames => Count > 0 && (CustomFiles != null || !string.IsNullOrEmpty(Prefix));

        public string GetFile(int frameIndex)
        {
            int idx = Math.Clamp(frameIndex, 0, Math.Max(0, Count - 1));
            if (CustomFiles != null) return CustomFiles[idx];
            if (Frames == 1 && Prefix.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return Prefix;
            return $"{Prefix}{Start + idx}.png";
        }

        // [Phát thêm] Lấy độ bù tọa độ Y của frame hiện tại (mặc định bằng 0)
        public double GetOffsetY(int frameIndex)
        {
            if (FrameOffsetsY == null || FrameOffsetsY.Length == 0) return 0;
            int idx = Math.Clamp(frameIndex, 0, FrameOffsetsY.Length - 1);
            return FrameOffsetsY[idx];
        }
    }

    public class EffectConfig
    {
        public string Prefix { get; init; } = "";
        public int Start { get; init; } = 1;
        public int Frames { get; init; } = 0;
        public double FrameTime { get; init; } = 0.05;
        public double Width { get; init; } = 120;
        public double Height { get; init; } = 120;
        public bool IsCenterY { get; init; } = true;
        public double GroundOffsetY { get; init; } = 20;
        public double OffsetX { get; init; } = 0;
        public double OffsetY { get; init; } = 0;
        public int ZIndex { get; init; } = 0;
        public bool FollowOwner { get; init; } = false;
        public bool Loop { get; init; } = false;

        public bool HasFrames => Frames > 0 && !string.IsNullOrEmpty(Prefix);
    }
    public class FighterConfig
    {
        public AnimClip Transform { get; init; } = new();
        public double BodyWidthRatio { get; init; } = 0.5;
        public double AttackReach { get; init; } = 25;
        public double Width { get; init; } = 100;
        public double Height { get; init; } = 110;
        public double GroundBottom { get; init; } = 620;
        public bool StartFacingRight { get; init; } = true;
        public double FootPadding { get; init; } = 15;
        public double KnockdownFootPadding { get; init; } = 32; // [Phát thêm] Độ hạ thấp khung hình khi nhân vật nằm ngã/gục


        // Sát thương và tầm đánh
        public int AttackDamage { get; init; } = 5;
        public int CrouchAttackDamage { get; init; } = 10;
        public int JumpAttackDamage { get; init; } = 8;
        public int ThrowDamage { get; init; } = 15;
        public int Special1Damage { get; init; } = 22;
        public int Special2Damage { get; init; } = 28;
        public int UltimateDamage { get; init; } = 35;
        public double UltimateReach { get; init; } = 35;

        // Thông số lao tới khi dùng Chiêu cuối
        public int UltimateChargeFrames { get; init; } = 16;
        public double UltimateDashSpeed { get; init; } = 950;
        public double UltimateDashDistance { get; init; } = 420;

        // Nhịp ngắt combo đánh thường
        public int ComboSplitFrame { get; init; } = 5;

        // Hiệu ứng đạn riêng cho Special 1 (dùng cho chiêu phun lửa của Sasuke)
        public string Special1ProjectilePrefix { get; init; } = "";
        public int Special1ProjectileFrames { get; init; } = 10;
        public int Special1SpawnFrame { get; init; } = 8;

        // Toàn bộ các bộ Animation tương ứng với các thư mục ảnh mới
        public AnimClip Idle { get; init; } = new();
        public AnimClip Run { get; init; } = new();
        public AnimClip Jump { get; init; } = new();
        public AnimClip Dash { get; init; } = new();
        public AnimClip Block { get; init; } = new();

        public AnimClip Attack { get; init; } = new();
        public AnimClip JumpAttack { get; init; } = new();
        public AnimClip CrouchAttack { get; init; } = new();

        public AnimClip Throw { get; init; } = new();
        public AnimClip JumpThrow { get; init; } = new();

        public AnimClip Special1 { get; init; } = new();
        public AnimClip Special2 { get; init; } = new();
        public AnimClip Ultimate { get; init; } = new();

        public AnimClip Hurt { get; init; } = new();
        public AnimClip Knockdown { get; init; } = new();
        public AnimClip GetUp { get; init; } = new();
        public EffectConfig? DashEffect { get; init; }
        public EffectConfig? UltimateBackEffect { get; init; }
    }
}