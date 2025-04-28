using Project1.Dungeon;
using Project1.Inventory;

namespace Project1.Combat;

public class CombatManager
{
    private readonly Player _player;
    private readonly Controller _controller;
    private readonly EnemyManager _enemyManager;
    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _font;
    private readonly Texture2D _damageTexture;

    private readonly List<DamageIndicator> _damageIndicators = [];

    public CombatManager(Player player, Controller controller, EnemyManager enemyManager, SpriteBatch spriteBatch, SpriteFont font, GraphicsDevice graphicsDevice)
    {
        _player = player;
        _controller = controller;
        _enemyManager = enemyManager;
        _spriteBatch = spriteBatch;
        _font = font;

        // Create a white pixel texture for damage indicators
        _damageTexture = new Texture2D(graphicsDevice, 1, 1);
        _damageTexture.SetData([Color.White]);
    }

    public void Update(GameTime gameTime, KeyboardState keyboardState)
    {
        _player.Update(gameTime);

        // Attack enemy if in front of player
        if (keyboardState.IsKeyDown(Keys.Space) && _player.CanAttack())
        {
            HandlePlayerAttack();
        }

        // Update damage indicators
        UpdateDamageIndicators(gameTime);
    }

    private void HandlePlayerAttack()
    {
        // Get the position in front of the player based on facing direction
        var attackOffset = _controller.FacingDirection switch
        {
            0 => new Point(0, -1), // North
            1 => new Point(-1, 0), // West
            2 => new Point(0, 1),  // South
            3 => new Point(1, 0),  // East
            _ => Point.Zero
        };

        var attackPosition = new Point(
            _player.GridPosition.X + attackOffset.X,
            _player.GridPosition.Y + attackOffset.Y
        );

        // Check if there's an enemy at the attack position
        var enemy = _enemyManager.GetEnemyAtPosition(attackPosition);
        if (enemy != null)
        {
            var damage = _player.CalculateAttackDamage();
            enemy.TakeDamage(damage);
            _player.ResetAttackCooldown();

            // Create damage indicator
            AddDamageIndicator(damage, attackPosition);
        }
    }

    private void AddDamageIndicator(int damage, Point position) => _damageIndicators.Add(new DamageIndicator
    {
        Damage = damage,
        Position = position,
        TimeLeft = 1.0f,
        Offset = Vector2.Zero
    });

    private void UpdateDamageIndicators(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        for (var i = _damageIndicators.Count - 1; i >= 0; i--)
        {
            var indicator = _damageIndicators[i];
            indicator.TimeLeft -= deltaTime;

            // Move indicator upward
            indicator.Offset += new Vector2(0, -60 * deltaTime);

            // Remove expired indicators
            if (indicator.TimeLeft <= 0)
            {
                _damageIndicators.RemoveAt(i);
            }
            else
            {
                _damageIndicators[i] = indicator;
            }
        }
    }

    public void DrawDamageIndicators(Matrix view, Matrix projection)
    {
        foreach (var indicator in _damageIndicators)
        {
            // Project the world position to screen coordinates
            var worldPos = new Vector3(
                indicator.Position.X,
                0.5f, // Slightly above the ground
                indicator.Position.Y
            );

            var screenPos = _spriteBatch.GraphicsDevice.Viewport.Project(
                worldPos,
                projection,
                view,
                Matrix.Identity
            );

            // Only draw if in front of camera
            if (screenPos.Z < 1)
            {
                var damageText = indicator.Damage.ToString();
                var textSize = _font.MeasureString(damageText);

                // Calculate final position with offset
                var finalPos = new Vector2(screenPos.X, screenPos.Y) + indicator.Offset - (textSize / 2);

                // Draw with changing color and fading
                var alpha = Math.Min(1, indicator.TimeLeft * 2);
                var textColor = Color.Lerp(Color.Yellow, Color.Red, 1 - indicator.TimeLeft);
                textColor.A = (byte)(alpha * 255);

                // Draw outlined text
                _spriteBatch.DrawString(_font, damageText, finalPos + new Vector2(1, 1), Color.Black * alpha);
                _spriteBatch.DrawString(_font, damageText, finalPos, textColor);
            }
        }
    }

    private struct DamageIndicator
    {
        public int Damage;
        public Point Position;
        public float TimeLeft;
        public Vector2 Offset;
    }
}
