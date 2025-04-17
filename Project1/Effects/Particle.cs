namespace Project1.Effects;
public class Particle
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Scale;
    public float Rotation;
    public float RotationVelocity;
    public Color Color;
    public float Alpha;
    public float Lifetime;
    public float Age;

    public bool IsActive => Age < Lifetime;

    public void Update(GameTime gameTime)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Position += Velocity * delta;
        Rotation += RotationVelocity * delta;
        Age += delta;

        // Fade out as particle ages
        Alpha = MathHelper.Lerp(1.0f, 0.0f, Age / Lifetime);
    }
}
