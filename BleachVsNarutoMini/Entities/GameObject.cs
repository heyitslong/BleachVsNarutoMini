namespace BleachVsNarutoMini.Entities
{
    public class GameObject
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Speed { get; set; }

        public GameObject(double x, double y, double width, double height, double speed)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Speed = speed;

        }
    }
}