using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Project1;

public class DungeonCrawlerController(Point startPosition, DungeonMap map)
{
    public Point GridPosition { get; private set; } = startPosition;
    public int FacingDirection { get; private set; } = 0; // 0=N,1=E,2=S,3=W

    private KeyboardState _previousState;
    private readonly double _moveCooldown = 0.15;
    private double _moveTimer;
    private readonly DungeonMap _map = map;

    public void Update(GameTime gameTime)
    {
        _moveTimer += gameTime.ElapsedGameTime.TotalSeconds;
        var currentState = Keyboard.GetState();

        if (_moveTimer >= _moveCooldown)
        {
            if (IsKeyPressed(Keys.A, currentState)) // Rotate Left
            {
                FacingDirection = (FacingDirection + 1) % 4;
                _moveTimer = 0;
            }
            else if (IsKeyPressed(Keys.D, currentState)) // Rotate Right
            {
                FacingDirection = (FacingDirection + 3) % 4;
                _moveTimer = 0;
            }
            else if (IsKeyPressed(Keys.W, currentState)) // Move Forward
            {
                AttemptMove(1);
            }
            else if (IsKeyPressed(Keys.S, currentState)) // Move Backward
            {
                AttemptMove(-1);
            }
        }

        _previousState = currentState;
    }

    private void AttemptMove(int direction)
    {
        var offset = FacingOffset();
        Point target = GridPosition + offset.Multiply(direction);


        if (_map.IsWalkable(target))
        {
            GridPosition = target;
            _moveTimer = 0;
        }
    }

    private Point FacingOffset() => FacingDirection switch
    {
        0 => new Point(0, -1), // North
        1 => new Point(1, 0),  // East
        2 => new Point(0, 1),  // South
        3 => new Point(-1, 0), // West
        _ => Point.Zero
    };

    private bool IsKeyPressed(Keys key, KeyboardState currentState) => currentState.IsKeyDown(key) && !_previousState.IsKeyDown(key);

    public Vector3 GetWorldPosition(float tileSize) => new(GridPosition.X * tileSize, 0, GridPosition.Y * tileSize);

    public Quaternion Rotation => Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(FacingDirection * 90));
}
