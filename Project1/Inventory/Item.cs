using System;
using System.Collections.Generic;

namespace Project1.Inventory;

#region Item System

public enum ItemType
{
    Weapon,
    Armor,
    Potion,
    Scroll,
    Key,
    Treasure
}

public class Item
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public ItemType Type { get; }
    public bool IsStackable { get; }
    public int MaxStackSize { get; }
    public Rectangle TextureRegion { get; } // For sprite sheet based items

    public Item(string id, string name, string description, ItemType type,
                bool isStackable = false, int maxStackSize = 1, Rectangle? textureRegion = null)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        IsStackable = isStackable;
        MaxStackSize = maxStackSize;
        TextureRegion = textureRegion ?? new Rectangle(0, 0, 32, 32);
    }

    public virtual void Use(Player player)
    {
        // Base implementation does nothing
    }
}

public class ItemInstance
{
    public Item Item { get; }
    public int Count { get; private set; }

    public ItemInstance(Item item, int count = 1)
    {
        Item = item;
        Count = Math.Min(count, item.MaxStackSize);
    }

    public bool CanStack(Item item) => Item.Id == item.Id && Item.IsStackable && Count < Item.MaxStackSize;

    public bool AddToStack(int amount)
    {
        if (!Item.IsStackable || Count >= Item.MaxStackSize)
            return false;

        int newCount = Math.Min(Count + amount, Item.MaxStackSize);
        int actualAdded = newCount - Count;
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

public class ItemDatabase
{
    private readonly Dictionary<string, Item> _items = new Dictionary<string, Item>();

    public void RegisterItem(Item item)
    {
        _items[item.Id] = item;
    }

    public Item GetItem(string id)
    {
        return _items.TryGetValue(id, out var item) ? item : null;
    }
}

#endregion

#region Inventory System

public class Inventory
{
    private readonly List<ItemInstance> _items;
    private readonly int _capacity;

    public IReadOnlyList<ItemInstance> Items => _items;
    public int Count => _items.Count;
    public int Capacity => _capacity;

    public event Action<Item> ItemAdded;
    public event Action<Item> ItemRemoved;
    public event Action InventoryChanged;

    public Inventory(int capacity = 20)
    {
        _capacity = capacity;
        _items = new List<ItemInstance>(capacity);
    }

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
        for (int i = _items.Count - 1; i >= 0; i--)
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
        int totalCount = 0;
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
        if (instance.Item.Type == ItemType.Potion || instance.Item.Type == ItemType.Scroll)
        {
            if (instance.RemoveFromStack(1) && instance.Count <= 0)
            {
                _items.RemoveAt(index);
            }
            InventoryChanged?.Invoke();
        }
    }
}

#endregion

#region Player

public class Player
{
    public Inventory Inventory { get; } = new Inventory(20);
    public int Health { get; private set; } = 100;
    public int MaxHealth { get; private set; } = 100;

    public void Heal(int amount)
    {
        Health = Math.Min(Health + amount, MaxHealth);
    }

    public void TakeDamage(int amount)
    {
        Health = Math.Max(Health - amount, 0);
    }
}

#endregion

#region Item Implementations

public class HealthPotion : Item
{
    private readonly int _healAmount;

    public HealthPotion(string id, string name, int healAmount)
        : base(id, name, $"Restores {healAmount} health", ItemType.Potion, true, 5)
    {
        _healAmount = healAmount;
    }

    public override void Use(Player player)
    {
        player.Heal(_healAmount);
    }
}

public class Key : Item
{
    public string DoorId { get; }

    public Key(string id, string name, string doorId)
        : base(id, name, "Opens a locked door", ItemType.Key)
    {
        DoorId = doorId;
    }
}

#endregion

#region Inventory UI

public class InventoryUI
{
    private readonly SpriteBatch _spriteBatch;
    private readonly Texture2D _backgroundTexture;
    private readonly Texture2D _slotTexture;
    private readonly Texture2D _itemsTexture;
    private readonly SpriteFont _font;
    private readonly Inventory _inventory;

