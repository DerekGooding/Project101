using System;
using System.Collections.Generic;

namespace Project1.Audio;

public class AmbientSoundManager
{
    private readonly AudioManager _audioManager;
    private readonly Dictionary<string, string> _areaAmbientGroups = [];
    private readonly Random _random = Random.Shared;
    private string _currentArea = "dungeon";
    private double _timeSinceLastAmbient = 0;
    private double _nextAmbientTime = 0;

    public AmbientSoundManager(AudioManager audioManager)
    {
        _audioManager = audioManager;

        // Map areas to ambient sound groups
        _areaAmbientGroups["dungeon"] = "dungeon";
        _areaAmbientGroups["cave"] = "cave";
        _areaAmbientGroups["castle"] = "castle";

        // Set initial ambient time
        ResetAmbientTimer();
    }

    public void SetArea(string area)
    {
        if (_areaAmbientGroups.ContainsKey(area))
        {
            _currentArea = area;
        }
    }

    public void Update(GameTime gameTime)
    {
        _timeSinceLastAmbient += gameTime.ElapsedGameTime.TotalSeconds;

        if (_timeSinceLastAmbient >= _nextAmbientTime)
        {
            _audioManager.PlayRandomAmbientSound(_areaAmbientGroups[_currentArea]);
            _timeSinceLastAmbient = 0;
            ResetAmbientTimer();
        }
    }

    private void ResetAmbientTimer() => _nextAmbientTime = _random.NextDouble() * 12 + 8;
}
