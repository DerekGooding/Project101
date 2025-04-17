using Project1.Dungeon;

namespace Project1.Audio;

public class FootstepSystem
{
    private readonly AudioManager _audioManager;
    private readonly Dictionary<int, string> _floorTypeSounds = [];
    private readonly Map _map;
    private Point _lastPosition;
    private int _lastDirection;

    public FootstepSystem(AudioManager audioManager, Map map)
    {
        _audioManager = audioManager;
        _map = map;

        // Map floor types to sound effects
        _floorTypeSounds[0] = "footstep_stone"; // Default floor
        _floorTypeSounds[2] = "footstep_wood";  // Wood floor
        _floorTypeSounds[3] = "footstep_water"; // Water puddles
    }

    public void Update(Point currentPosition, int currentDirection)
    {
        if (_lastPosition != currentPosition)
        {
            // Player has moved, play footstep sound
            var floorType = 0; // Default to stone floor

            // Get floor type at current position (could be extended to check map data)
            // floorType = _map.GetFloorType(currentPosition);

            if (_floorTypeSounds.TryGetValue(floorType, out var soundName))
            {
                _audioManager.PlaySound(soundName);
            }

            _lastPosition = currentPosition;
        }

        _lastDirection = currentDirection;
    }
}