using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BleachVsNarutoMini.Entities
{
    public class DustEffect
    {
        private readonly string _prefix;
        private readonly int _start;
        private readonly int _totalFrames;
        private readonly double _frameTime;
        private readonly double _width;
        private readonly double _height;
        private readonly bool _isCenterY;
        private readonly double _groundOffsetY;
        private readonly Fighter? _followOwner;
        private readonly FighterState _followState;
        private readonly double _offsetX;
        private readonly double _offsetY;
        private readonly bool _loop;
        private readonly ScaleTransform _flip;

        private double _timer;
        private int _frame;

        public Image Sprite { get; }
        public bool IsFinished { get; private set; }

        // Constructor cũ giữ nguyên để tương thích code sẵn có
        public DustEffect(double centerX, double targetY, bool isCenterY = false)
            : this("Assets/effect/khiNhanVatChayNhanh/bui", 1, 7, 0.05,
                   centerX, targetY, 160, 55, isCenterY, 20, true, 0,
                   null, FighterState.Idle, 0, 0, false)
        {
        }

        // Constructor mở rộng cho mọi hiệu ứng (dịch chuyển Naruto, bụi Sasuke, sét Chidori...)
        public DustEffect(
            string prefix, int start, int totalFrames, double frameTime,
            double centerX, double targetY, double width, double height,
            bool isCenterY, double groundOffsetY, bool facingRight, int zIndex,
            Fighter? followOwner, FighterState followState,
            double offsetX, double offsetY, bool loop)
        {
            _prefix = prefix;
            _start = start;
            _totalFrames = totalFrames;
            _frameTime = frameTime;
            _width = width;
            _height = height;
            _isCenterY = isCenterY;
            _groundOffsetY = groundOffsetY;
            _followOwner = followOwner;
            _followState = followState;
            _offsetX = offsetX;
            _offsetY = offsetY;
            _loop = loop;
            _flip = new ScaleTransform(facingRight ? 1 : -1, 1);

            Sprite = new Image
            {
                Width = width,
                Height = height,
                Source = ImageCache.Get($"{_prefix}{_start}.png"),
                IsHitTestVisible = false,
                RenderTransform = _flip,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            RenderOptions.SetBitmapScalingMode(Sprite, BitmapScalingMode.NearestNeighbor);
            Panel.SetZIndex(Sprite, zIndex);

            SetPosition(centerX, targetY);
        }

        public void Update(double dt)
        {
            if (IsFinished) return;

            if (_followOwner != null)
            {
                if (_followOwner.IsDead || _followOwner.State != _followState)
                {
                    IsFinished = true;
                    return;
                }

                double dir = _followOwner.FacingRight ? 1 : -1;
                _flip.ScaleX = dir;
                double cx = _followOwner.X + _followOwner.Width / 2 + _offsetX * dir;
                double ty = (_isCenterY ? (_followOwner.Y + _followOwner.Height / 2) : (_followOwner.Y + _followOwner.Height)) + _offsetY;
                SetPosition(cx, ty);
            }

            _timer += dt;
            while (_timer >= _frameTime)
            {
                _timer -= _frameTime;
                _frame++;
                if (_frame >= _totalFrames)
                {
                    if (_loop) _frame = 0;
                    else
                    {
                        IsFinished = true;
                        return;
                    }
                }
                Sprite.Source = ImageCache.Get($"{_prefix}{_start + _frame}.png");
            }
        }

        private void SetPosition(double centerX, double targetY)
        {
            Canvas.SetLeft(Sprite, centerX - _width / 2);
            Canvas.SetTop(Sprite, _isCenterY ? (targetY - _height / 2) : (targetY - _height + _groundOffsetY));
        }
    }
}