using Project1.Effects;
using Project1.Inventory;

namespace Project1.Dungeon.Hazards;

public class FireJet(Point position, int damage, ParticleSystem fireParticles)
    : Hazard(position, HazardType.Trap, "A jet that periodically shoots flames", true)
{
    private readonly int _damage = damage;
    private bool _isEmitting = false;
    private double _emitTimer = 0;
    private readonly double _emitDuration = 2.0; // Fire stays on for 2 seconds
    private readonly double _cooldownDuration = 3.0; // Then 3 seconds off
    private readonly ParticleSystem _fireParticles = fireParticles;

    public override void Trigger(Player player)
    {
        if (IsActive && _isEmitting)
        {
            player.TakeDamage(_damage);
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (!IsActive) return;

        _emitTimer += gameTime.ElapsedGameTime.TotalSeconds;

        if (_isEmitting && _emitTimer >= _emitDuration)
        {
            _isEmitting = false;
            _emitTimer = 0;
        }
        else if (!_isEmitting && _emitTimer >= _cooldownDuration)
        {
            _isEmitting = true;
            _emitTimer = 0;
            // Add fire particles
            _fireParticles.EmitBurst(
                GetWorldPosition(),
                new Vector3(0, 1, 0),
                30,
                Color.Orange,
                Color.Red);
        }
    }

    private Vector3 GetWorldPosition() => new(Position.X * 2f, 0.5f, Position.Y * 2f);
}
