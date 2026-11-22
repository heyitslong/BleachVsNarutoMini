using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BleachVsNarutoMini.Entities
{
    // [Phát thêm] Lớp quản lý hiệu ứng bụi dưới chân khi Nhảy/Lướt và làn khói tại vị trí cũ/mới khi Tốc biến
    public class DustEffect
    {
        private const int TotalFrames = 7;          // bui1.png -> bui7.png
        private const double FrameTime = 0.05;      // Thời gian chạy mỗi khung hình (giây)
        private const double EffectWidth = 160;     // Chiều rộng đám bụi/khói
        private const double EffectHeight = 55;     // Chiều cao đám bụi/khói
        private const double GroundOffsetY = 20;    // [Phát thêm] Độ dịch xuống theo trục Y để khói nằm sát bàn chân (tăng số này nếu muốn thấp hơn nữa)

        private static readonly BitmapImage[] _frames = new BitmapImage[TotalFrames];
        private static bool _loaded;

        private double _timer;
        private int _frame;

        public Image Sprite { get; }
        public bool IsFinished { get; private set; }

        public DustEffect(double centerX, double targetY, bool isCenterY = false)
        {
            EnsureLoaded();

            Sprite = new Image
            {
                Width = EffectWidth,
                Height = EffectHeight,
                Source = _frames[0],
                IsHitTestVisible = false
            };
            RenderOptions.SetBitmapScalingMode(Sprite, BitmapScalingMode.NearestNeighbor);

            Canvas.SetLeft(Sprite, centerX - EffectWidth / 2);
            // [Phát thêm] Cộng thêm GroundOffsetY để hạ thấp đám khói xuống sát mặt đất
            Canvas.SetTop(Sprite, isCenterY ? (targetY - EffectHeight / 2) : (targetY - EffectHeight + GroundOffsetY));
        }

        public void Update(double dt)
        {
            if (IsFinished) return;

            _timer += dt;
            while (_timer >= FrameTime)
            {
                _timer -= FrameTime;
                _frame++;
                if (_frame >= TotalFrames)
                {
                    IsFinished = true;
                    return;
                }
                Sprite.Source = _frames[_frame];
            }
        }

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;

            for (int i = 0; i < TotalFrames; i++)
            {
                string fileName = $"bui{i + 1}.png";
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri($"pack://application:,,,/Assets/effect/{fileName}", UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                _frames[i] = bmp;
            }
        }
    }
}