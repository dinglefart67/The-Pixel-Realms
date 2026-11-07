using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class Arrow
{
    public Image Visual;
    public double Damage;

    public double WorldX;
    public double WorldY;

    public double VelocityX;
    public double VelocityY;

    private double gravity = 900;
    private double lifeTime = 0;
    private double flyStraightTime = 0.15;

    private RotateTransform rotation = new RotateTransform();

    public Arrow(Canvas canvas, double x, double y, double vx, double vy, double damage)
    {
        WorldX = x;
        WorldY = y;

        VelocityX = vx;
        VelocityY = vy;

        Damage = damage;

        Visual = new Image
        {
            Width = 54,
            Height = 6,
            Source = new BitmapImage(
                new Uri("Assets/Sprites/Projectiles/Arrow.png", UriKind.Relative)
            ),
            RenderTransformOrigin = new Point(0.5, 0.5)
        };

        Visual.RenderTransform = rotation;

        canvas.Children.Add(Visual);
    }

    public void Update(double deltaTime)
    {
        lifeTime += deltaTime;

        if (lifeTime > flyStraightTime)
        {
            VelocityY += gravity * deltaTime;
        }

        WorldX += VelocityX * deltaTime;
        WorldY += VelocityY * deltaTime;

        double angle = Math.Atan2(VelocityY, VelocityX) * 180 / Math.PI;
        rotation.Angle = angle;
    }

    public Rect GetRect()
    {
        return new Rect(WorldX, WorldY, Visual.Width, Visual.Height);
    }
}