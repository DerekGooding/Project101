namespace Project1.Dungeon;

public class Tile
{
    public TileType Type { get; set; }
    public bool IsLocked { get; set; }
    public string? KeyId { get; set; }
    public bool IsExplored { get; set; }

    public bool IsWalkable => Type switch
    {
        TileType.Floor => true,
        TileType.Door => !IsLocked,
        TileType.Stairs => true,
        _ => false
    };
}