    private bool _isVisible = false;
    private int _selectedIndex = 0;
    private KeyboardState _previousKeyState;
    private Rectangle _inventoryRect;
    private readonly int _slotSize = 40;
    private readonly int _padding = 10;
    private readonly int _slotsPerRow = 5;

    public bool IsVisible => _isVisible;

    public InventoryUI(Game game, SpriteBatch spriteBatch, Inventory inventory, SpriteFont font, Texture2D itemsTexture = null)
    {
        _spriteBatch = spriteBatch;
        _inventory = inventory;
        _font = font;
        _itemsTexture = itemsTexture;

        // Create placeholder textures if needed
        _backgroundTexture = new Texture2D(game.GraphicsDevice, 1, 1);
        _backgroundTexture.SetData(new[] { new Color(0, 0, 0, 200) });

        _slotTexture = new Texture2D(game.GraphicsDevice, 1, 1);
        _slotTexture.SetData(new[] { Color.Gray });

        // Create a default items texture if none provided
        if (_itemsTexture == null)
        {
            _itemsTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            _itemsTexture.SetData(new[] { Color.White });
        }

        // Create inventory rectangle based on screen size
        var viewport = game.GraphicsDevice.Viewport;
        int inventoryWidth = _slotsPerRow * (_slotSize + _padding) + _padding;
        int inventoryHeight = ((inventory.Capacity + _slotsPerRow - 1) / _slotsPerRow) * (_slotSize + _padding) + _padding;

        _inventoryRect = new Rectangle(
            (viewport.Width - inventoryWidth) / 2,
            (viewport.Height - inventoryHeight) / 2,
            inventoryWidth,
            inventoryHeight
        );
    }

    public void Toggle()
    {
        _isVisible = !_isVisible;
        if (_isVisible)
        {
            _selectedIndex = 0;
        }
    }

    public void Update(GameTime gameTime)
    {
        if (!_isVisible)
            return;

        var keyState = Keyboard.GetState();

        // Navigation
        if (IsNewKeyPress(Keys.Right, keyState))
        {
            _selectedIndex = Math.Min(_selectedIndex + 1, _inventory.Count - 1);
        }
        else if (IsNewKeyPress(Keys.Left, keyState))
        {
            _selectedIndex = Math.Max(_selectedIndex - 1, 0);
        }
        else if (IsNewKeyPress(Keys.Down, keyState))
        {
            _selectedIndex = Math.Min(_selectedIndex + _slotsPerRow, _inventory.Count - 1);
        }
        else if (IsNewKeyPress(Keys.Up, keyState))
        {
            _selectedIndex = Math.Max(_selectedIndex - _slotsPerRow, 0);
        }
        else if (IsNewKeyPress(Keys.Enter, keyState) || IsNewKeyPress(Keys.Space, keyState))
        {
            UseSelectedItem();
        }
        else if (IsNewKeyPress(Keys.I, keyState) || IsNewKeyPress(Keys.Tab, keyState) || IsNewKeyPress(Keys.Escape, keyState))
        {
            Toggle();
        }

        _previousKeyState = keyState;
    }

