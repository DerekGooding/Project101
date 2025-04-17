using Project1.Dungeon;

namespace Project1.Effects;

public class ParticleSystem
{
    private readonly List<Particle> _particles = [];
    private readonly Texture2D _particleTexture;
    private readonly Random _random = Random.Shared;
    private readonly GraphicsDevice _graphicsDevice;

    private const int MaxParticles = 500;

    public ParticleSystem(GraphicsDevice graphicsDevice)
    {
        // Create a simple particle texture
        _particleTexture = new Texture2D(graphicsDevice, 1, 1);
        _particleTexture.SetData([Color.White]);
        _graphicsDevice = graphicsDevice;
    }

    public void EmitBurst(Vector3 position, Vector3 direction, int count,
                         Color startColor, Color endColor, float speed = 1.0f)
    {
        // Clean up any inactive particles
        _particles.RemoveAll(p => !p.IsActive);

        // Limit to max particles
        var actualCount = Math.Min(count, MaxParticles - _particles.Count);

        for (var i = 0; i < actualCount; i++)
        {
            // Create random velocity around the direction
            var randomVelocity = new Vector3(
                (float)(_random.NextDouble() - 0.5f),
                (float)(_random.NextDouble() - 0.5f),
                (float)(_random.NextDouble() - 0.5f)
            );

            var velocity = direction + randomVelocity;
            velocity.Normalize();
            velocity *= speed * (0.5f + (float)_random.NextDouble());

            // Interpolate between start and end colors
            var colorFactor = (float)_random.NextDouble();
            var color = new Color(
                (int)MathHelper.Lerp(startColor.R, endColor.R, colorFactor),
                (int)MathHelper.Lerp(startColor.G, endColor.G, colorFactor),
                (int)MathHelper.Lerp(startColor.B, endColor.B, colorFactor)
            );

            var particle = new Particle
            {
                Position = position,
                Velocity = velocity,
                Color = color,
                Alpha = 1.0f,
                Scale = 0.1f + (float)_random.NextDouble() * 0.2f,
                Rotation = (float)_random.NextDouble() * MathHelper.TwoPi,
                RotationVelocity = (float)(_random.NextDouble() - 0.5f) * 2.0f,
                Lifetime = 0.5f + (float)_random.NextDouble() * 1.5f,
                Age = 0
            };

            _particles.Add(particle);
        }
    }

    public void Update(GameTime gameTime)
    {
        for (var i = _particles.Count - 1; i >= 0; i--)
        {
            _particles[i].Update(gameTime);

            // Remove expired particles
            if (!_particles[i].IsActive)
            {
                _particles.RemoveAt(i);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        if (_particles.Count == 0) return;

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);

        foreach (var particle in _particles)
        {
            // Convert 3D position to screen position
            var position = particle.Position;
            var screenPos = ProjectToScreen(position, camera);

            var color = particle.Color * particle.Alpha;

            var scale = particle.Scale * (1.0f - particle.Age / particle.Lifetime);

            spriteBatch.Draw(
                _particleTexture,
                screenPos,
                null,
                color,
                particle.Rotation,
                new Vector2(0.5f, 0.5f), // Origin at center
                scale,
                SpriteEffects.None,
                0.5f // Layer depth
            );
        }

        spriteBatch.End();
    }

    private Vector2 ProjectToScreen(Vector3 position, Camera camera)
    {
        // Project 3D point to 2D screen space
        var viewport = Matrix.CreateTranslation(-0.5f, -0.5f, 0) *
                      Matrix.CreateScale(
                          _graphicsDevice.Viewport.Width,
                          _graphicsDevice.Viewport.Height,
                          1);

        var worldPos = Vector3.Transform(position, camera.View);
        worldPos = Vector3.Transform(worldPos, camera.Projection);

        return new Vector2(
            (worldPos.X + 1.0f) / 2.0f * _graphicsDevice.Viewport.Width,
            (-worldPos.Y + 1.0f) / 2.0f * _graphicsDevice.Viewport.Height);
    }
}
