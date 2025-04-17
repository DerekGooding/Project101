namespace Project1.Inventory;

public class ItemPickupManager(Player player, SpriteBatch spriteBatch, SpriteFont font, Texture2D itemTexture)
{
    private readonly List<ItemPickup> _pickups = [];
    private readonly Player _player = player;
    private readonly SpriteFont _font = font;
    private readonly SpriteBatch _spriteBatch = spriteBatch;
    private readonly Texture2D _itemTexture = itemTexture;

    public void AddPickup(ItemPickup pickup) => _pickups.Add(pickup);

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
