namespace Project1.Dungeon;

public class Tile
{
    public TileType Type { get; set; }
    public bool IsExplored { get; set; }

    public bool IsWalkable => Type switch
    {
        TileType.Floor => true,
        TileType.Stairs => true,
        _ => false
    };
}