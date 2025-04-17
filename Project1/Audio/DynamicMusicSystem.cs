using System;
using System.Collections.Generic;

namespace Project1.Audio;

public class DynamicMusicSystem
{
    private readonly AudioManager _audioManager;
    private GameState _currentState = GameState.Exploration;
    private readonly Dictionary<GameState, string> _musicTracks = [];
    private float _tensionLevel = 0f;
    private readonly float _tensionThreshold = 0.7f;
    private readonly float _tensionDecayRate = 0.05f;

    public DynamicMusicSystem(AudioManager audioManager)
    {
        _audioManager = audioManager;

        // Map game states to music tracks
        _musicTracks[GameState.Exploration] = "dungeon_ambient";
        _musicTracks[GameState.Combat] = "dungeon_battle";
        _musicTracks[GameState.Puzzle] = "dungeon_puzzle";
        _musicTracks[GameState.Tension] = "dungeon_tension";
        _musicTracks[GameState.Victory] = "victory";
    }

    public void SetGameState(GameState newState)
    {
        if (newState == _currentState)
            return;

        _currentState = newState;
        _audioManager.PlayMusic(_musicTracks[newState]);
    }

    public void IncreaseTension(float amount)
    {
        _tensionLevel = MathHelper.Clamp(_tensionLevel + amount, 0f, 1f);

        // Switch to tension music if threshold is crossed
        if (_tensionLevel >= _tensionThreshold && _currentState == GameState.Exploration)
        {
            SetGameState(GameState.Tension);
        }
    }

    public void Update(GameTime gameTime)
    {
        // Gradually decrease tension level if not in combat
        if (_currentState != GameState.Combat && _tensionLevel > 0)
        {
            _tensionLevel = Math.Max(0, _tensionLevel - (_tensionDecayRate * (float)gameTime.ElapsedGameTime.TotalSeconds));

            // Switch back to exploration music when tension drops
            if (_tensionLevel < _tensionThreshold && _currentState == GameState.Tension)
            {
                SetGameState(GameState.Exploration);
            }
        }
    }
}