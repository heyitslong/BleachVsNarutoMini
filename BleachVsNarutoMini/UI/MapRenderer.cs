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
    
    //  VẼ BẢN ĐỒ LÊN MỘT CANVAS RIÊNG
    public static class MapRenderer
    {
        public static void Draw(Canvas layer, MapData map)
        {
            layer.Children.Clear();
            layer.Background = map.Background;

            var gridBrush = new SolidColorBrush(Color.FromArgb(0x14, 255, 255, 255));

            // Lưới mờ trên nền cho dễ nhìn
            for (double x = 64; x < MapLibrary.Width; x += 64)
                layer.Children.Add(new Line
                {
                    X1 = x, X2 = x, Y1 = 0, Y2 = MapLibrary.Height,
                    Stroke = gridBrush, StrokeThickness = 1
                });
            for (double y = 64; y < MapLibrary.Height; y += 64)
                layer.Children.Add(new Line
                {
                    X1 = 0, X2 = MapLibrary.Width, Y1 = y, Y2 = y,
                    Stroke = gridBrush, StrokeThickness = 1
                });

            // Vạch giữa sân
            layer.Children.Add(new Line
            {
                X1 = MapLibrary.Width / 2, X2 = MapLibrary.Width / 2,
                Y1 = 0, Y2 = MapLibrary.Height,
                Stroke = new SolidColorBrush(Color.FromArgb(0x30, 255, 255, 255)),
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 6, 6 }
            });

            // Hộp và tường
            foreach (var r in map.Obstacles)
            {
                var box = new Rectangle
                {
                    Width = r.Width,
                    Height = r.Height,
                    Fill = map.ObstacleFill,
                    Stroke = map.ObstacleBorder,
                    StrokeThickness = 2,
                    RadiusX = 3,
                    RadiusY = 3
                };
                Canvas.SetLeft(box, r.X);
                Canvas.SetTop(box, r.Y);
                layer.Children.Add(box);
            }
        }
    }
}
