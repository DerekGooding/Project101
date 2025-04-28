using Project1.Inventory;

namespace Project1.Dungeon.Hazards;

public class Door(Door3D door3D) : Hazard(new Point((door3D.TileA.X + door3D.TileB.X) / 2,
                         (door3D.TileA.Y + door3D.TileB.Y) / 2), HazardType.Obstacle, "A locked door", true)
{
    private readonly Door3D _door3D = door3D;

    public override void Trigger(Player player)
    {
        if (_door3D.IsLocked && player.Inventory.HasItem(_door3D.KeyId))
        {
            Open();
            player.Inventory.RemoveItem(_door3D.KeyId);
        }
    }

    public void Open()
    {
        _door3D.SetLocked(false);
        IsActive = false; // Door is no longer an obstacle
    }

    public void Close()
    {
        _door3D.SetLocked(true);
        IsActive = true; // Door is an obstacle again
    }

    public override void Reset()
    {
        base.Reset();
        _door3D.SetLocked(true);
    }
}