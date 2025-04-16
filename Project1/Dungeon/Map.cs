namespace Project1.Dungeon;

public class Map
{
    private readonly int[,] _tiles;

    public Map() => _tiles = new int[,]
        {
            { 1, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 1 },
            { 1, 0, 1, 0, 1 },
            { 1, 0, 0, 0, 1 },
            { 1, 0, 1, 1, 1 },
            { 1, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1 },
        };

    public bool IsWalkable(Point pos) => pos.X >= 0 && pos.Y >= 0 && pos.X < _tiles.GetLength(1)
        && pos.Y < _tiles.GetLength(0) && _tiles[pos.Y, pos.X] == 0;

    public int Width => _tiles.GetLength(1);
    public int Height => _tiles.GetLength(0);
}
