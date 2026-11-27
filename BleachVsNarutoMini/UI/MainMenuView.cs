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
        public string Image { get; set; } = "";
        public string BattlePose { get; set; } = "";
        public string Type { get; set; } = "";
        public int Attack { get; set; }
        public int Speed { get; set; }
        public int Defense { get; set; }
        public string Special { get; set; } = "";
        public string Ultimate { get; set; } = "";
        public string Description { get; set; } = "";
    }

    internal static class MenuKit
    {
        public static BitmapImage? LoadImage(string relativePath)
        {
            string cleanPath = relativePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
                ? relativePath
                : "Assets/Images/" + relativePath;

            var candidates = new List<string> { cleanPath };
            if (cleanPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                candidates.Add(cleanPath[..^4] + ".jpg");

            foreach (var path in candidates)
            {
                try
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri("pack://application:,,,/" + path, UriKind.Absolute);
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();
                    img.Freeze();
                    return img;
                }
                catch { }

                try
                {
                    string diskPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.Replace('/', '\\'));
                    if (System.IO.File.Exists(diskPath))
                    {
                        var img = new BitmapImage();
                        img.BeginInit();
                        img.UriSource = new Uri(diskPath, UriKind.Absolute);
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        img.EndInit();
                        img.Freeze();
                        return img;
                    }
                }
                catch { }
            }
            return null;
        }

        // Dải giấy cuộn cổ màu vàng nâu viền mực đen như thanh tiêu đề trong ảnh mẫu
        public static FrameworkElement MakeScrollBanner(string title, double width = 440, double fontSize = 34)
        {
            var container = new Grid
            {
                Width = width,
                Height = fontSize + 24,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Lớp bóng dưới giấy cuộn
            var shadow = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(14, 6), new Point(width, 2),
                    new Point(width - 14, fontSize + 22), new Point(0, fontSize + 18)
                },
                Fill = new SolidColorBrush(Color.FromArgb(0xCC, 0x0C, 0x09, 0x07))
            };
            container.Children.Add(shadow);

            // Thân giấy cuộn màu parchment trầm
            var parchment = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(16, 2), new Point(width - 2, 0),
                    new Point(width - 18, fontSize + 18), new Point(2, fontSize + 16)
                },
                Fill = new LinearGradientBrush(
                    Color.FromRgb(0xC9, 0xA8, 0x76),
                    Color.FromRgb(0x9E, 0x7E, 0x52),
                    new Point(0, 0), new Point(0, 1)),
                Stroke = new SolidColorBrush(Color.FromRgb(0x2B, 0x1D, 0x12)),
                StrokeThickness = 2
            };
            container.Children.Add(parchment);

            var text = new TextBlock
            {
                Text = title,
                FontFamily = UiKit.DisplayFont,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x15, 0x0E)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                RenderTransform = new SkewTransform(-8, 0),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            container.Children.Add(text);

            return container;
        }

        public static FrameworkElement MakeOutlinedText(string text, double size, Brush fill, Brush stroke, double skewX = -6)
        {
            var grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                RenderTransform = new SkewTransform(skewX, 0),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            var offsets = new[] { (-2, -1), (2, -1), (-2, 2), (2, 2) };
            foreach (var (ox, oy) in offsets)
            {
                grid.Children.Add(new TextBlock
                {
                    Text = text,
                    FontFamily = UiKit.DisplayFont,
                    FontSize = size,
                    FontWeight = FontWeights.Bold,
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
                FontWeight = FontWeights.Bold,
                Foreground = fill,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            return grid;
        }

        // Vẽ đinh tán kim loại 4 góc khung sắt như trong ảnh mẫu
        public static void AddCornerRivets(Grid target)
        {
            var alignments = new[]
            {
                (HorizontalAlignment.Left, VerticalAlignment.Top),
                (HorizontalAlignment.Right, VerticalAlignment.Top),
                (HorizontalAlignment.Left, VerticalAlignment.Bottom),
                (HorizontalAlignment.Right, VerticalAlignment.Bottom)
            };

            foreach (var (ha, va) in alignments)
            {
                target.Children.Add(new Ellipse
                {
                    Width = 7,
                    Height = 7,
                    Fill = new SolidColorBrush(Color.FromRgb(0x6E, 0x64, 0x5C)),
                    Stroke = new SolidColorBrush(Color.FromRgb(0x18, 0x15, 0x13)),
                    StrokeThickness = 1,
                    HorizontalAlignment = ha,
                    VerticalAlignment = va,
                    Margin = new Thickness(5)
                });
            }
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
                Background = new SolidColorBrush(Color.FromRgb(0x1E, 0x1C, 0x1B)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x4E, 0x46, 0x3F)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(3),
                Cursor = Cursors.Hand,
                Child = content
            };

            slot.MouseEnter += (_, _) =>
            {
                if (_selected != value)
                    slot.BorderBrush = new SolidColorBrush(Color.FromRgb(0xC8, 0x9B, 0x62));
            };

            slot.MouseLeave += (_, _) =>
            {
                if (_selected != value)
                    slot.BorderBrush = new SolidColorBrush(Color.FromRgb(0x4E, 0x46, 0x3F));
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
                    : new SolidColorBrush(Color.FromRgb(0x4E, 0x46, 0x3F));

                slot.Background = active
                    ? new SolidColorBrush(Color.FromArgb(0xFF, (byte)(accent.R / 3), (byte)(accent.G / 3), (byte)(accent.B / 3)))
                    : new SolidColorBrush(Color.FromRgb(0x1E, 0x1C, 0x1B));
            }
            Changed?.Invoke(value);
        }
    }

    // =========================================================================
    //  1. MÀN HÌNH CHỜ MỞ ĐẦU (DÙNG ẢNH TRONG FOLDER map)
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

            var bgImg = MenuKit.LoadImage("map/title_screen_bg.png");
            if (bgImg != null)
            {
                stage.Children.Add(new Image { Source = bgImg, Stretch = Stretch.Fill });
            }
            else
            {
                stage.Background = UiKit.DimBackground();
            }

            var buttonPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 22)
            };

            var btnPlay = UiKit.MakeButton("▶  BẮT ĐẦU CHƠI", (_, _) => PlayClicked?.Invoke(), new Thickness(0, 0, 0, 10), isPrimary: true);
            btnPlay.Width = 250;
            btnPlay.Height = 46;
            buttonPanel.Children.Add(btnPlay);

            var btnExit = UiKit.MakeButton("✕  THOÁT GAME", (_, _) => Application.Current.Shutdown(), new Thickness(0), isPrimary: false);
            btnExit.Width = 250;
            btnExit.Height = 40;
            buttonPanel.Children.Add(btnExit);

            stage.Children.Add(buttonPanel);

            Content = new Viewbox { Stretch = Stretch.Uniform, Child = stage };
        }

        // Nền đêm khói bụi + vầng trăng đỏ trầm đồng bộ với ảnh mẫu
        internal static Grid BuildScrollMoonBackdrop()
        {
            var root = new Grid
            {
                Background = new LinearGradientBrush(
                    Color.FromRgb(0x19, 0x18, 0x1A),
                    Color.FromRgb(0x0D, 0x0C, 0x0E),
                    new Point(0, 0), new Point(0, 1))
            };

            // Vầng trăng đỏ thẫm mờ phía sau tiêu đề
            var crimsonMoon = new Ellipse
            {
                Width = 340,
                Height = 340,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 30, 0, 0),
                IsHitTestVisible = false,
                Fill = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(0x55, 0x9E, 0x2A, 0x1B), 0.0),
                        new GradientStop(Color.FromArgb(0x28, 0x6E, 0x1B, 0x12), 0.65),
                        new GradientStop(Colors.Transparent, 1.0)
                    }
                }
            };
            root.Children.Add(crimsonMoon);

            // Lớp phủ tối viền (vignette)
            root.Children.Add(new Rectangle
            {
                IsHitTestVisible = false,
                Fill = new RadialGradientBrush
                {
                    Center = new Point(0.5, 0.5),
                    RadiusX = 0.85,
                    RadiusY = 0.85,
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Colors.Transparent, 0.4),
                        new GradientStop(Color.FromArgb(0x99, 0x07, 0x06, 0x08), 1.0)
                    }
                }
            });

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
    //  2. TRANG CHỌN NHÂN VẬT (BÁM SÁT ẢNH MẪU image_994dc6)
    // =========================================================================
    public class CharacterSelectView : UserControl
    {
        private const string AvatarFolder = "Assets/Images/Avatar/";

        public static readonly CharacterInfo[] CharacterList =
        {
            new CharacterInfo
            {
                Id = "Naruto",
                Name = "NARUTO UZUMAKI",
                Image = AvatarFolder + "naruto.png",
                BattlePose = "Naruto/dung/narutodung1.png",
                Type = "SHINOBI / MELEE",
                Attack = 4,
                Speed = 4,
                Defense = 4,
                Special = "RASENGAN (↓→ + J)",
                Ultimate = "ODAMA RASENGAN (↓→ + K)",
                Description = "Jinchuriki Cửu Vĩ với tốc độ áp sát dồn dập và sát thương cận chiến bùng nổ."
            },
            new CharacterInfo
            {
                Id = "Sasuke",
                Name = "SASUKE UCHIHA",
                Image = AvatarFolder + "sasuke.png",
                BattlePose = "Sasuke/dung/sasukedung1.png",
                Type = "SHINOBI / LIGHTNING",
                Attack = 4,
                Speed = 4,
                Defense = 4,
                Special = "CHIDORI (↓→ + J)",
                Ultimate = "KIRIN / SUSANOO (↓→ + K)",
                Description = "Tộc nhân Uchiha sở hữu nhãn thuật Sharingan sắc bén cùng lôi thuật chớp nhoáng."
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

        private string _p1Id;
        private string _p2Id;

        private readonly Dictionary<string, (Border Frame, Border P1Badge, Border P2Badge)> _gridCells = new();

        private readonly Image _p1Pose;
        private readonly TextBlock _p1Name;
        private readonly TextBlock _p1Type;
        private readonly TextBlock _p1Atk;
        private readonly TextBlock _p1Spd;
        private readonly TextBlock _p1Def;
        private readonly TextBlock _p1Spec;
        private readonly TextBlock _p1Ult;
        private readonly TextBlock _p1Desc;

        private readonly Image _p2Pose;
        private readonly TextBlock _p2Name;
        private readonly TextBlock _p2Type;
        private readonly TextBlock _p2Atk;
        private readonly TextBlock _p2Spd;
        private readonly TextBlock _p2Def;
        private readonly TextBlock _p2Spec;
        private readonly TextBlock _p2Ult;
        private readonly TextBlock _p2Desc;

        public CharacterSelectView(string lastP1 = "Naruto", string lastP2 = "Sasuke")
        {
            _p1Id = lastP1;
            _p2Id = lastP2;

            var root = TitleView.BuildScrollMoonBackdrop();

            var page = new Grid
            {
                Width = 980,
                Height = 600,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            page.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Tiêu đề giấy cuộn
            page.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 3 cột chính
            page.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Thanh nút đáy

            // --- HÀNG 1: TIÊU ĐỀ GIẤY CUỘN "SELECT CHARACTER" ---
            var banner = MenuKit.MakeScrollBanner("SELECT CHARACTER", 460, 36);
            banner.Margin = new Thickness(0, 4, 0, 10);
            Grid.SetRow(banner, 0);
            page.Children.Add(banner);

            // --- HÀNG 2: BỐ CỤC 3 CỘT [THẺ P1] - [LƯỚI SẮT 2x2 + VS] - [THẺ P2] ---
            var bodyGrid = new Grid { Margin = new Thickness(0, 4, 0, 8) };
            bodyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(285) });
            bodyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            bodyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(285) });

            // CỘT TRÁI: PLAYER 1 (Xanh chàm trầm)
            var p1Panel = BuildCharacterDossier(
                "PLAYER 1",
                Color.FromRgb(0x28, 0x3D, 0x52),
                Color.FromRgb(0x18, 0x25, 0x33),
                Color.FromRgb(0x5C, 0x8A, 0xB8),
                facingRight: true,
                out _p1Pose, out _p1Name, out _p1Type,
                out _p1Atk, out _p1Spd, out _p1Def,
                out _p1Spec, out _p1Ult, out _p1Desc);
            Grid.SetColumn(p1Panel, 0);
            bodyGrid.Children.Add(p1Panel);

            // CỘT GIỮA: HUY HIỆU VS + LƯỚI SẮT 2x2 ĐÓNG ĐINH TÁN
            var centerWrap = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var ironFrame = new Border
            {
                Margin = new Thickness(0, 22, 0, 0),
                Background = new LinearGradientBrush(
                    Color.FromRgb(0x2D, 0x28, 0x24),
                    Color.FromRgb(0x1B, 0x18, 0x16),
                    new Point(0, 0), new Point(0, 1)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x5E, 0x52, 0x47)),
                BorderThickness = new Thickness(3),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(16, 22, 16, 16),
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 16, ShadowDepth = 4, Opacity = 0.85 }
            };

            var ironContent = new Grid();
            MenuKit.AddCornerRivets(ironContent);

            var slotGrid = new Grid { Margin = new Thickness(6) };
            slotGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            slotGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            slotGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            slotGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Ô 1 (Trên trái): NARUTO
            var narutoCell = BuildRosterCell(CharacterList[0]);
            Grid.SetRow(narutoCell, 0);
            Grid.SetColumn(narutoCell, 0);
            slotGrid.Children.Add(narutoCell);

            // Ô 2 (Trên phải): SASUKE
            var sasukeCell = BuildRosterCell(CharacterList[1]);
            Grid.SetRow(sasukeCell, 0);
            Grid.SetColumn(sasukeCell, 1);
            slotGrid.Children.Add(sasukeCell);

            // Ô 3 (Dưới trái): Ô KHÓA
            var lockedCell = BuildLockedCell("🔒", "");
            Grid.SetRow(lockedCell, 1);
            Grid.SetColumn(lockedCell, 0);
            slotGrid.Children.Add(lockedCell);

            // Ô 4 (Dưới phải): COMING SOON
            var soonCell = BuildLockedCell("", "COMING\nSOON");
            Grid.SetRow(soonCell, 1);
            Grid.SetColumn(soonCell, 1);
            slotGrid.Children.Add(soonCell);

            ironContent.Children.Add(slotGrid);
            ironFrame.Child = ironContent;
            centerWrap.Children.Add(ironFrame);

            // Bảng gỗ/sắt chữ VS nằm vắt lên mép trên khung lưới
            var vsHeader = MenuKit.MakeScrollBanner("VS", 96, 26);
            vsHeader.VerticalAlignment = VerticalAlignment.Top;
            vsHeader.HorizontalAlignment = HorizontalAlignment.Center;
            centerWrap.Children.Add(vsHeader);

            Grid.SetColumn(centerWrap, 1);
            bodyGrid.Children.Add(centerWrap);

            // CỘT PHẢI: PLAYER 2 (Đỏ thẫm bụi)
            var p2Panel = BuildCharacterDossier(
                "PLAYER 2",
                Color.FromRgb(0x54, 0x25, 0x22),
                Color.FromRgb(0x33, 0x16, 0x14),
                Color.FromRgb(0xB8, 0x5C, 0x56),
                facingRight: false,
                out _p2Pose, out _p2Name, out _p2Type,
                out _p2Atk, out _p2Spd, out _p2Def,
                out _p2Spec, out _p2Ult, out _p2Desc);
            Grid.SetColumn(p2Panel, 2);
            bodyGrid.Children.Add(p2Panel);

            Grid.SetRow(bodyGrid, 1);
            page.Children.Add(bodyGrid);

            // --- HÀNG 3: THANH ĐIỀU HƯỚNG DƯỚI CÙNG ---
            var bottomBar = new Grid { Margin = new Thickness(0, 4, 0, 0) };

            var btnBack = UiKit.MakeButton("◄  TRỞ VỀ [ESC]", (_, _) => BackClicked?.Invoke(), new Thickness(0), isPrimary: false);
            btnBack.HorizontalAlignment = HorizontalAlignment.Left;
            btnBack.Width = 175;
            btnBack.Height = 40;
            bottomBar.Children.Add(btnBack);

            var btnNext = UiKit.MakeButton("CHỌN ĐẤU TRƯỜNG [ENTER]  ►", (_, _) => NextClicked?.Invoke(_p1Id, _p2Id), new Thickness(0), isPrimary: true);
            btnNext.HorizontalAlignment = HorizontalAlignment.Center;
            btnNext.Width = 270;
            btnNext.Height = 42;
            bottomBar.Children.Add(btnNext);

            var hintBox = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Color.FromRgb(0x1C, 0x19, 0x17)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x5E, 0x52, 0x47)),
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(12, 8, 12, 8),
                Child = new TextBlock
                {
                    Text = "P1: A/D   |   P2: ←/→",
                    FontFamily = UiKit.BodyFont,
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xD8, 0xC8, 0xB4)),
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            bottomBar.Children.Add(hintBox);

            Grid.SetRow(bottomBar, 2);
            page.Children.Add(bottomBar);

            // Hỗ trợ điều khiển bàn phím A/D cho P1 và Mũi tên trái/phải cho P2
            Focusable = true;
            Loaded += (_, _) => Keyboard.Focus(this);
            PreviewKeyDown += OnKeyNavigate;

            RefreshSelection();

            root.Children.Add(TitleView.WrapViewbox(page));
            Content = root;
        }

        private void OnKeyNavigate(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A || e.Key == Key.D)
            {
                _p1Id = _p1Id == "Naruto" ? "Sasuke" : "Naruto";
                RefreshSelection();
                e.Handled = true;
            }
            else if (e.Key == Key.Left || e.Key == Key.Right)
            {
                _p2Id = _p2Id == "Naruto" ? "Sasuke" : "Naruto";
                RefreshSelection();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                NextClicked?.Invoke(_p1Id, _p2Id);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                BackClicked?.Invoke();
                e.Handled = true;
            }
        }

        private Border BuildRosterCell(CharacterInfo info)
        {
            var cellGrid = new Grid();

            var avatar = new Image
            {
                Width = 128,
                Height = 128,
                Stretch = Stretch.UniformToFill,
                Source = MenuKit.LoadImage(info.Image)
            };
            RenderOptions.SetBitmapScalingMode(avatar, BitmapScalingMode.NearestNeighbor);
            cellGrid.Children.Add(avatar);

            // Nhãn P1 góc dưới trái
            var p1Badge = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x1E, 0x4E, 0x79)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x64, 0xB5, 0xF6)),
                BorderThickness = new Thickness(1.5),
                Padding = new Thickness(6, 2, 6, 2),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(4),
                Visibility = Visibility.Collapsed,
                Child = new TextBlock
                {
                    Text = "P1",
                    FontFamily = UiKit.BodyFont,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White
                }
            };
            cellGrid.Children.Add(p1Badge);

            // Nhãn P2 góc dưới phải
            var p2Badge = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x7A, 0x22, 0x22)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50)),
                BorderThickness = new Thickness(1.5),
                Padding = new Thickness(6, 2, 6, 2),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(4),
                Visibility = Visibility.Collapsed,
                Child = new TextBlock
                {
                    Text = "P2",
                    FontFamily = UiKit.BodyFont,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White
                }
            };
            cellGrid.Children.Add(p2Badge);

            var frame = new Border
            {
                Width = 134,
                Height = 134,
                Margin = new Thickness(6),
                Background = new SolidColorBrush(Color.FromRgb(0x18, 0x16, 0x15)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x52, 0x4A, 0x42)),
                BorderThickness = new Thickness(3),
                Cursor = Cursors.Hand,
                Child = cellGrid
            };

            // Click chuột trái chọn cho P1, chuột phải (hoặc click lần 2) chọn cho P2
            frame.MouseLeftButtonDown += (_, _) =>
            {
                _p1Id = info.Id;
                RefreshSelection();
            };
            frame.MouseRightButtonDown += (_, _) =>
            {
                _p2Id = info.Id;
                RefreshSelection();
            };

            _gridCells[info.Id] = (frame, p1Badge, p2Badge);
            return frame;
        }

        private static Border BuildLockedCell(string icon, string label)
        {
            var stack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!string.IsNullOrEmpty(icon))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = icon,
                    FontSize = 36,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x6E, 0x64, 0x5A)),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            }

            if (!string.IsNullOrEmpty(label))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = label,
                    FontFamily = UiKit.BodyFont,
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x7A, 0x6E, 0x62)),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            }

            return new Border
            {
                Width = 134,
                Height = 134,
                Margin = new Thickness(6),
                Background = new SolidColorBrush(Color.FromArgb(0xCC, 0x14, 0x12, 0x11)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x3D, 0x36, 0x30)),
                BorderThickness = new Thickness(2.5),
                Child = stack
            };
        }

        private static Border BuildCharacterDossier(
            string playerTag, Color topColor, Color bottomColor, Color borderColor, bool facingRight,
            out Image poseImg, out TextBlock nameText, out TextBlock typeText,
            out TextBlock atkText, out TextBlock spdText, out TextBlock defText,
            out TextBlock specText, out TextBlock ultText, out TextBlock descText)
        {
            var wrapper = new Grid();
            MenuKit.AddCornerRivets(wrapper);

            var stack = new StackPanel { Margin = new Thickness(14, 12, 14, 12) };

            // Nhãn PLAYER 1 / PLAYER 2 ở góc
            var headerTag = new TextBlock
            {
                Text = playerTag,
                FontFamily = UiKit.DisplayFont,
                FontSize = 18,
                Foreground = new SolidColorBrush(Color.FromRgb(0xE8, 0xDF, 0xD1)),
                HorizontalAlignment = facingRight ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                RenderTransform = new SkewTransform(-8, 0)
            };
            stack.Children.Add(headerTag);

            // Hình nhân vật đứng giữa phần trên thẻ
            poseImg = new Image
            {
                Width = 128,
                Height = 128,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 2, 0, 6),
                HorizontalAlignment = HorizontalAlignment.Center,
                RenderTransform = new ScaleTransform(facingRight ? 1 : -1, 1),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            RenderOptions.SetBitmapScalingMode(poseImg, BitmapScalingMode.NearestNeighbor);
            stack.Children.Add(poseImg);

            // Tên nhân vật chữ in nghiêng đậm
            nameText = new TextBlock
            {
                FontFamily = UiKit.DisplayFont,
                FontSize = 24,
                Foreground = new SolidColorBrush(Color.FromRgb(0xF5, 0xEC, 0xDF)),
                RenderTransform = new SkewTransform(-8, 0),
                Margin = new Thickness(0, 0, 0, 6)
            };
            stack.Children.Add(nameText);

            // Đường kẻ ngang phân tách
            stack.Children.Add(new Border
            {
                Height = 1.5,
                Background = new SolidColorBrush(Color.FromArgb(0x66, 0xE8, 0xDF, 0xD1)),
                Margin = new Thickness(0, 0, 0, 8)
            });

            // Các dòng chỉ số + sao vàng
            typeText = AddStatLine(stack, "⚔", "TYPE:", Brushes.White);
            atkText = AddStatLine(stack, "✦", "ATTACK:", new SolidColorBrush(Color.FromRgb(0xE5, 0xA9, 0x3B)));
            spdText = AddStatLine(stack, "➤", "SPEED:", new SolidColorBrush(Color.FromRgb(0xE5, 0xA9, 0x3B)));
            defText = AddStatLine(stack, "🛡", "DEFENSE:", new SolidColorBrush(Color.FromRgb(0xE5, 0xA9, 0x3B)));
            specText = AddStatLine(stack, "🌀", "SPECIAL:", new SolidColorBrush(Color.FromRgb(0xE8, 0xDF, 0xD1)));
            ultText = AddStatLine(stack, "🔥", "ULTIMATE:", new SolidColorBrush(Color.FromRgb(0xE8, 0xDF, 0xD1)));

            // Đoạn mô tả ngắn ở đáy thẻ
            descText = new TextBlock
            {
                FontFamily = UiKit.BodyFont,
                FontSize = 11.5,
                Foreground = new SolidColorBrush(Color.FromRgb(0xC2, 0xB8, 0xAA)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 8, 0, 0),
                LineHeight = 15
            };
            stack.Children.Add(descText);

            wrapper.Children.Add(stack);

            return new Border
            {
                Background = new LinearGradientBrush(topColor, bottomColor, new Point(0, 0), new Point(0, 1)),
                BorderBrush = new SolidColorBrush(borderColor),
                BorderThickness = new Thickness(2.5),
                CornerRadius = new CornerRadius(4),
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 14, ShadowDepth = 3, Opacity = 0.8 },
                Child = wrapper
            };
        }

        private static TextBlock AddStatLine(StackPanel parent, string icon, string label, Brush valueBrush)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            row.Children.Add(new TextBlock
            {
                Text = icon + " ",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0xC8, 0x9B, 0x62)),
                Width = 18
            });
            row.Children.Add(new TextBlock
            {
                Text = label + " ",
                FontFamily = UiKit.BodyFont,
                FontSize = 11.5,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0xD8, 0xCE, 0xC0))
            });
            var val = new TextBlock
            {
                FontFamily = UiKit.BodyFont,
                FontSize = 11.5,
                FontWeight = FontWeights.Bold,
                Foreground = valueBrush
            };
            row.Children.Add(val);
            parent.Children.Add(row);
            return val;
        }

        private static string FormatStars(int count)
        {
            int filled = Math.Max(0, Math.Min(5, count));
            return new string('★', filled) + new string('☆', 5 - filled);
        }

        private void RefreshSelection()
        {
            foreach (var kv in _gridCells)
            {
                bool isP1 = kv.Key == _p1Id;
                bool isP2 = kv.Key == _p2Id;
                var (frame, p1Badge, p2Badge) = kv.Value;

                p1Badge.Visibility = isP1 ? Visibility.Visible : Visibility.Collapsed;
                p2Badge.Visibility = isP2 ? Visibility.Visible : Visibility.Collapsed;

                if (isP1 && isP2)
                    frame.BorderBrush = new SolidColorBrush(Color.FromRgb(0xD4, 0xA0, 0x54));
                else if (isP1)
                    frame.BorderBrush = new SolidColorBrush(Color.FromRgb(0x5C, 0x8A, 0xB8));
                else if (isP2)
                    frame.BorderBrush = new SolidColorBrush(Color.FromRgb(0xB8, 0x5C, 0x56));
                else
                    frame.BorderBrush = new SolidColorBrush(Color.FromRgb(0x52, 0x4A, 0x42));
            }

            ApplyCharacterToDossier(_p1Id, _p1Pose, _p1Name, _p1Type, _p1Atk, _p1Spd, _p1Def, _p1Spec, _p1Ult, _p1Desc);
            ApplyCharacterToDossier(_p2Id, _p2Pose, _p2Name, _p2Type, _p2Atk, _p2Spd, _p2Def, _p2Spec, _p2Ult, _p2Desc);
        }

        private static void ApplyCharacterToDossier(
            string id, Image poseImg, TextBlock nameText, TextBlock typeText,
            TextBlock atkText, TextBlock spdText, TextBlock defText,
            TextBlock specText, TextBlock ultText, TextBlock descText)
        {
            foreach (var c in CharacterList)
            {
                if (c.Id != id) continue;
                poseImg.Source = MenuKit.LoadImage(c.BattlePose);
                nameText.Text = c.Name;
                typeText.Text = c.Type;
                atkText.Text = FormatStars(c.Attack);
                spdText.Text = FormatStars(c.Speed);
                defText.Text = FormatStars(c.Defense);
                specText.Text = c.Special;
                ultText.Text = c.Ultimate;
                descText.Text = c.Description;
                break;
            }
        }
    }

    // =========================================================================
    //  3. TRANG CHỌN ĐẤU TRƯỜNG & THỜI GIAN (ĐỒNG BỘ TÔNG SẮT GỈ / GIẤY CUỘN)
    // =========================================================================
    public class MapSelectView : UserControl
    {
        private static readonly string[] TimeValues = { "30", "60", "90", "0" };
        private static readonly string[] TimeLabels = { "30s", "60s", "90s", "∞" };

        public event Action<string, int>? StartBattleClicked;
        public event Action? BackClicked;

        private readonly ArcadeSlotGroup _mapGroup = new();
        private readonly ArcadeSlotGroup _timeGroup = new();

        public MapSelectView(string lastMap = "Arena1", int lastSeconds = 60)
        {
            var root = TitleView.BuildScrollMoonBackdrop();

            var panel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var banner = MenuKit.MakeScrollBanner("SELECT STAGE & TIME", 480, 36);
            banner.Margin = new Thickness(0, 0, 0, 18);
            panel.Children.Add(banner);

            // Khung sắt chứa 3 đấu trường
            var mapFrame = new Border
            {
                Background = new LinearGradientBrush(
                    Color.FromRgb(0x2B, 0x26, 0x22),
                    Color.FromRgb(0x19, 0x16, 0x14),
                    new Point(0, 0), new Point(0, 1)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x5E, 0x52, 0x47)),
                BorderThickness = new Thickness(3),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(18, 14, 18, 16),
                Margin = new Thickness(0, 0, 0, 14)
            };

            var mapGrid = new Grid();
            MenuKit.AddCornerRivets(mapGrid);
            var mapRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            mapRow.Children.Add(MakeMapCard("Arena1", "THUNG LŨNG TẬN CÙNG", "Assets/Images/map/arena1_bg.png", Color.FromRgb(0x2B, 0x2B, 0x3C)));
            mapRow.Children.Add(MakeMapCard("Arena2", "RỪNG CHẾT", "Assets/Images/map/arena2_bg.png", Color.FromRgb(0x2E, 0x3B, 0x23)));
            mapRow.Children.Add(MakeMapCard("Arena3", "HỌC VIỆN LÀNG LÁ", "Assets/Images/map/arena3_bg.png", Color.FromRgb(0x3E, 0x2F, 0x22)));
            mapGrid.Children.Add(mapRow);
            mapFrame.Child = mapGrid;
            panel.Children.Add(mapFrame);

            // Khung sắt chứa thời gian
            var timeFrame = new Border
            {
                Background = new LinearGradientBrush(
                    Color.FromRgb(0x2B, 0x26, 0x22),
                    Color.FromRgb(0x19, 0x16, 0x14),
                    new Point(0, 0), new Point(0, 1)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x5E, 0x52, 0x47)),
                BorderThickness = new Thickness(3),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(18, 10, 18, 12)
            };

            var timeGrid = new Grid();
            MenuKit.AddCornerRivets(timeGrid);
            var timeStack = new StackPanel();
            timeStack.Children.Add(new TextBlock
            {
                Text = "ROUND TIME",
                FontFamily = UiKit.DisplayFont,
                FontSize = 18,
                Foreground = new SolidColorBrush(Color.FromRgb(0xD8, 0xC8, 0xB4)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 6)
            });

            var timeRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            for (int i = 0; i < TimeValues.Length; i++)
            {
                var text = new TextBlock
                {
                    Text = TimeLabels[i],
                    FontFamily = UiKit.DisplayFont,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xEB, 0xDF, 0xCE)),
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                timeRow.Children.Add(_timeGroup.Add(TimeValues[i], text, 124, 42, Color.FromRgb(0xC8, 0x9B, 0x62)));
            }
            timeStack.Children.Add(timeRow);
            timeGrid.Children.Add(timeStack);
            timeFrame.Child = timeGrid;
            panel.Children.Add(timeFrame);

            _mapGroup.Select(Array.IndexOf(MapLibrary.AllIds, lastMap) >= 0 ? lastMap : "Arena1");
            string t = lastSeconds.ToString();
            _timeGroup.Select(Array.IndexOf(TimeValues, t) >= 0 ? t : "60");

            // Nút điều hướng đáy
            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 18, 0, 0)
            };

            var back = UiKit.MakeButton("◄  TRỞ VỀ", (_, _) => BackClicked?.Invoke(), new Thickness(0), isPrimary: false);
            back.Width = 170;
            back.Height = 42;

            var start = UiKit.MakeButton("⚔  VÀO TRẬN ĐẤU", (_, _) =>
            {
                if (!int.TryParse(_timeGroup.Value, out int seconds)) seconds = 60;
                StartBattleClicked?.Invoke(_mapGroup.Value, seconds);
            }, new Thickness(18, 0, 0, 0), isPrimary: true);
            start.Width = 230;
            start.Height = 42;

            buttons.Children.Add(back);
            buttons.Children.Add(start);
            panel.Children.Add(buttons);

            root.Children.Add(TitleView.WrapViewbox(panel));
            Content = root;
        }

        private Border MakeMapCard(string id, string caption, string imageFile, Color fallback)
        {
            var stack = new StackPanel();
            stack.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x16, 0x13, 0x11)),
                Padding = new Thickness(4, 5, 4, 5),
                Child = new TextBlock
                {
                    Text = caption,
                    FontFamily = UiKit.BodyFont,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xE8, 0xDF, 0xD1)),
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            });

            var thumbBox = new Border
            {
                Width = 204,
                Height = 112,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x3D, 0x36, 0x30)),
                BorderThickness = new Thickness(0, 1.5, 0, 0)
            };

            var src = MenuKit.LoadImage(imageFile);
            if (src != null)
                thumbBox.Child = new Image { Source = src, Stretch = Stretch.UniformToFill };
            else
                thumbBox.Child = new Rectangle { Fill = new SolidColorBrush(fallback) };

            stack.Children.Add(thumbBox);

            return _mapGroup.Add(id, stack, 210, 144, Color.FromRgb(0xC8, 0x9B, 0x62));
        }
    }
}