using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BleachVsNarutoMini.UI
{
    public class CharacterInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Image { get; set; } = "";        // Ảnh avatar ô lưới
        public string BattlePose { get; set; } = "";   // Ảnh dáng đứng pixel
        public string Special { get; set; } = "";
        public int Attack { get; set; }
        public int Throw { get; set; }
        public int Ult { get; set; }
    }

    internal static class MenuKit
    {
        public static BitmapImage? LoadImage(string relativePath)
        {
            try
            {
                string path = relativePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
                    ? relativePath
                    : "Assets/Images/" + relativePath;

                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri("pack://application:,,,/" + path, UriKind.Absolute);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                img.Freeze();
                return img;
            }
            catch
            {
                return null;
            }
        }

        public static FrameworkElement MakeOutlinedText(string text, double size, Brush fill, Brush stroke,
            double strokeThickness, Color glowColor, double skewX = -8)
        {
            var grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                RenderTransform = new SkewTransform(skewX, 0),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            var offsets = new[] { (-3, -2), (3, -2), (-3, 3), (3, 3), (0, 3) };
            foreach (var (ox, oy) in offsets)
            {
                grid.Children.Add(new TextBlock
                {
                    Text = text,
                    FontFamily = UiKit.DisplayFont,
                    FontSize = size,
                    FontWeight = FontWeights.Black,
                    Foreground = stroke,
                    Margin = new Thickness(ox, oy, -ox, -oy),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            }

            grid.Children.Add(new TextBlock
            {
                Text = text,
                FontFamily = UiKit.DisplayFont,
                FontSize = size,
                FontWeight = FontWeights.Black,
                Foreground = fill,
                HorizontalAlignment = HorizontalAlignment.Center,
                Effect = new DropShadowEffect
                {
                    Color = glowColor,
                    BlurRadius = 18,
                    ShadowDepth = 0,
                    Opacity = 0.8
                }
            });
            return grid;
        }
    }

    internal class ArcadeSlotGroup
    {
        private readonly Dictionary<string, (Border Slot, Color Accent)> _slots = new();
        private string _selected = "";
        public string Value => _selected;
        public event Action<string>? Changed;

        public Border Add(string value, UIElement content, double width, double height, Color accentColor)
        {
            var slot = new Border
            {
                Width = width,
                Height = height,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromArgb(0xE0, 0x12, 0x11, 0x16)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x4D)),
                BorderThickness = new Thickness(2),
                Cursor = Cursors.Hand,
                Child = content
            };

            slot.MouseEnter += (_, _) =>
            {
                if (_selected != value)
                    slot.BorderBrush = Brushes.White;
            };

            slot.MouseLeave += (_, _) =>
            {
                if (_selected != value)
                    slot.BorderBrush = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x4D));
            };

            slot.MouseLeftButtonDown += (_, _) => Select(value);
            _slots[value] = (slot, accentColor);
            return slot;
        }

        public void Select(string value)
        {
            if (!_slots.ContainsKey(value)) return;
            _selected = value;

            foreach (var kv in _slots)
            {
                bool active = kv.Key == value;
                var slot = kv.Value.Slot;
                var accent = kv.Value.Accent;

                slot.BorderThickness = new Thickness(active ? 3 : 2);
                slot.BorderBrush = active
                    ? new SolidColorBrush(accent)
                    : new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x4D));

                slot.Background = active
                    ? new SolidColorBrush(Color.FromArgb(0xFA, (byte)(accent.R / 3.5), (byte)(accent.G / 3.5), (byte)(accent.B / 3.5)))
                    : new SolidColorBrush(Color.FromArgb(0xE0, 0x12, 0x11, 0x16));

                slot.Effect = active
                    ? new DropShadowEffect { Color = accent, BlurRadius = 12, ShadowDepth = 0, Opacity = 0.85 }
                    : null;
            }
            Changed?.Invoke(value);
        }
    }

    // =========================================================================
    //  1. MÀN HÌNH CHỜ MỞ ĐẦU (ĐÃ GẮN ẢNH TỐI NGẦU cSaacWc_Wu)
    // =========================================================================
    public class TitleView : UserControl
    {
        public event Action? PlayClicked;

        public TitleView()
        {
            var stage = new Grid
            {
                Width = MapLibrary.Width,
                Height = MapLibrary.Height,
                ClipToBounds = true
            };

            // Nạp bức ảnh dark anime clash cSaacWc_Wu làm nền
            var bgImg = MenuKit.LoadImage("map/title_screen_bg.png");
            if (bgImg != null)
            {
                stage.Children.Add(new Image { Source = bgImg, Stretch = Stretch.Fill });
            }
            else
            {
                stage.Background = UiKit.DimBackground();
            }

            // Phủ lớp tối bụi bặm (vignette khói)
            var smokeOverlay = new Rectangle
            {
                Fill = new RadialGradientBrush
                {
                    Center = new Point(0.5, 0.5),
                    RadiusX = 0.85,
                    RadiusY = 0.85,
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(0x11, 0, 0, 0), 0.0),
                        new GradientStop(Color.FromArgb(0x99, 0x05, 0x04, 0x06), 1.0)
                    }
                },
                IsHitTestVisible = false
            };
            stage.Children.Add(smokeOverlay);

            // Cụm nút bấm đặt khéo léo bên dưới để không che nhân vật
            var buttonPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 24)
            };

            var btnPlay = UiKit.MakeButton("▶  BẮT ĐẦU CHƠI", (_, _) => PlayClicked?.Invoke(), new Thickness(0, 0, 0, 10), isPrimary: true);
            btnPlay.Width = 260;
            btnPlay.Height = 48;
            buttonPanel.Children.Add(btnPlay);

            var btnExit = UiKit.MakeButton("✕  THOÁT GAME", (_, _) => Application.Current.Shutdown(), new Thickness(0), isPrimary: false);
            btnExit.Width = 260;
            btnExit.Height = 42;
            buttonPanel.Children.Add(btnExit);

            stage.Children.Add(buttonPanel);

            Content = new Viewbox
            {
                Stretch = Stretch.Uniform,
                Child = stage
            };
        }

        internal static Grid BuildDarkBackdrop()
        {
            var root = new Grid
            {
                Background = new LinearGradientBrush(
                    Color.FromRgb(0x13, 0x11, 0x16),
                    Color.FromRgb(0x0A, 0x09, 0x0C),
                    new Point(0, 0), new Point(1, 1))
            };

            // Tạo các mảng chéo tối kiểu chiến trường gỉ
            var poly = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(0, 0), new Point(480, 0), new Point(220, 640), new Point(0, 640)
                },
                Fill = new SolidColorBrush(Color.FromArgb(0x12, 0x9E, 0x3D, 0x0E))
            };
            root.Children.Add(poly);
            return root;
        }

        internal static Viewbox WrapViewbox(UIElement child) => new()
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            Margin = new Thickness(16),
            Child = child
        };
    }

    // =========================================================================
    //  2. TRANG CHỌN NHÂN VẬT (BỐ CỤC LƯỚI Ở GIỮA, 2 TƯỚNG TỐI NGẦU HAI BÊN)
    // =========================================================================
    public class CharacterSelectView : UserControl
    {
        private const string AvatarFolder = "Assets/Images/Avatar/";

        public static readonly CharacterInfo[] CharacterList =
        {
            new CharacterInfo
            {
                Id = "Naruto", Name = "UZUMAKI NARUTO",
                Image = AvatarFolder + "naruto.png",
                BattlePose = "Naruto/dung/narutodung1.png",
                Special = "RASENGAN", Attack = 5, Throw = 12, Ult = 40
            },
            new CharacterInfo
            {
                Id = "Sasuke", Name = "UCHIHA SASUKE",
                Image = AvatarFolder + "sasuke.png",
                BattlePose = "Sasuke/dung/sasukedung1.png",
                Special = "CHIDORI", Attack = 6, Throw = 18, Ult = 32
            }
        };

        public static string DisplayName(string id)
        {
            foreach (var c in CharacterList)
                if (c.Id == id) return c.Id;
            return id;
        }

        public event Action<string, string>? NextClicked;
        public event Action? BackClicked;

        private readonly ArcadeSlotGroup _p1Group = new();
        private readonly ArcadeSlotGroup _p2Group = new();
        private readonly Image _p1BigSprite;
        private readonly Image _p2BigSprite;
        private readonly TextBlock _p1NameLabel;
        private readonly TextBlock _p2NameLabel;
        private readonly TextBlock _p1SkillLabel;
        private readonly TextBlock _p2SkillLabel;

        public CharacterSelectView(string lastP1 = "Naruto", string lastP2 = "Sasuke")
        {
            var root = TitleView.BuildDarkBackdrop();

            var mainLayout = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var header = MenuKit.MakeOutlinedText("SELECT CHARACTER", 40,
                new SolidColorBrush(Color.FromRgb(0xD8, 0x43, 0x15)), // Cam tối gỉ
                Brushes.Black, 3, Color.FromRgb(0x7E, 0x1B, 0x00), -10);
            header.Margin = new Thickness(0, 0, 0, 12);
            mainLayout.Children.Add(header);

            var arenaGrid = new Grid { HorizontalAlignment = HorizontalAlignment.Center };
            arenaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(270) });
            arenaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            arenaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(270) });

            // Cột P1 (Xanh Thép tối)
            var p1Side = BuildHeroStand(
                "PLAYER 1  [W A S D]",
                Color.FromRgb(0x19, 0x76, 0xD2),
                facingRight: true,
                out _p1BigSprite, out _p1NameLabel, out _p1SkillLabel);
            Grid.SetColumn(p1Side, 0);
            arenaGrid.Children.Add(p1Side);

            // Khung lưới chọn ô kim loại tối
            var centerBox = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0xF2, 0x12, 0x10, 0x16)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x4D)),
                BorderThickness = new Thickness(2),
                Padding = new Thickness(16, 12, 16, 14),
                Margin = new Thickness(12, 0, 12, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var centerStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

            centerStack.Children.Add(MakeGridHeader("1P SELECT", Color.FromRgb(0x29, 0xB6, 0xF6)));
            centerStack.Children.Add(BuildCharacterSlots(_p1Group, Color.FromRgb(0x29, 0xB6, 0xF6)));

            var vs = MenuKit.MakeOutlinedText("VS", 36,
                new LinearGradientBrush(Color.FromRgb(0xE6, 0x51, 0x00), Color.FromRgb(0xBF, 0x36, 0x0C), new Point(0, 0), new Point(0, 1)),
                Brushes.Black, 3, Color.FromRgb(0xB7, 0x1C, 0x1C), -14);
            vs.Margin = new Thickness(0, 6, 0, 6);
            centerStack.Children.Add(vs);

            centerStack.Children.Add(MakeGridHeader("2P SELECT", Color.FromRgb(0xEF, 0x53, 0x50)));
            centerStack.Children.Add(BuildCharacterSlots(_p2Group, Color.FromRgb(0xEF, 0x53, 0x50)));

            centerBox.Child = centerStack;
            Grid.SetColumn(centerBox, 1);
            arenaGrid.Children.Add(centerBox);

            // Cột P2 (Đỏ Máu tối)
            var p2Side = BuildHeroStand(
                "PLAYER 2  [ARROWS]",
                Color.FromRgb(0xC6, 0x28, 0x28),
                facingRight: false,
                out _p2BigSprite, out _p2NameLabel, out _p2SkillLabel);
            Grid.SetColumn(p2Side, 2);
            arenaGrid.Children.Add(p2Side);

            mainLayout.Children.Add(arenaGrid);

            _p1Group.Changed += id => UpdateHeroStand(id, _p1BigSprite, _p1NameLabel, _p1SkillLabel);
            _p2Group.Changed += id => UpdateHeroStand(id, _p2BigSprite, _p2NameLabel, _p2SkillLabel);
            _p1Group.Select(lastP1);
            _p2Group.Select(lastP2);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 18, 0, 0)
            };

            var back = UiKit.MakeButton("◄ QUAY LẠI", (_, _) => BackClicked?.Invoke(), new Thickness(0), isPrimary: false);
            back.Width = 170;
            back.Height = 44;

            var next = UiKit.MakeButton("CHỌN ĐẤU TRƯỜNG ►", (_, _) => NextClicked?.Invoke(_p1Group.Value, _p2Group.Value), new Thickness(16, 0, 0, 0), isPrimary: true);
            next.Width = 230;
            next.Height = 44;

            buttons.Children.Add(back);
            buttons.Children.Add(next);
            mainLayout.Children.Add(buttons);

            root.Children.Add(TitleView.WrapViewbox(mainLayout));
            Content = root;
        }

        private static TextBlock MakeGridHeader(string text, Color color) => new()
        {
            Text = text,
            FontFamily = UiKit.BodyFont,
            FontSize = 13,
            Foreground = new SolidColorBrush(color),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 4)
        };

        private static StackPanel BuildCharacterSlots(ArcadeSlotGroup group, Color accent)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            foreach (var c in CharacterList)
            {
                var img = new Image
                {
                    Width = 68,
                    Height = 68,
                    Stretch = Stretch.UniformToFill,
                    Source = MenuKit.LoadImage(c.Image)
                };
                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
                row.Children.Add(group.Add(c.Id, img, 76, 76, accent));
            }
            return row;
        }

        private static Border BuildHeroStand(string tag, Color accent, bool facingRight,
            out Image bigSprite, out TextBlock nameLabel, out TextBlock skillLabel)
        {
            var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

            var tagBar = new Border
            {
                Background = new SolidColorBrush(accent),
                Padding = new Thickness(12, 3, 12, 3),
                RenderTransform = new SkewTransform(-10, 0),
                RenderTransformOrigin = new Point(0.5, 0.5),
                Child = new TextBlock
                {
                    Text = tag,
                    FontFamily = UiKit.BodyFont,
                    FontSize = 12,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };
            stack.Children.Add(tagBar);

            var stageBox = new Grid { Height = 190, Margin = new Thickness(0, 6, 0, 4) };
            var floorGlow = new Ellipse
            {
                Width = 140,
                Height = 22,
                VerticalAlignment = VerticalAlignment.Bottom,
                Fill = new RadialGradientBrush(Color.FromArgb(0x55, accent.R, accent.G, accent.B), Colors.Transparent)
            };
            bigSprite = new Image
            {
                Width = 175,
                Height = 175,
                Stretch = Stretch.Uniform,
                Source = MenuKit.LoadImage("Naruto/dung/narutodung1.png"),
                VerticalAlignment = VerticalAlignment.Bottom,
                RenderTransform = new ScaleTransform(facingRight ? 1 : -1, 1),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            RenderOptions.SetBitmapScalingMode(bigSprite, BitmapScalingMode.NearestNeighbor);
            stageBox.Children.Add(floorGlow);
            stageBox.Children.Add(bigSprite);
            stack.Children.Add(stageBox);

            nameLabel = new TextBlock
            {
                FontFamily = UiKit.DisplayFont,
                FontSize = 22,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            skillLabel = new TextBlock
            {
                FontFamily = UiKit.BodyFont,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x6C, 0x00)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0)
            };
            stack.Children.Add(nameLabel);
            stack.Children.Add(skillLabel);

            return new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0xC8, 0x10, 0x0E, 0x13)),
                BorderBrush = new SolidColorBrush(accent),
                BorderThickness = new Thickness(2),
                Padding = new Thickness(12),
                Child = stack
            };
        }

        private static void UpdateHeroStand(string id, Image sprite, TextBlock nameLabel, TextBlock skillLabel)
        {
            foreach (var c in CharacterList)
            {
                if (c.Id != id) continue;
                sprite.Source = MenuKit.LoadImage(c.BattlePose);
                nameLabel.Text = c.Name;
                skillLabel.Text = "ULT: " + c.Special + "  |  ATK " + c.Attack + "  |  THROW " + c.Throw;
                break;
            }
        }
    }

    // =========================================================================
    //  3. TRANG CHỌN ĐẤU TRƯỜNG & THỜI GIAN
    // =========================================================================
    public class MapSelectView : UserControl
    {
        private static readonly string[] TimeValues = { "30", "60", "90", "0" };
        private static readonly string[] TimeLabels = { "30s", "60s", "90s", "∞ VÔ HẠN" };

        public event Action<string, int>? StartBattleClicked;
        public event Action? BackClicked;

        private readonly ArcadeSlotGroup _mapGroup = new();
        private readonly ArcadeSlotGroup _timeGroup = new();

        public MapSelectView(string lastMap = "Arena1", int lastSeconds = 60)
        {
            var root = TitleView.BuildDarkBackdrop();

            var panel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var header = MenuKit.MakeOutlinedText("STAGE & TIME SELECT", 40,
                new SolidColorBrush(Color.FromRgb(0xD8, 0x43, 0x15)),
                Brushes.Black, 3, Color.FromRgb(0x7E, 0x1B, 0x00), -10);
            header.Margin = new Thickness(0, 0, 0, 16);
            panel.Children.Add(header);

            // Khung chọn Đấu trường
            var mapBox = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0xEE, 0x12, 0x10, 0x16)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x4D)),
                BorderThickness = new Thickness(2),
                Padding = new Thickness(16, 12, 16, 14),
                Margin = new Thickness(0, 0, 0, 14)
            };

            var mapStack = new StackPanel();
            mapStack.Children.Add(MakeBarTitle("CHỌN ĐẤU TRƯỜNG (STAGE)", Color.FromRgb(0xFF, 0xB3, 0x00)));
            var mapRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            mapRow.Children.Add(MakeMapCard("Arena1", "THUNG LŨNG TẬN CÙNG", "Assets/Images/map/arena1_bg.png", Color.FromRgb(0x2B, 0x2B, 0x3C)));
            mapRow.Children.Add(MakeMapCard("Arena2", "RỪNG CHẾT", "Assets/Images/map/arena2_bg.png", Color.FromRgb(0x2E, 0x3B, 0x23)));
            mapRow.Children.Add(MakeMapCard("Arena3", "HỌC VIỆN LÀNG LÁ", "Assets/Images/map/arena3_bg.png", Color.FromRgb(0x3E, 0x2F, 0x22)));
            mapStack.Children.Add(mapRow);
            mapBox.Child = mapStack;
            panel.Children.Add(mapBox);

            // Khung chọn Thời gian
            var timeBox = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0xEE, 0x12, 0x10, 0x16)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x29, 0xB6, 0xF6)),
                BorderThickness = new Thickness(2),
                Padding = new Thickness(16, 10, 16, 12)
            };

            var timeStack = new StackPanel();
            timeStack.Children.Add(MakeBarTitle("THỜI GIAN TRẬN ĐẤU (TIME)", Color.FromRgb(0x29, 0xB6, 0xF6)));
            var timeRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            for (int i = 0; i < TimeValues.Length; i++)
            {
                var text = new TextBlock
                {
                    Text = TimeLabels[i],
                    FontFamily = UiKit.BodyFont,
                    Foreground = Brushes.White,
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                timeRow.Children.Add(_timeGroup.Add(TimeValues[i], text, 120, 40, Color.FromRgb(0x29, 0xB6, 0xF6)));
            }
            timeStack.Children.Add(timeRow);
            timeBox.Child = timeStack;
            panel.Children.Add(timeBox);

            _mapGroup.Select(Array.IndexOf(MapLibrary.AllIds, lastMap) >= 0 ? lastMap : "Arena1");
            string t = lastSeconds.ToString();
            _timeGroup.Select(Array.IndexOf(TimeValues, t) >= 0 ? t : "60");

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 18, 0, 0)
            };

            var back = UiKit.MakeButton("◄ ĐỔI NHÂN VẬT", (_, _) => BackClicked?.Invoke(), new Thickness(0), isPrimary: false);
            back.Width = 190;
            back.Height = 44;

            var start = UiKit.MakeButton("⚔  VÀO TRẬN ĐẤU", (_, _) =>
            {
                if (!int.TryParse(_timeGroup.Value, out int seconds)) seconds = 60;
                StartBattleClicked?.Invoke(_mapGroup.Value, seconds);
            }, new Thickness(16, 0, 0, 0), isPrimary: true);
            start.Width = 230;
            start.Height = 44;

            buttons.Children.Add(back);
            buttons.Children.Add(start);
            panel.Children.Add(buttons);

            root.Children.Add(TitleView.WrapViewbox(panel));
            Content = root;
        }

        private static TextBlock MakeBarTitle(string text, Color accent) => new()
        {
            Text = text,
            FontFamily = UiKit.BodyFont,
            Foreground = new SolidColorBrush(accent),
            FontSize = 13,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };

        private Border MakeMapCard(string id, string caption, string imageFile, Color fallback)
        {
            var stack = new StackPanel();
            var thumbBox = new Border
            {
                Width = 196,
                Height = 108,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };

            var src = MenuKit.LoadImage(imageFile);
            if (src != null)
                thumbBox.Child = new Image { Source = src, Stretch = Stretch.UniformToFill };
            else
                thumbBox.Child = new Rectangle { Fill = new SolidColorBrush(fallback) };

            stack.Children.Add(thumbBox);
            stack.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0xEE, 0x0A, 0x09, 0x0D)),
                Padding = new Thickness(4, 5, 4, 5),
                Child = new TextBlock
                {
                    Text = caption,
                    FontFamily = UiKit.BodyFont,
                    Foreground = Brushes.White,
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            });

            return _mapGroup.Add(id, stack, 202, 140, Color.FromRgb(0xFF, 0xB3, 0x00));
        }
    }
}