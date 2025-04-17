using Project1.Inventory;
using System;

namespace Project1.Dungeon.Puzzles;
public class MovableBlock(Map map, Point position) : Hazard(position, HazardType.Obstacle, "A heavy stone block that can be pushed", true)
{
    private Point _originalPosition = position;
    private bool _canBeMoved = true;
    private readonly Map _map = map;

    public override void Trigger(Player player)
    {
        if (!_canBeMoved || !IsActive) return;

        // Determine push direction (same as player's facing direction)
        var facingOffset = player.FacingOffset;
        var targetPosition = Position + facingOffset;

        // Check if the block can be moved to the target position
        if (CanMoveTo(targetPosition))
        {
            // Update position
            var oldPosition = Position;
            Position = targetPosition;

            // Notify puzzle that block has moved
            BlockMoved?.Invoke(this, oldPosition, Position);
        }
    }

    private bool CanMoveTo(Point position) => _map.IsWalkable(position) && !HasAnotherBlock(position);

    private bool HasAnotherBlock(Point position) => Parent.GetBlockAtPosition(position) != null;

    public override void Reset()
    {
        base.Reset();
        Position = _originalPosition;
    }

    public void LockInPlace() => _canBeMoved = false;

    // Event for puzzle logic
    public event Action<MovableBlock, Point, Point> BlockMoved;
}
