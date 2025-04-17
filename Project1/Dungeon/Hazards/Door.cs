using Project1.Inventory;

namespace Project1.Dungeon.Hazards;

public class Door(Point position) : Hazard(position, HazardType.Obstacle, "A locked door", true)
{
    private bool _isOpen = false;

    public override void Trigger(Player player)
    {
        // Check if player has the right key
        if (!_isOpen && player.Inventory.HasItem("rusty_key"))
        {
            Open();
            player.Inventory.RemoveItem("rusty_key");
        }
    }

    public void Open()
    {
        _isOpen = true;
        IsActive = false; // Door is no longer an obstacle
    }

    public void Close()
    {
        _isOpen = false;
        IsActive = true; // Door is an obstacle again
    }

    public override void Reset()
    {
        base.Reset();
        _isOpen = false;
    }
}