using System;

namespace Project1.Inventory;

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
        _backgroundTexture.SetData([new Color(0, 0, 0, 200)]);

        _slotTexture = new Texture2D(game.GraphicsDevice, 1, 1);
        _slotTexture.SetData([Color.Gray]);

        // Create a default items texture if none provided
        if (_itemsTexture == null)
        {
            _itemsTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            _itemsTexture.SetData([Color.White]);
        }

        // Create inventory rectangle based on screen size
        var viewport = game.GraphicsDevice.Viewport;
        var inventoryWidth = (_slotsPerRow * (_slotSize + _padding)) + _padding;
        var inventoryHeight = (((inventory.Capacity + _slotsPerRow - 1) / _slotsPerRow) * (_slotSize + _padding)) + _padding;

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

    public void Update(GameTime gameTime, KeyboardState keyState)
    {
        if (!_isVisible)
            return;

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
        for (var i = 0; i < _inventory.Capacity; i++)
        {
            var row = i / _slotsPerRow;
            var col = i % _slotsPerRow;

            var slotRect = new Rectangle(
                _inventoryRect.X + _padding + (col * (_slotSize + _padding)),
                _inventoryRect.Y + _padding + (row * (_slotSize + _padding)),
                _slotSize,
                _slotSize
            );

            // Draw slot background
            var slotColor = (i == _selectedIndex && _inventory.Count > 0) ? Color.Yellow : Color.Gray;
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
                    var countText = item.Count.ToString();
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
            var details = $"{selected.Item.Name}\n{selected.Item.Description}";

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

    private bool IsNewKeyPress(Keys key, KeyboardState currentState) => currentState.IsKeyDown(key) && !_previousKeyState.IsKeyDown(key);
}