    public void Draw()
    {
        if (!_isVisible)
            return;

        // Draw inventory background
        _spriteBatch.Draw(_backgroundTexture, _inventoryRect, Color.White);

        // Draw slots and items
        for (int i = 0; i < _inventory.Capacity; i++)
        {
            int row = i / _slotsPerRow;
            int col = i % _slotsPerRow;

            var slotRect = new Rectangle(
                _inventoryRect.X + _padding + col * (_slotSize + _padding),
                _inventoryRect.Y + _padding + row * (_slotSize + _padding),
                _slotSize,
                _slotSize
            );

            // Draw slot background
            Color slotColor = (i == _selectedIndex && _inventory.Count > 0) ? Color.Yellow : Color.Gray;
            _spriteBatch.Draw(_slotTexture, slotRect, slotColor);

            // Draw item if slot is filled
            if (i < _inventory.Count)
            {
                var item = _inventory.Items[i];

                // Draw item icon
                _spriteBatch.Draw(_itemsTexture, slotRect, item.Item.TextureRegion, Color.White);

                // Draw stack count if stackable
                if (item.Item.IsStackable && item.Count > 1)
                {
                    string countText = item.Count.ToString();
                    var textSize = _font.MeasureString(countText);
                    var countPosition = new Vector2(
                        slotRect.Right - textSize.X - 2,
                        slotRect.Bottom - textSize.Y - 2
                    );

                    // Draw text with outline for better visibility
                    _spriteBatch.DrawString(_font, countText, countPosition + new Vector2(1, 1), Color.Black);
                    _spriteBatch.DrawString(_font, countText, countPosition, Color.White);
                }
            }
        }

        // Draw selected item details
        if (_inventory.Count > 0 && _selectedIndex >= 0 && _selectedIndex < _inventory.Count)
        {
            var selected = _inventory.Items[_selectedIndex];
            string details = $"{selected.Item.Name}\n{selected.Item.Description}";

            var detailsPosition = new Vector2(
                _inventoryRect.X + _padding,
                _inventoryRect.Bottom + 10
            );

            // Draw text with black outline for better visibility
            _spriteBatch.DrawString(_font, details, detailsPosition + new Vector2(1, 1), Color.Black);
            _spriteBatch.DrawString(_font, details, detailsPosition, Color.White);
        }
    }

    private void UseSelectedItem()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _inventory.Count)
        {
            // Use the item on the player (would need to pass player reference)
            // This is just a placeholder for the implementation
        }
    }

    private bool IsNewKeyPress(Keys key, KeyboardState currentState)
    {
        return currentState.IsKeyDown(key) && !_previousKeyState.IsKeyDown(key);
    }
}

#endregion

#region Item Pickup System

public class ItemPickup
{
    public Point Position { get; }
    public Item Item { get; }
    public int Count { get; }
    public bool IsCollected { get; private set; }

    public ItemPickup(Point position, Item item, int count = 1)
    {
        Position = position;
        Item = item;
        Count = count;
    }

    public void Collect()
    {
        IsCollected = true;
    }
}

public class ItemPickupManager
{
    private readonly List<ItemPickup> _pickups = new List<ItemPickup>();
    private readonly Player _player;
    private readonly SpriteFont _font;
    private readonly SpriteBatch _spriteBatch;
    private readonly Texture2D _itemTexture;

    public ItemPickupManager(Player player, SpriteBatch spriteBatch, SpriteFont font, Texture2D itemTexture)
    {
        _player = player;
        _spriteBatch = spriteBatch;
        _font = font;
        _itemTexture = itemTexture;
    }

    public void AddPickup(ItemPickup pickup)
    {
        _pickups.Add(pickup);
    }

    public void CheckPickups(Point playerPosition)
    {
        foreach (var pickup in _pickups)
        {
            if (!pickup.IsCollected && pickup.Position == playerPosition)
            {
                if (_player.Inventory.AddItem(pickup.Item, pickup.Count))
                {
                    pickup.Collect();
                }
            }
        }
    }

    public void DrawPickupIndicators(Point playerPosition)
    {
        foreach (var pickup in _pickups)
        {
            if (!pickup.IsCollected && pickup.Position == playerPosition)
            {
                var screenPos = new Vector2(
                    _spriteBatch.GraphicsDevice.Viewport.Width / 2,
                    _spriteBatch.GraphicsDevice.Viewport.Height - 50);

                _spriteBatch.DrawString(_font, $"Press F to pick up {pickup.Item.Name}", screenPos, Color.Gold);
                break;
            }
        }
    }
}

#endregion
