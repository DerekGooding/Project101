using Microsoft.Xna.Framework;

namespace Project1;

public class DungeonCamera
{
    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }

    public void Update(Vector3 position, Quaternion rotation)
    {
        var forward = Vector3.Transform(Vector3.Forward, rotation);
        var target = position + forward;
        var up = Vector3.Up;

        View = Matrix.CreateLookAt(position + new Vector3(0, 0.5f, 0), target, up);
        Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 16f / 9f, 0.1f, 100f);
    }
}