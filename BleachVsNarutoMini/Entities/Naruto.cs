using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BleachVsNarutoMini.Entities
{
    public class Naruto
    {
        // Hình ảnh đại diện cho Naruto trên màn hình
        public Image Sprite { get; private set; }

        // Các biến trạng thái (bỏ chữ "naruto" ở đầu đi vì nó đã nằm trong class Naruto rồi)
        private int runFrame = 2;
        private int attackFrame = 1;
        private int jumpFrame = 1;
        private int attackTick = 0;

        public bool IsMoveLeft = false;
        public bool IsMoveRight = false;
        public bool IsAttacking = false;
        public bool IsJumping = false;

        // Hàm khởi tạo: Chạy ngay khi tạo ra Naruto
        public Naruto()
        {
            Sprite = new Image { Width = 150, Height = 150 };

            // Tọa độ xuất phát
            Canvas.SetLeft(Sprite, 100);
            Canvas.SetTop(Sprite, 180);

            // Ảnh đứng im mặc định
            Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/dung1.png"));
        }

        // ================== BỘ NÃO DI CHUYỂN ==================
        public void UpdateLogic()
        {
            if (IsAttacking)
            {
                attackTick++;
                if (attackTick >= 2)
                {
                    Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/danh" + attackFrame + ".png"));
                    attackFrame++;
                    attackTick = 0;

                    if (attackFrame > 3) // Đánh từ danh1 đến danh3
                    {
                        IsAttacking = false;
                        attackFrame = 1;
                        Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/dung1.png"));
                    }
                }
            }
            else if (IsJumping)
            {
                // Nhảy từ nhay1 đến nhay4
                if (jumpFrame <= 2)
                    Canvas.SetTop(Sprite, Canvas.GetTop(Sprite) - 20);
                else
                    Canvas.SetTop(Sprite, Canvas.GetTop(Sprite) + 20);

                Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/nhay" + jumpFrame + ".png"));
                jumpFrame++;
                if (jumpFrame > 4)
                {
                    IsJumping = false;
                    jumpFrame = 1;
                    Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/dung1.png"));
                }
            }
            else if (IsMoveLeft)
            {
                Canvas.SetLeft(Sprite, Canvas.GetLeft(Sprite) - 10);
                Sprite.RenderTransform = new ScaleTransform(-1, 1);
                Sprite.RenderTransformOrigin = new Point(0.5, 0.5);

                if (runFrame < 2 || runFrame > 3) runFrame = 2;
                Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/dung" + runFrame + ".png"));
                runFrame = (runFrame == 2) ? 3 : 2;
            }
            else if (IsMoveRight)
            {
                Canvas.SetLeft(Sprite, Canvas.GetLeft(Sprite) + 10);
                Sprite.RenderTransform = new ScaleTransform(1, 1);
                Sprite.RenderTransformOrigin = new Point(0.5, 0.5);

                if (runFrame < 2 || runFrame > 3) runFrame = 2;
                Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/dung" + runFrame + ".png"));
                runFrame = (runFrame == 2) ? 3 : 2;
            }
        }

        // ================== XỬ LÝ PHÍM BẤM ==================
        public void HandleKeyDown(Key key)
        {
            if (key == Key.A && !IsAttacking && !IsJumping) IsMoveLeft = true;
            if (key == Key.D && !IsAttacking && !IsJumping) IsMoveRight = true;
            if (key == Key.Space && !IsAttacking && !IsJumping) IsJumping = true;
            if (key == Key.J && !IsJumping)
            {
                IsAttacking = true;
                attackTick = 0;
            }
        }

        public void HandleKeyUp(Key key)
        {
            if (key == Key.A)
            {
                IsMoveLeft = false;
                runFrame = 2;
                if (!IsAttacking && !IsJumping)
                    Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/dung1.png"));
            }
            if (key == Key.D)
            {
                IsMoveRight = false;
                runFrame = 2;
                if (!IsAttacking && !IsJumping)
                    Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/dung1.png"));
            }
        }
    }
}