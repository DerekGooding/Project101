namespace Project1.Inventory;

public class ItemPickup(Point position, Item item, int count = 1)
{
    public Point Position { get; } = position;
    public Item Item { get; } = item;
    public int Count { get; } = count;
    public bool IsCollected { get; private set; }

    public void Collect() => IsCollected = true;
}
