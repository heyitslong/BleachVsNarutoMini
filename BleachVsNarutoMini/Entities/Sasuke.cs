using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BleachVsNarutoMini.Entities
{
    public class Sasuke
    {
        // Hình ảnh đại diện cho Sasuke
        public Image Sprite { get; private set; }

        // Các biến trạng thái của riêng Sasuke
        private int runFrame = 1;
        private int attackFrame = 1;
        private int comboCount = 0;
        private int jumpFrame = 1;
        private int attackTick = 0;

        public bool IsMoveLeft = false;
        public bool IsMoveRight = false;
        public bool IsAttacking = false;
        public bool IsJumping = false;

        // Hàm khởi tạo
        public Sasuke()
        {
            Sprite = new Image { Width = 100, Height = 100 };

            // Tọa độ xuất phát bên phải
            Canvas.SetLeft(Sprite, 500);
            Canvas.SetTop(Sprite, 200);

            // Cài đặt tâm xoay và cho mặt mặc định quay sang trái nhìn Naruto
            Sprite.RenderTransformOrigin = new Point(0.5, 0.5);
            Sprite.RenderTransform = new ScaleTransform(-1, 1);

            Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskdung1.png"));
        }

        // ================== BỘ NÃO DI CHUYỂN ==================
        public void UpdateLogic()
        {
            if (IsAttacking)
            {
                attackTick++;
                if (attackTick >= 2)
                {
                    Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskdanh" + attackFrame + ".png"));
                    attackFrame++;
                    attackTick = 0;

                    // Logic Combo của riêng Sasuke
                    if (comboCount == 1 && attackFrame > 3)
                    {
                        IsAttacking = false;
                        attackFrame = 1;
                        comboCount = 0;
                        Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskdung1.png"));
                    }
                    else if (comboCount >= 2 && attackFrame > 7)
                    {
                        IsAttacking = false;
                        attackFrame = 1;
                        comboCount = 0;
                        Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskdung1.png"));
                    }
                }
            }
            else if (IsJumping)
            {
                // Nhảy 6 khung hình của Sasuke
                if (jumpFrame <= 3)
                    Canvas.SetTop(Sprite, Canvas.GetTop(Sprite) - 20);
                else
                    Canvas.SetTop(Sprite, Canvas.GetTop(Sprite) + 20);

                Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/ssknhay" + jumpFrame + ".png"));
                jumpFrame++;
                if (jumpFrame > 6)
                {
                    IsJumping = false;
                    jumpFrame = 1;
                    Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskdung1.png"));
                }
            }
            else if (IsMoveLeft)
            {
                Canvas.SetLeft(Sprite, Canvas.GetLeft(Sprite) - 10);
                Sprite.RenderTransform = new ScaleTransform(-1, 1);

                Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskchay" + runFrame + ".png"));
                runFrame++;
                if (runFrame > 6) runFrame = 1;
            }
            else if (IsMoveRight)
            {
                Canvas.SetLeft(Sprite, Canvas.GetLeft(Sprite) + 10);
                Sprite.RenderTransform = new ScaleTransform(1, 1);

                Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskchay" + runFrame + ".png"));
                runFrame++;
                if (runFrame > 6) runFrame = 1;
            }
        }

        // ================== XỬ LÝ PHÍM BẤM ==================
        public void HandleKeyDown(Key key)
        {
            if (key == Key.Left && !IsAttacking && !IsJumping) IsMoveLeft = true;
            if (key == Key.Right && !IsAttacking && !IsJumping) IsMoveRight = true;
            if (key == Key.D5 && !IsAttacking && !IsJumping) IsJumping = true; // Phím nhảy Numpad 5
            if (key == Key.D4 && !IsJumping) // Phím đánh Numpad 4
            {
                IsAttacking = true;
                comboCount++;
                if (comboCount > 2) comboCount = 2; // Giới hạn combo
            }
        }

        public void HandleKeyUp(Key key)
        {
            if (key == Key.Left)
            {
                IsMoveLeft = false;
                runFrame = 1;
                if (!IsAttacking && !IsJumping)
                    Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskdung1.png"));
            }
            if (key == Key.Right)
            {
                IsMoveRight = false;
                runFrame = 1;
                if (!IsAttacking && !IsJumping)
                    Sprite.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskdung1.png"));
            }
        }
    }
}