using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;

namespace BleachVsNarutoMini.Entities
{
    public class Projectile : GameObject
    {
        private readonly int _dir;
        private readonly ScaleTransform _flip;
        private readonly string _baseFile;
        private double _animTimer;
        private int _animFrame = 1;

        public Image Sprite { get; }
        public Fighter Owner { get; }
        public int Damage { get; }
        public bool IsActive { get; private set; } = true;
        public Rect Bounds => new(X, Y, Width, Height);

        public Projectile(Fighter owner, string baseImage, double x, double y, double w, double h, double spd, int dmg, bool facingRight)
            : base(x, y, w, h, spd)
        {
            Owner = owner; Damage = dmg; _baseFile = baseImage;
            _dir = facingRight ? 1 : -1; _flip = new ScaleTransform(_dir, 1);

            Sprite = new Image { Width = w, Height = h, RenderTransform = _flip, RenderTransformOrigin = new Point(0.5, 0.5) };
            RenderOptions.SetBitmapScalingMode(Sprite, BitmapScalingMode.NearestNeighbor);
            Sprite.Source = ImageCache.Get($"{_baseFile}{_animFrame}.png");
            Draw();
        }

        public void Update(double dt, Fighter target, IReadOnlyList<Rect> obstacles, double stageWidth)
        {
            if (!IsActive) return;
            X += _dir * Speed * dt;

            // Hoạt ảnh xoay phi tiêu (từ 1 đến 5)
            _animTimer += dt;
            if (_animTimer >= 0.05)
            {
                _animTimer -= 0.05;
                _animFrame++;
                if (_animFrame > 5) _animFrame = 1;
                Sprite.Source = ImageCache.Get($"{_baseFile}{_animFrame}.png");
            }
            Draw();

            if (X < 0 || X + Width > stageWidth) { IsActive = false; return; }
            if (!target.IsDead && Bounds.IntersectsWith(target.Bounds)) { target.TakeDamage(Damage); IsActive = false; return; }
        }

        private void Draw() { Canvas.SetLeft(Sprite, X); Canvas.SetTop(Sprite, Y); }
    }
}