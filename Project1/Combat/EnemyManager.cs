using Project1.Dungeon;
using Project1.Inventory;

namespace Project1.Combat;

public class EnemyManager(SpriteBatch spriteBatch, Texture2D enemyTexture, Player player, SpriteFont font, Map map)
{
    private readonly List<Enemy> _enemies = [];
    private readonly SpriteBatch _spriteBatch = spriteBatch;
    private readonly Texture2D _enemyTexture = enemyTexture;
    public Player Player { get; } = player;
    private readonly SpriteFont _font = font;
    private readonly Map _map = map;

    public void AddEnemy(Enemy enemy) => _enemies.Add(enemy);

    public void Update(GameTime gameTime)
    {
        for (var i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];

            if (!enemy.IsAlive)
            {
                _enemies.RemoveAt(i);
                Player.GainExperience(enemy.ExperienceValue);
                continue;
            }

            enemy.Update(gameTime);

            // Basic AI: if adjacent to player, attack if cooldown allows
            if (IsAdjacentToPlayer(enemy) && enemy.CanAttack())
            {
                // Face player before attacking
                enemy.FacingDirection = DirectionToPlayer(enemy);

                // Attack player
                var damage = enemy.CalculateAttackDamage();
                Player.TakeDamage(damage);
                enemy.ResetAttackCooldown();
            }
            else
            {
                // Simple movement AI - move toward player if not too close
                if (gameTime.TotalGameTime.TotalSeconds % 2 < 0.02) // Move every ~2 seconds
                {
                    TryMoveTowardPlayer(enemy);
                }
            }
        }
    }

    public Enemy? GetEnemyAtPosition(Point position) => _enemies.FirstOrDefault(e => e.GridPosition == position);

    public void DrawEnemies3D(Matrix view, Matrix projection, float tileSize, Model enemyModel)
    {
        if(enemyModel == null) return;
        foreach (var enemy in _enemies)
        {
            // Draw enemy 3D model
            var world = Matrix.CreateRotationY(MathHelper.ToRadians(enemy.FacingDirection * 90)) *
                          Matrix.CreateTranslation(enemy.GridPosition.X * tileSize, 0, enemy.GridPosition.Y * tileSize);

            foreach (var mesh in enemyModel.Meshes)
            {
                foreach (var effect in mesh.Effects.OfType<BasicEffect>())
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }
    }

    public void DrawHealthBars()
    {
        foreach (var enemy in _enemies)
        {
            // Project 3D position to screen
            var position = new Vector3(enemy.GridPosition.X, 0.5f, enemy.GridPosition.Y);
            var projected = _spriteBatch.GraphicsDevice.Viewport.Project(
                position,
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 16f / 9f, 0.1f, 100f),
                Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up),
                Matrix.Identity);

            if (projected.Z < 1) // If in front of camera
            {
                // Draw enemy health bar
                var healthPercentage = (float)enemy.Health / enemy.MaxHealth;
                const int barWidth = 50;
                const int barHeight = 5;
                var barX = (int)projected.X - (barWidth / 2);
                var barY = (int)projected.Y - 30;

                // Background
                _spriteBatch.Draw(_enemyTexture, new Rectangle(barX, barY, barWidth, barHeight), Color.DarkRed);
                // Health
                _spriteBatch.Draw(_enemyTexture, new Rectangle(barX, barY, (int)(barWidth * healthPercentage), barHeight), Color.Red);
            }
        }
    }

    private bool IsAdjacentToPlayer(Enemy enemy)
    {
        var dx = Math.Abs(enemy.GridPosition.X - Player.GridPosition.X);
        var dy = Math.Abs(enemy.GridPosition.Y - Player.GridPosition.Y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    private int DirectionToPlayer(Enemy enemy)
    {
        if (enemy.GridPosition.X < Player.GridPosition.X) return 3; // East
        if (enemy.GridPosition.X > Player.GridPosition.X) return 1; // West
        if (enemy.GridPosition.Y < Player.GridPosition.Y) return 2; // South
        return 0; // North
    }

    private void TryMoveTowardPlayer(Enemy enemy)
    {
        // Simple pathfinding toward player
        var dx = Player.GridPosition.X - enemy.GridPosition.X;
        var dy = Player.GridPosition.Y - enemy.GridPosition.Y;

        // Try to move along the axis with greater distance
        if (Math.Abs(dx) > Math.Abs(dy))
        {
            TryMove(enemy, new Point(Math.Sign(dx), 0));
        }
        else
        {
            TryMove(enemy, new Point(0, Math.Sign(dy)));
        }
    }

    private void TryMove(Enemy enemy, Point offset)
    {
        var newPos = new Point(enemy.GridPosition.X + offset.X, enemy.GridPosition.Y + offset.Y);

        // Check if new position is valid
        if (_map.IsWalkable(newPos) && GetEnemyAtPosition(newPos) == null && newPos != Player.GridPosition)
        {
            enemy.GridPosition = newPos;

            // Update facing direction based on movement
            if (offset.X > 0) enemy.FacingDirection = 3; // East
            else if (offset.X < 0) enemy.FacingDirection = 1; // West
            else if (offset.Y > 0) enemy.FacingDirection = 2; // South
            else if (offset.Y < 0) enemy.FacingDirection = 0; // North
        }
    }
}
