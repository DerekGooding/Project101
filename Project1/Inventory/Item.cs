namespace Project1.Inventory;

public class Item(string id, string name, string description, ItemType type,
            bool isStackable = false, int maxStackSize = 1, Rectangle? textureRegion = null)
{
    public string Id { get; } = id;
    public string Name { get; } = name;
    public string Description { get; } = description;
    public ItemType Type { get; } = type;
    public bool IsStackable { get; } = isStackable;
    public int MaxStackSize { get; } = maxStackSize;
    public Rectangle TextureRegion { get; } = textureRegion ?? new Rectangle(0, 0, 32, 32);

    public virtual void Use(Player player)
    {
        // Base implementation does nothing
    }
}
