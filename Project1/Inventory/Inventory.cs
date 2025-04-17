using System;
using System.Collections.Generic;

namespace Project1.Inventory;

public class Inventory(int capacity = 20)
{
    private readonly List<ItemInstance> _items = new(capacity);
    private readonly int _capacity = capacity;

    public IReadOnlyList<ItemInstance> Items => _items;
    public int Count => _items.Count;
    public int Capacity => _capacity;

    public event Action<Item> ItemAdded;
    public event Action<Item> ItemRemoved;
    public event Action InventoryChanged;

    private Weapon _equippedWeapon;

    public bool AddItem(Item item, int count = 1)
    {
        // Try to add to existing stack first
        if (item.IsStackable)
        {
            foreach (var instance in _items)
            {
                if (instance.CanStack(item) && instance.AddToStack(count))
                {
                    ItemAdded?.Invoke(item);
                    InventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // Add new item if we have space
        if (_items.Count < _capacity)
        {
            _items.Add(new ItemInstance(item, count));
            ItemAdded?.Invoke(item);
            InventoryChanged?.Invoke();
            return true;
        }

        return false;
    }

    public bool RemoveItem(string itemId, int count = 1)
    {
        for (var i = _items.Count - 1; i >= 0; i--)
        {
            var instance = _items[i];
            if (instance.Item.Id == itemId)
            {
                if (instance.Count <= count)
                {
                    // Remove the whole stack
                    _items.RemoveAt(i);
                    ItemRemoved?.Invoke(instance.Item);
                    InventoryChanged?.Invoke();
                    return true;
                }
                else
                {
                    // Remove part of the stack
                    instance.RemoveFromStack(count);
                    ItemRemoved?.Invoke(instance.Item);
                    InventoryChanged?.Invoke();
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasItem(string itemId, int requiredCount = 1)
    {
        var totalCount = 0;
        foreach (var instance in _items)
        {
            if (instance.Item.Id == itemId)
            {
                totalCount += instance.Count;
                if (totalCount >= requiredCount)
                    return true;
            }
        }
        return false;
    }

    public void UseItem(int index, Player player)
    {
        if (index < 0 || index >= _items.Count)
            return;

        var instance = _items[index];
        instance.Item.Use(player);

        // Remove consumable items after use
        if (instance.Item.Type is ItemType.Potion or ItemType.Scroll)
        {
            if (instance.RemoveFromStack(1) && instance.Count <= 0)
            {
                _items.RemoveAt(index);
            }
            InventoryChanged?.Invoke();
        }
    }

    public Weapon GetEquippedWeapon() => _equippedWeapon;

    public void EquipWeapon(Weapon weapon)
    {
        if (_equippedWeapon != null)
        {
            // Unequip the current weapon
            _equippedWeapon = null;
        }
        _equippedWeapon = weapon;
        InventoryChanged?.Invoke();
    }
}
