using System;

namespace Project1.Inventory;

public class ItemInstance(Item item, int count = 1)
{
    public Item Item { get; } = item;
    public int Count { get; private set; } = Math.Min(count, item.MaxStackSize);

    public bool CanStack(Item item) => Item.Id == item.Id && Item.IsStackable && Count < Item.MaxStackSize;

    public bool AddToStack(int amount)
    {
        if (!Item.IsStackable || Count >= Item.MaxStackSize)
            return false;

        var newCount = Math.Min(Count + amount, Item.MaxStackSize);
        var actualAdded = newCount - Count;
        Count = newCount;
        return actualAdded > 0;
    }

    public bool RemoveFromStack(int amount)
    {
        if (amount > Count)
            return false;

        Count -= amount;
        return true;
    }
}
