namespace Project1.Dungeon;

public class Map
{
    private readonly Tile[,] _tiles;
    private readonly Random _random = Random.Shared;
    public int Width { get; }
    public int Height { get; }
    private readonly List<Room> _rooms = [];
    private readonly List<Door3D> _doors = [];
    public IReadOnlyList<Door3D> Doors => _doors;

    public Map(int width = 50, int height = 50)
    {
        Width = width;
        Height = height;
        _tiles = new Tile[Height, Width];

        // Initialize all tiles as walls
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                _tiles[y, x] = new Tile { Type = TileType.Wall };
            }
        }
    }

    public bool IsWalkable(Point pos) =>
        pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height && _tiles[pos.Y, pos.X].IsWalkable;

    public Tile? GetTile(Point pos) =>
        pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height ? _tiles[pos.Y, pos.X] : null;

    public TileType GetTileType(Point pos) =>
        pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height ? _tiles[pos.Y, pos.X].Type : TileType.Wall;

    public void SetTile(Point pos, TileType type, bool isLocked = false, string? keyId = null)
    {
        if (pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height)
        {
            _tiles[pos.Y, pos.X].Type = type;
            _tiles[pos.Y, pos.X].IsLocked = isLocked;
            _tiles[pos.Y, pos.X].KeyId = keyId;
        }
    }

    public void GenerateDungeon()
    {
        // We'll implement procedural generation here
        CreateRooms();
        CreateCorridors();
        AddDoors();
        AddFeatures();
    }

    private class Room
    {
        public int X, Y, Width, Height;
        public List<Point> Doors = [];
        public string RoomType = "normal"; // normal, treasure, boss, etc.

        public bool Intersects(Room other) => X < other.X + other.Width && X + Width > other.X &&
                   Y < other.Y + other.Height && Y + Height > other.Y;

        public Point Center => new(X + (Width / 2), Y + (Height / 2));
    }

    private void CreateRooms()
    {
        // Attempt to place several random sized rooms
        var maxRooms = 15;
        var minSize = 4;
        var maxSize = 10;

        for (var i = 0; i < 100; i++) // 100 attempts
        {
            var width = _random.Next(minSize, maxSize);
            var height = _random.Next(minSize, maxSize);
            var x = _random.Next(1, Width - width - 1);
            var y = _random.Next(1, Height - height - 1);

            var newRoom = new Room { X = x, Y = y, Width = width, Height = height };

            // Check if it intersects with existing rooms
            var failed = false;
            foreach (var room in _rooms)
            {
                if (newRoom.Intersects(room))
                {
                    failed = true;
                    break;
                }
            }

            if (!failed)
            {
                // Room doesn't intersect, so add it
                CreateRoom(newRoom);
                _rooms.Add(newRoom);

                // Assign room types - mostly normal, but some special rooms
                if (_rooms.Count == 1)
                {
                    newRoom.RoomType = "start";
                }
                else
                {
                    var roomType = _random.Next(0, 10);
                    newRoom.RoomType = roomType switch
                    {
                        8 => "treasure",
                        9 => "boss",
                        _ => "normal"
                    };
                }

                if (_rooms.Count >= maxRooms)
                {
                    break;
                }
            }
        }

        // Ensure we have a stairs room
        if (_rooms.Count > 1)
        {
            _rooms[^1].RoomType = "stairs";
        }
    }

    private void CreateRoom(Room room)
    {
        // Set floor tiles within the room dimensions
        for (var y = room.Y; y < room.Y + room.Height; y++)
        {
            for (var x = room.X; x < room.X + room.Width; x++)
            {
                SetTile(new Point(x, y), TileType.Floor);
            }
        }
    }

    private void CreateCorridors()
    {
        // Connect each room to the next one
        for (var i = 0; i < _rooms.Count - 1; i++)
        {
            var roomA = _rooms[i];
            var roomB = _rooms[i + 1];

            // Connect room centers
            var pointA = roomA.Center;
            var pointB = roomB.Center;

            // Randomly choose horizontal-then-vertical or vertical-then-horizontal
            if (_random.Next(0, 2) == 0)
            {
                CreateHorizontalTunnel(pointA.X, pointB.X, pointA.Y);
                CreateVerticalTunnel(pointA.Y, pointB.Y, pointB.X);
            }
            else
            {
                CreateVerticalTunnel(pointA.Y, pointB.Y, pointA.X);
                CreateHorizontalTunnel(pointA.X, pointB.X, pointB.Y);
            }
        }
    }

    private void CreateHorizontalTunnel(int x1, int x2, int y)
    {
        for (var x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
        {
            SetTile(new Point(x, y), TileType.Floor);
        }
    }

    private void CreateVerticalTunnel(int y1, int y2, int x)
    {
        for (var y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
        {
            SetTile(new Point(x, y), TileType.Floor);
        }
    }

    private void AddDoors()
    {
        // For each room, look at each tile on its perimeter
        foreach (var room in _rooms)
        {
            // Add doors where corridors meet room walls
            for (var x = room.X; x < room.X + room.Width; x++)
            {
                // Check top wall
                CheckForDoor(new Point(x, room.Y - 1), new Point(x, room.Y), room);

                // Check bottom wall
                CheckForDoor(new Point(x, room.Y + room.Height), new Point(x, room.Y + room.Height - 1), room);
            }

            for (var y = room.Y; y < room.Y + room.Height; y++)
            {
                // Check left wall
                CheckForDoor(new Point(room.X - 1, y), new Point(room.X, y), room);

                // Check right wall
                CheckForDoor(new Point(room.X + room.Width, y), new Point(room.X + room.Width - 1, y), room);
            }
        }
    }

    private void CheckForDoor(Point outsidePos, Point insidePos, Room room)
    {
        var outsideTile = GetTile(outsidePos);
        var insideTile = GetTile(insidePos);

        // If there's floor on both sides of a wall, check if it's a hallway
        if (outsideTile != null && insideTile != null &&
            outsideTile.Type == TileType.Floor && insideTile.Type == TileType.Floor)
        {
            // Determine if this is a hallway by checking adjacent tiles
            if (IsHallway(outsidePos, insidePos))
            {
                // Pick a position for the door (either outsidePos or insidePos)
                var doorPos = _random.Next(0, 2) == 0 ? outsidePos : insidePos;

                // Special rooms have locked doors
                var isLocked = room.RoomType is "treasure" or "boss";
                var keyId = isLocked ? $"key_{room.RoomType}_{_rooms.IndexOf(room)}" : null;

                // Determine door orientation (NORTH_SOUTH or EAST_WEST)
                int orientation = DetermineDoorOrientation(doorPos);

                // Add to door collection for rendering
                _doors.Add(new Door3D(doorPos, orientation, isLocked, keyId));

                // Set tile in map grid
                SetTile(doorPos, TileType.Door, isLocked, keyId);
                room.Doors.Add(doorPos);
            }
        }
    }

    private bool IsHallway(Point outsidePos, Point insidePos)
    {
        // Check if this connection is in a hallway (narrow passage)
        // We'll determine this by checking if there are walls on opposite sides

        // Calculate direction vector from outside to inside
        var dx = insidePos.X - outsidePos.X;
        var dy = insidePos.Y - outsidePos.Y;

        // Check perpendicular directions for walls
        if (dx != 0) // Horizontal connection (east-west)
        {
            // Check north and south for walls
            var northOutside = GetTileType(new Point(outsidePos.X, outsidePos.Y - 1));
            var southOutside = GetTileType(new Point(outsidePos.X, outsidePos.Y + 1));
            var northInside = GetTileType(new Point(insidePos.X, insidePos.Y - 1));
            var southInside = GetTileType(new Point(insidePos.X, insidePos.Y + 1));

            // If there's at least one wall to the north and south, it's a hallway
            var hasNorthWall = northOutside == TileType.Wall || northInside == TileType.Wall;
            var hasSouthWall = southOutside == TileType.Wall || southInside == TileType.Wall;

            return hasNorthWall || hasSouthWall;
        }
        else if (dy != 0) // Vertical connection (north-south)
        {
            // Check east and west for walls
            var eastOutside = GetTileType(new Point(outsidePos.X + 1, outsidePos.Y));
            var westOutside = GetTileType(new Point(outsidePos.X - 1, outsidePos.Y));
            var eastInside = GetTileType(new Point(insidePos.X + 1, insidePos.Y));
            var westInside = GetTileType(new Point(insidePos.X - 1, insidePos.Y));

            // If there's at least one wall to the east and west, it's a hallway
            var hasEastWall = eastOutside == TileType.Wall || eastInside == TileType.Wall;
            var hasWestWall = westOutside == TileType.Wall || westInside == TileType.Wall;

            return hasEastWall || hasWestWall;
        }

        return false;
    }

    private int DetermineDoorOrientation(Point pos)
    {
        // Check adjacent tiles to determine orientation
        var northTile = GetTileType(new Point(pos.X, pos.Y - 1));
        var southTile = GetTileType(new Point(pos.X, pos.Y + 1));
        var eastTile = GetTileType(new Point(pos.X + 1, pos.Y));
        var westTile = GetTileType(new Point(pos.X - 1, pos.Y));

        // If north/south are floor, then the door should be oriented east-west
        if ((northTile == TileType.Floor || southTile == TileType.Floor) &&
            (eastTile == TileType.Wall || westTile == TileType.Wall))
        {
            return Door3D.EAST_WEST;
        }

        // Otherwise, the door is oriented north-south
        return Door3D.NORTH_SOUTH;
    }

    private void AddFeatures()
    {
        // Add features based on room type
        foreach (var room in _rooms)
        {
            switch (room.RoomType)
            {
                case "start":
                    // Start room is just empty
                    break;

                case "normal":
                    // Add some random features to normal rooms
                    AddRandomFeatures(room);
                    break;

                case "treasure":
                    // Add treasure chests
                    AddTreasureFeatures(room);
                    break;

                case "boss":
                    // Boss room features
                    AddBossRoomFeatures(room);
                    break;

                case "stairs":
                    // Add stairs to next level
                    AddStairs(room);
                    break;
            }
        }
    }

    private void AddRandomFeatures(Room room)
    {
        // Add some random water or lava pools
        var featureCount = _random.Next(0, 3);

        for (var i = 0; i < featureCount; i++)
        {
            var x = _random.Next(room.X + 1, room.X + room.Width - 1);
            var y = _random.Next(room.Y + 1, room.Y + room.Height - 1);
            var size = _random.Next(1, 3);
            var type = _random.Next(0, 2) == 0 ? TileType.Water : TileType.Lava;

            // Create a small pool
            for (var dy = -size; dy <= size; dy++)
            {
                for (var dx = -size; dx <= size; dx++)
                {
                    if ((dx * dx) + (dy * dy) <= size * size)
                    {
                        var pos = new Point(x + dx, y + dy);
                        if (pos.X > room.X && pos.X < room.X + room.Width - 1 &&
                            pos.Y > room.Y && pos.Y < room.Y + room.Height - 1)
                        {
                            SetTile(pos, type);
                        }
                    }
                }
            }
        }
    }

    private void AddTreasureFeatures(Room room)
    {
        // Mark center area for treasure placement
        var center = room.Center;

        // This would be where we'd place treasure items
        // The actual item placement would be done elsewhere
    }

    private void AddBossRoomFeatures(Room room)
    {
        // Add dramatic features to the boss room
        // Maybe lava moat around a central platform
        var center = room.Center;
        var moatRadius = Math.Min(room.Width, room.Height) / 3;
        var platformRadius = moatRadius - 1;

        for (var y = room.Y; y < room.Y + room.Height; y++)
        {
            for (var x = room.X; x < room.X + room.Width; x++)
            {
                var dx = x - center.X;
                var dy = y - center.Y;
                var distSq = (dx * dx) + (dy * dy);

                if (distSq <= moatRadius * moatRadius && distSq > platformRadius * platformRadius)
                {
                    SetTile(new Point(x, y), TileType.Lava);
                }
            }
        }
    }

    private void AddStairs(Room room) =>
        // Place stairs in the center of the room
        SetTile(room.Center, TileType.Stairs);

    public Point GetStartPosition() =>
        // Return the center of the first room as the player start position
        _rooms.Count > 0 ? _rooms[0].Center : new Point(1, 1);
}
