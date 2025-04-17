using Microsoft.Xna.Framework.Audio;

namespace Project1.Audio;

public class PositionalAudio(AudioManager audioManager)
{
    private readonly AudioManager _audioManager = audioManager;
    private readonly AudioListener _listener = new();
    private readonly Dictionary<Point, List<AudioEmitter>> _emitters = [];

    public void AddSoundEmitter(Point position, string soundId, float minDistance = 1.0f, float maxDistance = 10.0f)
    {
        var emitter = new AudioEmitter
        {
            Position = new Vector3(position.X * 2, 0, position.Y * 2),
            Forward = Vector3.Forward,
            Up = Vector3.Up
        };

        if (!_emitters.ContainsKey(position))
        {
            _emitters[position] = [];
        }

        _emitters[position].Add(emitter);
    }

    public void UpdateListener(Vector3 position, Vector3 forward, Vector3 up)
    {
        _listener.Position = position;
        _listener.Forward = forward;
        _listener.Up = up;
    }

    public void PlaySoundAt(Point position, string soundId)
    {
        if (!_emitters.ContainsKey(position))
            return;

        foreach (var emitter in _emitters[position])
        {
            if (_audioManager.SoundEffects.TryGetValue(soundId, out var sound))
            {
                var instance = sound.CreateInstance();
                instance.Apply3D(_listener, emitter);
                instance.Play();
            }
        }
    }
}