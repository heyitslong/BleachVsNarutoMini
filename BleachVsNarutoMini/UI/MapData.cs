using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

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

        public static readonly string[] AllIds = { "Arena1", "Arena2" };

        public static MapData Get(string id)
        {
            return id == "Arena2" ? CreateArena2() : CreateArena1();
        }

        // Đấu trường 1: Đấu trường Đá 
        private static MapData CreateArena1()
        {
            var map = new MapData
            {
                Id = "Arena1",
                Name = "Đấu trường Đá",
                Background = Frozen(Color.FromRgb(0x2B, 0x2B, 0x3C)),
                ObstacleFill = Frozen(Color.FromRgb(0x7A, 0x6A, 0x58)),
                ObstacleBorder = Brushes.Black,
                P1Spawn = new Point(80, 300),
                P2Spawn = new Point(920, 300)
            };

            AddBorderWalls(map.Obstacles, 20);

            map.Obstacles.Add(new Rect(472, 270, 80, 100));   // hộp lớn ở giữa
            map.Obstacles.Add(new Rect(200, 150, 100, 40));   // 4 hộp nhỏ đối xứng
            map.Obstacles.Add(new Rect(724, 150, 100, 40));
            map.Obstacles.Add(new Rect(200, 450, 100, 40));
            map.Obstacles.Add(new Rect(724, 450, 100, 40));
            return map;
        }

        //  Đấu trường 2: Đấu trường Băng 
        private static MapData CreateArena2()
        {
            var map = new MapData
            {
                Id = "Arena2",
                Name = "Đấu trường Băng",
                Background = Frozen(Color.FromRgb(0x1B, 0x3A, 0x4B)),
                ObstacleFill = Frozen(Color.FromRgb(0x8E, 0xCA, 0xE6)),
                ObstacleBorder = Frozen(Color.FromRgb(0x21, 0x9E, 0xBC)),
                P1Spawn = new Point(80, 300),
                P2Spawn = new Point(920, 300)
            };

            AddBorderWalls(map.Obstacles, 20);

            map.Obstacles.Add(new Rect(300, 20, 40, 200));    // 4 cột đối xứng
            map.Obstacles.Add(new Rect(684, 20, 40, 200));
            map.Obstacles.Add(new Rect(300, 420, 40, 200));
            map.Obstacles.Add(new Rect(684, 420, 40, 200));
            map.Obstacles.Add(new Rect(432, 300, 160, 40));   // thanh ngang ở giữa
            return map;
        }

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
    }
}
