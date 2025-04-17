using Project1.Inventory;
using System;
using System.Collections.Generic;

namespace Project1.Dungeon;

public class HazardManager
{
    private readonly List<Hazard> _hazards = [];
    private readonly SpriteBatch _spriteBatch;
    private readonly Texture2D _iconTexture;
    private readonly SpriteFont _font;
    private readonly Player _player;

    public HazardManager(Game game, SpriteBatch spriteBatch, SpriteFont font, Player player)
    {
        _spriteBatch = spriteBatch;
        _font = font;
        _player = player;

        // Create a simple icon texture for hazards
        _iconTexture = new Texture2D(game.GraphicsDevice, 1, 1);
        _iconTexture.SetData([Color.White]);
    }

    public void AddHazard(Hazard hazard)
    {
        hazard.Parent = this;
        _hazards.Add(hazard);
    }

    public void Update(GameTime gameTime, Point playerPosition)
    {
        foreach (var hazard in _hazards)
        {
            hazard.Update(gameTime);

            if (hazard.Position == playerPosition && hazard.IsActive)
            {
                hazard.Trigger(_player);
            }
        }
    }

    public void DrawHazardIndicators(Point playerPosition)
    {
        foreach (var hazard in _hazards)
        {
            if (hazard.Position == playerPosition && hazard.IsVisible &&
                (hazard.Type == HazardType.Puzzle || hazard.Type == HazardType.Obstacle))
            {
                var screenPos = new Vector2(
                    _spriteBatch.GraphicsDevice.Viewport.Width / 2,
                    _spriteBatch.GraphicsDevice.Viewport.Height - 80);

                _spriteBatch.DrawString(_font, $"Press E to interact", screenPos, Color.Yellow);
                break;
            }
        }
    }

    public Hazard GetHazardAtPosition(Point position) =>
        _hazards.FirstOrDefault(h => h.Position == position && h.IsVisible);

    public void ResetAllHazards()
    {
        foreach (var hazard in _hazards)
        {
            hazard.Reset();
        }
    }

    public void DrawHazards(Camera camera, float tileSize)
    {
        // For 3D visualization of hazards in the dungeon
        // This would integrate with your rendering pipeline
    }

    internal object GetBlockAtPosition(Point position) => throw new NotImplementedException();
}