using System.Collections.Generic;

namespace Project1.Inventory;

public class ItemDatabase
{
    private readonly Dictionary<string, Item> _items = [];

    public void RegisterItem(Item item) => _items[item.Id] = item;

    public Item GetItem(string id) => _items.TryGetValue(id, out var item) ? item : null;
}
