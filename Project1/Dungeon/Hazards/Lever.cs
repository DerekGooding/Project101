using Project1.Inventory;

namespace Project1.Dungeon.Hazards;

public class Lever(Point position, Action onActivate, Action onDeactivate)
    : Hazard(position, HazardType.Puzzle, "A wall lever that can be pulled", true)
{
    private readonly Action _onActivate = onActivate;
    private readonly Action _onDeactivate = onDeactivate;
    private bool _isOn = false;

    public override void Trigger(Player player)
    {
        if (IsActive)
        {
            _isOn = !_isOn;

            if (_isOn)
            {
                _onActivate?.Invoke();
            }
            else
            {
                _onDeactivate?.Invoke();
            }
        }
    }
}
