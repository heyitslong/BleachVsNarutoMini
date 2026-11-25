using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BleachVsNarutoMini.Entities
{
    public class Projectile : GameObject
    {
        private readonly int _dir;
        private readonly ScaleTransform _flip;
        private readonly string _baseFile;
        private readonly int _maxFrames;
        private readonly double _frameTime;
        private readonly bool _knockdown;
        private double _animTimer;
        private int _animFrame = 1;

        public Image Sprite { get; }
        public Fighter Owner { get; }
        public int Damage { get; }
        public bool IsActive { get; private set; } = true;
        public Rect Bounds => new(X, Y, Width, Height);

        public Projectile(
            Fighter owner, string baseImage, double x, double y, double w, double h,
            double spd, int dmg, bool facingRight, int maxFrames = 5, double frameTime = 0.05, bool knockdown = false)
            : base(x, y, w, h, spd)
        {
            Owner = owner;
            Damage = dmg;
            _baseFile = baseImage;
            _maxFrames = maxFrames;
            _frameTime = frameTime;
            _knockdown = knockdown;
            _dir = facingRight ? 1 : -1;
            _flip = new ScaleTransform(_dir, 1);

            Sprite = new Image
            {
                Width = w,
                Height = h,
                RenderTransform = _flip,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            RenderOptions.SetBitmapScalingMode(Sprite, BitmapScalingMode.NearestNeighbor);
            Sprite.Source = ImageCache.Get($"{_baseFile}{_animFrame}.png");
            Draw();
        }

        public void Update(double dt, Fighter target, IReadOnlyList<Rect> obstacles, double stageWidth)
        {
            if (!IsActive) return;
            X += _dir * Speed * dt;

            _animTimer += dt;
            if (_animTimer >= _frameTime)
            {
                _animTimer -= _frameTime;
                _animFrame++;
                if (_animFrame > _maxFrames) _animFrame = 1;
                Sprite.Source = ImageCache.Get($"{_baseFile}{_animFrame}.png");
            }
            Draw();

            if (X < -Width || X > stageWidth + Width) { IsActive = false; return; }
            if (!target.IsDead && target.State != FighterState.Knockdown && target.State != FighterState.GetUp && Bounds.IntersectsWith(target.Bounds))
            {
                target.TakeDamage(Damage, _knockdown ? 0.5 : 0.3, _knockdown, X + Width / 2);
                IsActive = false;
            }
        }

        private void Draw()
        {
            Canvas.SetLeft(Sprite, X);
            Canvas.SetTop(Sprite, Y);
        }
    }
}