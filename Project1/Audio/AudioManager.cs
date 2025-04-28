using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Project1.Audio;
public class AudioManager
{
    private readonly Game _game;
    private readonly Dictionary<string, Song> _songs = [];
    private readonly Dictionary<string, List<SoundEffect>> _ambientSounds = [];
    public Dictionary<string, SoundEffect> SoundEffects { get; } = [];

    private Song? _currentSong;
    private readonly List<SoundEffectInstance> _activeSoundInstances = [];
    private readonly Random _random = new();

    // Settings
    public float MusicVolume { get; set; } = 0.5f;
    public float SoundEffectVolume { get; set; } = 0.8f;
    public float AmbientVolume { get; set; } = 0.3f;
    public bool IsMusicEnabled { get; set; } = true;
    public bool IsSoundEnabled { get; set; } = true;

    public AudioManager(Game game)
    {
        _game = game;
        MediaPlayer.IsRepeating = true;
    }

    public void LoadContent()
    {
        // Load sound effects
        SoundEffects["footstep_stone"] = _game.Content.Load<SoundEffect>("Audio/Effects/footstep_stone");
        SoundEffects["door_open"] = _game.Content.Load<SoundEffect>("Audio/Effects/door_open");
        SoundEffects["item_pickup"] = _game.Content.Load<SoundEffect>("Audio/Effects/item_pickup");
        SoundEffects["dialog_next"] = _game.Content.Load<SoundEffect>("Audio/Effects/dialog_next");
        SoundEffects["inventory_open"] = _game.Content.Load<SoundEffect>("Audio/Effects/inventory_open");
        SoundEffects["inventory_close"] = _game.Content.Load<SoundEffect>("Audio/Effects/inventory_close");
        SoundEffects["player_damaged"] = _game.Content.Load<SoundEffect>("Audio/Effects/player_damaged");
        SoundEffects["attack"] = _game.Content.Load<SoundEffect>("Audio/Effects/attack");

        // Load music tracks
        //_songs["dungeon_ambient"] = _game.Content.Load<Song>("Audio/Music/dungeon_ambient");
        _songs["dungeon_battle"] = _game.Content.Load<Song>("Audio/Music/dungeon_battle");
        //_songs["dungeon_puzzle"] = _game.Content.Load<Song>("Audio/Music/dungeon_puzzle");
        _songs["main_menu"] = _game.Content.Load<Song>("Audio/Music/main_menu");

        // Load and group ambient sounds
        _ambientSounds["dungeon"] =
        [
            _game.Content.Load<SoundEffect>("Audio/Ambience/drip_1"),
            _game.Content.Load<SoundEffect>("Audio/Ambience/drip_2"),
            _game.Content.Load<SoundEffect>("Audio/Ambience/creak_1"),
            _game.Content.Load<SoundEffect>("Audio/Ambience/wind_1")
        ];
    }

    public void PlaySound(string soundName)
    {
        if (!IsSoundEnabled || !SoundEffects.TryGetValue(soundName, out var value))
            return;

        var instance = value.CreateInstance();
        instance.Volume = SoundEffectVolume;
        instance.Play();

        // Store long-playing sounds to manage their lifecycle
        if (soundName.StartsWith("amb_"))
        {
            _activeSoundInstances.Add(instance);
        }
    }

    public void PlayMusic(string songName)
    {
        if (!IsMusicEnabled || !_songs.TryGetValue(songName, out var value))
            return;

        _currentSong = value;
        MediaPlayer.Volume = MusicVolume;
        MediaPlayer.Play(_currentSong);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    public void StopMusic() => MediaPlayer.Stop();

    public void SetMusicVolume(float volume)
    {
        MusicVolume = MathHelper.Clamp(volume, 0f, 1f);
        MediaPlayer.Volume = MusicVolume;
    }

    public void PlayRandomAmbientSound(string ambientGroup)
    {
        if (!IsSoundEnabled || !_ambientSounds.TryGetValue(ambientGroup, out var sounds))
            return;
        if (sounds.Count > 0)
        {
            var index = _random.Next(sounds.Count);
            var instance = sounds[index].CreateInstance();
            instance.Volume = AmbientVolume;
            instance.Play();
            _activeSoundInstances.Add(instance);
        }
    }

    public void Update()
    {
        // Remove completed sound instances
        for (var i = _activeSoundInstances.Count - 1; i >= 0; i--)
        {
            if (_activeSoundInstances[i].State == SoundState.Stopped)
            {
                _activeSoundInstances.RemoveAt(i);
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    public void SetMasterVolume(float volume) => SoundEffect.MasterVolume = MathHelper.Clamp(volume, 0f, 1f);

    public void Dispose()
    {
        StopAllSounds();
        StopMusic();

        foreach (var sound in SoundEffects.Values)
        {
            sound.Dispose();
        }
    }

    private void StopAllSounds()
    {
        foreach (var instance in _activeSoundInstances)
        {
            instance.Stop();
            instance.Dispose();
        }
        _activeSoundInstances.Clear();
    }
}
