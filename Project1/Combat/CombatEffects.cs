using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Project1.Combat;

public class CombatEffects
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _font;

    private Texture2D _hitFlashTexture;
    private float _hitFlashTimer;
    private SoundEffect? _attackSound;
    private SoundEffect? _hitSound;
    private SoundEffect? _enemyDeathSound;

    public CombatEffects(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
        _font = font;

        // Create hit flash texture
        _hitFlashTexture = new Texture2D(graphicsDevice, 1, 1);
        _hitFlashTexture.SetData([Color.White]);
    }

    public void LoadContent(ContentManager content)
    {
        _attackSound = content.Load<SoundEffect>("Sounds/attack");
        _hitSound = content.Load<SoundEffect>("Sounds/hit");
        _enemyDeathSound = content.Load<SoundEffect>("Sounds/enemy_death");
    }

    public void PlayAttackSound() => _attackSound?.Play(0.5f, 0, 0);

    public void PlayHitSound() => _hitSound?.Play(0.6f, 0, 0);

    public void PlayEnemyDeathSound() => _enemyDeathSound?.Play(0.7f, 0, 0);

    public void ShowHitFlash() => _hitFlashTimer = 0.1f; // Show hit flash for 0.1 seconds

    public void Update(GameTime gameTime)
    {
        if (_hitFlashTimer > 0)
        {
            _hitFlashTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public void DrawHitFlash()
    {
        if (_hitFlashTimer > 0)
        {
            var viewport = _graphicsDevice.Viewport;
            var alpha = _hitFlashTimer * 5; // Fade out
            _spriteBatch.Draw(_hitFlashTexture, new Rectangle(0, 0, viewport.Width, viewport.Height),
                Color.Red * Math.Min(0.3f, alpha));
        }
    }
}