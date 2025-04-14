using Microsoft.Xna.Framework;

namespace Project1;
public static class PointExtension
{
    public static Point Multiply(this Point point, int scalar) => new(point.X * scalar, point.Y * scalar);
}
