using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Naruto
{
    public partial class MainWindow : Window
    {
        DispatcherTimer GameTimer = new DispatcherTimer();

        // --- BIẾN CỦA NARUTO ---
        int narutoRunFrame = 2;
        int narutoAttackFrame = 1;
        int narutoJumpFrame = 1;
        int narutoAttackTick = 0;
        bool narutoIsMoveLeft = false;
        bool narutoIsMoveRight = false;
        bool narutoIsAttacking = false;
        bool narutoIsJumping = false;

        // --- BIẾN CỦA SASUKE ---
        int sskRunFrame = 1;
        int sskAttackFrame = 1;
        int sskComboCount = 0;
        int sskJumpFrame = 1;
        int sskAttackTick = 0;
        bool sskIsMoveLeft = false;
        bool sskIsMoveRight = false;
        bool sskIsAttacking = false;
        bool sskIsJumping = false;

        public MainWindow()
        {
            InitializeComponent();
            GameTimer.Tick += GameTimer_Tick;
            GameTimer.Interval = TimeSpan.FromMilliseconds(50);
            GameTimer.Start();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // ================== LOGIC NARUTO ==================
            if (narutoIsAttacking)
            {
                narutoAttackTick++;
                if (narutoAttackTick >= 2)
                {
                    Player1.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/danh" + narutoAttackFrame + ".png"));
                    narutoAttackFrame++;
                    narutoAttackTick = 0;

                    if (narutoAttackFrame > 3) // Đánh từ danh1 đến danh3
                    {
                        narutoIsAttacking = false;
                        narutoAttackFrame = 1;
                        Player1.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/dung1.png"));
                    }
                }
            }
            else if (narutoIsJumping)
            {
                // Nhảy từ nhay1 đến nhay4 (2 hình đầu bay lên, 2 hình sau rớt xuống)
                if (narutoJumpFrame <= 2)
                    Canvas.SetTop(Player1, Canvas.GetTop(Player1) - 20);
                else
                    Canvas.SetTop(Player1, Canvas.GetTop(Player1) + 20);

                Player1.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/nhay" + narutoJumpFrame + ".png"));
                narutoJumpFrame++;
                if (narutoJumpFrame > 4) // Đã sửa thành nhay4 theo đúng ý bạn
                {
                    narutoIsJumping = false;
                    narutoJumpFrame = 1;
                    Player1.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/dung1.png"));
                }
            }
            else if (narutoIsMoveLeft)
            {
                Canvas.SetLeft(Player1, Canvas.GetLeft(Player1) - 10);
                Player1.RenderTransform = new ScaleTransform(-1, 1);
                Player1.RenderTransformOrigin = new Point(0.5, 0.5);

                if (narutoRunFrame < 2 || narutoRunFrame > 3) narutoRunFrame = 2;
                Player1.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/dung" + narutoRunFrame + ".png"));
                narutoRunFrame = (narutoRunFrame == 2) ? 3 : 2;
            }
            else if (narutoIsMoveRight)
            {
                Canvas.SetLeft(Player1, Canvas.GetLeft(Player1) + 10);
                Player1.RenderTransform = new ScaleTransform(1, 1);
                Player1.RenderTransformOrigin = new Point(0.5, 0.5);

                if (narutoRunFrame < 2 || narutoRunFrame > 3) narutoRunFrame = 2;
                Player1.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/dung" + narutoRunFrame + ".png"));
                narutoRunFrame = (narutoRunFrame == 2) ? 3 : 2;
            }

            // ================== LOGIC SASUKE ==================
            if (sskIsAttacking)
            {
                sskAttackTick++;
                if (sskAttackTick >= 2)
                {
                    Player2.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskdanh" + sskAttackFrame + ".png"));
                    sskAttackFrame++;
                    sskAttackTick = 0;

                    if (sskComboCount == 1 && sskAttackFrame > 3)
                    {
                        sskIsAttacking = false;
                        sskAttackFrame = 1;
                        sskComboCount = 0;
                        Player2.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskdung1.png"));
                    }
                    else if (sskComboCount >= 2 && sskAttackFrame > 7)
                    {
                        sskIsAttacking = false;
                        sskAttackFrame = 1;
                        sskComboCount = 0;
                        Player2.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskdung1.png"));
                    }
                }
            }
            else if (sskIsJumping)
            {
                if (sskJumpFrame <= 3)
                    Canvas.SetTop(Player2, Canvas.GetTop(Player2) - 20);
                else
                    Canvas.SetTop(Player2, Canvas.GetTop(Player2) + 20);

                Player2.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/ssknhay" + sskJumpFrame + ".png"));
                sskJumpFrame++;
                if (sskJumpFrame > 6)
                {
                    sskIsJumping = false;
                    sskJumpFrame = 1;
                    Player2.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskdung1.png"));
                }
            }
            else if (sskIsMoveLeft)
            {
                Canvas.SetLeft(Player2, Canvas.GetLeft(Player2) - 10);
                Player2.RenderTransform = new ScaleTransform(-1, 1);
                Player2.RenderTransformOrigin = new Point(0.5, 0.5);
                Player2.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskchay" + sskRunFrame + ".png"));
                sskRunFrame++;
                if (sskRunFrame > 6) sskRunFrame = 1;
            }
            else if (sskIsMoveRight)
            {
                Canvas.SetLeft(Player2, Canvas.GetLeft(Player2) + 10);
                Player2.RenderTransform = new ScaleTransform(1, 1);
                Player2.RenderTransformOrigin = new Point(0.5, 0.5);
                Player2.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskchay" + sskRunFrame + ".png"));
                sskRunFrame++;
                if (sskRunFrame > 6) sskRunFrame = 1;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A && !narutoIsAttacking && !narutoIsJumping) narutoIsMoveLeft = true;
            if (e.Key == Key.D && !narutoIsAttacking && !narutoIsJumping) narutoIsMoveRight = true;
            if (e.Key == Key.Space && !narutoIsAttacking && !narutoIsJumping) narutoIsJumping = true; // Phím Space nhảy
            if (e.Key == Key.J && !narutoIsJumping)
            {
                narutoIsAttacking = true;
                narutoAttackTick = 0;
            }

            if (e.Key == Key.Left && !sskIsAttacking && !sskIsJumping) sskIsMoveLeft = true;
            if (e.Key == Key.Right && !sskIsAttacking && !sskIsJumping) sskIsMoveRight = true;
            if (e.Key == Key.D5 && !sskIsAttacking && !sskIsJumping) sskIsJumping = true;
            if (e.Key == Key.D4 && !sskIsJumping)
            {
                sskIsAttacking = true;
                sskComboCount++;
                if (sskComboCount > 2) sskComboCount = 2;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                narutoIsMoveLeft = false;
                narutoRunFrame = 2;
                if (!narutoIsAttacking && !narutoIsJumping)
                    Player1.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/dung1.png"));
            }
            if (e.Key == Key.D)
            {
                narutoIsMoveRight = false;
                narutoRunFrame = 2;
                if (!narutoIsAttacking && !narutoIsJumping)
                    Player1.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/dung1.png"));
            }

            if (e.Key == Key.Left)
            {
                sskIsMoveLeft = false;
                sskRunFrame = 1;
                if (!sskIsAttacking && !sskIsJumping)
                    Player2.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskdung1.png"));
            }
            if (e.Key == Key.Right)
            {
                sskIsMoveRight = false;
                sskRunFrame = 1;
                if (!sskIsAttacking && !sskIsJumping)
                    Player2.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sskdung1.png"));
            }
        }
    }
}