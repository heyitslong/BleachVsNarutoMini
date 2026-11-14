using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace BleachVsNarutoMini.Entities
{
    public static class ImageCache
    {
        private static readonly Dictionary<string, BitmapImage> _cache = new();

        public static BitmapImage Get(string file)
        {
            if (!_cache.TryGetValue(file, out var img))
            {
                img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri($"pack://application:,,,/Assets/{file}");
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                img.Freeze();
                _cache[file] = img;
            }
            return img;
        }
    }
}