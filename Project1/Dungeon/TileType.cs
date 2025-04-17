﻿namespace Project1.Dungeon;

public enum TileType
{
    Floor,      // Basic walkable floor
    Wall,       // Basic wall
    Door,       // Door (possibly locked)
    Water,      // Water (might slow down or damage player)
    Lava,       // Lava (damages player)
    Stairs      // Stairs to another level
}
