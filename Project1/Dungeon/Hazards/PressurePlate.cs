using Project1.Inventory;
using System;

namespace Project1.Dungeon.Hazards;

public class PressurePlate(Point position, Action onActivate, Action onDeactivate = null)
    : Hazard(position, HazardType.Puzzle, "A pressure plate on the floor", true)
{
    public Action OnActivate { get; set; } = onActivate;
    public Action OnDeactivate { get; set; } = onDeactivate;

    public override void Trigger(Player player)
    {
        if (IsActive)
        {
            OnActivate?.Invoke();
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            OnDeactivate?.Invoke();
        }
    }
}
