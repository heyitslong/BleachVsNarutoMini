using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Imaging;

namespace BleachVsNarutoMini.UI
{
    
    //  DỮ LIỆU BẢN ĐỒ
    public class MapData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Brush Background { get; set; }
        public Brush ObstacleFill { get; set; }
        public Brush ObstacleBorder { get; set; }
        public Point P1Spawn { get; set; }          // vị trí xuất phát người chơi 1
        public Point P2Spawn { get; set; }          // vị trí xuất phát người chơi 2
        public List<Rect> Obstacles { get; set; } = new List<Rect>();

        // N2 dùng hàm này để chặn nhân vật / hủy đạn khi đụng hộp, tường
        public bool HitsObstacle(Rect body)
        {
            foreach (var o in Obstacles)
                if (body.IntersectsWith(o)) return true;
            return false;
        }
    }

    public static class MapLibrary
    {
        public const double Width = 1024;
        public const double Height = 640;

        // [Sửa] Thêm Arena3 vào danh sách
        public static readonly string[] AllIds = { "Arena1", "Arena2", "Arena3" };

        public static MapData Get(string id)
        {
            return id switch
            {
                "Arena2" => CreateArena2(),
                "Arena3" => CreateArena3(),
                _ => CreateArena1()
            };
        }

        // Đấu trường 1: Thung lũng Tận Cùng
        private static MapData CreateArena1()
        {
            var map = new MapData
            {
                Id = "Arena1",
                Name = "Thung lũng Tận Cùng",
                Background = LoadImageBrush("Assets/Images/map/arena1_bg.png",
                            Frozen(Color.FromRgb(0x2B, 0x2B, 0x3C))),
                ObstacleFill = Frozen(Color.FromRgb(0x5C, 0x54, 0x49)),
                ObstacleBorder = Brushes.Black,
                P1Spawn = new Point(80, 300),
                P2Spawn = new Point(920, 300)
            };
            return map;
        }

        // Đấu trường 2: Rừng Chết
        private static MapData CreateArena2()
        {
            var map = new MapData
            {
                Id = "Arena2",
                Name = "Rừng Chết",
                Background = LoadImageBrush("Assets/Images/map/arena2_bg.png",
                            Frozen(Color.FromRgb(0x2E, 0x3B, 0x23))),
                ObstacleFill = Frozen(Color.FromRgb(0x4A, 0x3B, 0x2C)),
                ObstacleBorder = Brushes.Black,
                P1Spawn = new Point(80, 300),
                P2Spawn = new Point(920, 300)
            };
            return map;
        }

        // [Thêm mới] Đấu trường 3: Học viện Làng Lá
        private static MapData CreateArena3()
        {
            var map = new MapData
            {
                Id = "Arena3",
                Name = "Học viện Làng Lá",
                Background = LoadImageBrush("Assets/Images/map/arena3_bg.png",
                            Frozen(Color.FromRgb(0x3E, 0x2F, 0x22))),
                ObstacleFill = Frozen(Color.FromRgb(0x6D, 0x4C, 0x3D)),
                ObstacleBorder = Brushes.Black,
                P1Spawn = new Point(80, 300),
                P2Spawn = new Point(920, 300)
            };
            return map;
        }

        // ... (giữ nguyên AddBorderWalls, Frozen, LoadImageBrush ở bên dưới)

        // 4 bức tường bao quanh sân
        private static void AddBorderWalls(List<Rect> list, double t)
        {
            list.Add(new Rect(0, 0, Width, t));               // trên
            list.Add(new Rect(0, Height - t, Width, t));      // dưới
            list.Add(new Rect(0, 0, t, Height));              // trái
            list.Add(new Rect(Width - t, 0, t, Height));      // phải
        }

        private static Brush Frozen(Color c)
        {
            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }
        // Nạp ảnh làm nền; nếu lỗi/không có ảnh thì dùng màu dự phòng
        private static Brush LoadImageBrush(string path, Brush fallback)
        {
            try
            {
                var img = new BitmapImage(new Uri("pack://application:,,,/" + path, UriKind.Absolute));
                img.Freeze();
                var brush = new ImageBrush(img) { Stretch = Stretch.Fill };
                brush.Freeze();
                return brush;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không nạp được ảnh: " + path + "\n\n" + ex.Message);
                return fallback;
            }
            {
                return fallback;
            }
        }
    }
}
