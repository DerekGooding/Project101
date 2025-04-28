namespace Project1.Dungeon;

public class Minimap
{
    private readonly bool[,] _explored;
    private readonly Texture2D _minimapTexture;
    private readonly Texture2D _playerMarker;
    private readonly Map _map;
    private readonly SpriteBatch _spriteBatch;
    private readonly int _tileSize = 8;
    private readonly Color _wallColor = new(64, 64, 64);
    private readonly Color _floorColor = new(192, 192, 192);
    private readonly Color _waterColor = new(0, 0, 255, 128);
    private readonly Color _lavaColor = new(255, 0, 0, 128);
    private readonly Color _playerColor = Color.Red;
    private readonly Color _unexploredColor = Color.Black;
    private readonly Rectangle _minimapRect;

    public Minimap(Game game, Map map, SpriteBatch spriteBatch)
    {
        _map = map;
        _spriteBatch = spriteBatch;
        _explored = new bool[map.Height, map.Width];

        // Create a white pixel texture for drawing
        _minimapTexture = new Texture2D(game.GraphicsDevice, 1, 1);
        _minimapTexture.SetData([Color.White]);

        // Create player marker
        _playerMarker = new Texture2D(game.GraphicsDevice, 1, 1);
        _playerMarker.SetData([Color.Red]);

        // Position the minimap in the top-right corner
        const int padding = 10;
        var mapWidth = map.Width * _tileSize;
        var mapHeight = map.Height * _tileSize;
        _minimapRect = new Rectangle(
            game.GraphicsDevice.Viewport.Width - mapWidth - padding,
            padding,
            mapWidth,
            mapHeight
        );
    }

    public void UpdateExplored(Point playerPosition, int viewDistance = 2)
    {
        // Mark tiles around the player as explored
        for (var y = -viewDistance; y <= viewDistance; y++)
        {
            for (var x = -viewDistance; x <= viewDistance; x++)
            {
                var checkPos = new Point(playerPosition.X + x, playerPosition.Y + y);
                if (checkPos.X >= 0 && checkPos.Y >= 0 &&
                    checkPos.X < _map.Width && checkPos.Y < _map.Height)
                {
                    _explored[checkPos.Y, checkPos.X] = true;
                }
            }
        }
    }

    public void Draw(Point playerPosition)
    {
        // Draw background
        _spriteBatch.Draw(_minimapTexture, _minimapRect, new Color(0, 0, 0, 150));

        // Draw map tiles
        for (var y = 0; y < _map.Height; y++)
        {
            for (var x = 0; x < _map.Width; x++)
            {
                if (_explored[y, x])
                {
                    var tileRect = new Rectangle(
                        _minimapRect.X + (x * _tileSize),
                        _minimapRect.Y + (y * _tileSize),
                        _tileSize,
                        _tileSize
                    );

                    var color = _map.IsWalkable(new Point(x, y)) ? _floorColor : _wallColor;
                    color = _map.GetTileType(new Point(x, y)) switch
                    {
                        TileType.Water => _waterColor,
                        TileType.Lava => _lavaColor,
                        _ => color
                    };
                    _spriteBatch.Draw(_minimapTexture, tileRect, color);
                }
            }
        }

        // Draw player marker
        var playerRect = new Rectangle(
            _minimapRect.X + (playerPosition.X * _tileSize),
            _minimapRect.Y + (playerPosition.Y * _tileSize),
            _tileSize,
            _tileSize
        );
        _spriteBatch.Draw(_playerMarker, playerRect, _playerColor);
    }
}
