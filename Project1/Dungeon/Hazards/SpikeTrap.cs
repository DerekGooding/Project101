using Project1.Inventory;

namespace Project1.Dungeon.Hazards;
public class SpikeTrap(Point position, int damage, bool isHidden = false)
    : Hazard(position, HazardType.Trap, "Sharp spikes shoot from the floor", !isHidden)
{
    private readonly int _damage = damage;
    private bool _isTriggered = false;
    private double _resetTimer = 0;
    private readonly double _resetDelay = 3.0; // Reset after 3 seconds

    public override void Trigger(Player player)
    {
        if (IsActive && !_isTriggered)
        {
            player.TakeDamage(_damage);
            _isTriggered = true;
            _resetTimer = 0;
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (_isTriggered)
        {
            _resetTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_resetTimer >= _resetDelay)
            {
                _isTriggered = false;
                IsVisible = true; // Once triggered, the trap becomes visible
            }
        }
    }

    public override void Reset()
    {
        base.Reset();
        _isTriggered = false;
        _resetTimer = 0;
    }
}
