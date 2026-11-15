using System;
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

        public Image Sprite { get; }
        public Fighter Owner { get; }
        public int Damage { get; }
        public bool IsActive { get; private set; } = true;

        public Rect Bounds => new(X, Y, Width, Height);

        public Projectile(
            Fighter owner,
            string imageFile,
            double x,
            double y,
            double width,
            double height,
            double speed,
            int damage,
            bool facingRight)
            : base(x, y, width, height, speed)
        {
            Owner = owner;
            Damage = damage;
            _dir = facingRight ? 1 : -1;
            _flip = new ScaleTransform(_dir, 1);

            Sprite = new Image
            {
                Width = width,
                Height = height,
                Source = ImageCache.Get(imageFile),
                RenderTransform = _flip,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            RenderOptions.SetBitmapScalingMode(Sprite, BitmapScalingMode.NearestNeighbor);
            Draw();
        }

        public void Update(
            double dt,
            Fighter target,
            IReadOnlyList<Rect> obstacles,
            double stageWidth)
        {
            if (!IsActive) return;

            X += _dir * Speed * dt;
            Draw();

            // 1. Bay ra khỏi hai mép sân -> tự hủy
            if (X < 0 || X + Width > stageWidth)
            {
                IsActive = false;
                return;
            }

            // 2. Trúng đối thủ -> gây sát thương & tự hủy
            if (!target.IsDead && Bounds.IntersectsWith(target.Bounds))
            {
                target.TakeDamage(Damage);
                IsActive = false;
                return;
            }

            // 3. Đụng vào các khối bục trên bản đồ (bỏ qua 4 tường viền ngoài cùng) -> tự hủy
            foreach (var obs in obstacles)
            {
                if (obs.Left <= 0 || obs.Right >= stageWidth || obs.Top <= 0 || obs.Bottom >= 640)
                    continue;

                if (Bounds.IntersectsWith(obs))
                {
                    IsActive = false;
                    return;
                }
            }
        }

        private void Draw()
        {
            Canvas.SetLeft(Sprite, X);
            Canvas.SetTop(Sprite, Y);
        }
    }
}