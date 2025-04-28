using Project1.Inventory;

namespace Project1.Dungeon;

public abstract class Hazard(Point position, HazardType type, string description, bool isVisible = true, bool isActive = true)
{
    public Point Position { get; set; } = position;
    public bool IsVisible { get; protected set; } = isVisible;
    public bool IsActive { get; protected set; } = isActive;
    public HazardType Type { get; } = type;
    public string Description { get; } = description;

    public HazardManager? Parent { get; set; }

    public abstract void Trigger(Player player);
    public virtual void Update(GameTime gameTime) { }
    public virtual void Reset() => IsActive = true;
    public virtual void Disable() => IsActive = false;
}
