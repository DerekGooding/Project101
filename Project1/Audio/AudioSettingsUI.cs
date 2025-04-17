using Microsoft.Xna.Framework.Audio;

namespace Project1.Audio;

public class AudioSettingsUI
{
    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _font;
    private readonly Texture2D _sliderTexture;
    private readonly Texture2D _sliderKnobTexture;
    private readonly AudioManager _audioManager;

    private Rectangle _musicVolumeSlider;
    private Rectangle _sfxVolumeSlider;
    private Rectangle _masterVolumeSlider;
    private bool _isVisible;
    private bool _isDraggingMusic;
    private bool _isDraggingSfx;
    private bool _isDraggingMaster;

    public bool IsVisible => _isVisible;

    public AudioSettingsUI(Game game, SpriteBatch spriteBatch, SpriteFont font, AudioManager audioManager)
    {
        _spriteBatch = spriteBatch;
        _font = font;
        _audioManager = audioManager;

        // Create textures for UI elements
        _sliderTexture = new Texture2D(game.GraphicsDevice, 1, 1);
        _sliderTexture.SetData(new[] { Color.Gray });

        _sliderKnobTexture = new Texture2D(game.GraphicsDevice, 1, 1);
        _sliderKnobTexture.SetData(new[] { Color.White });

        // Create UI rectangles
        var viewport = game.GraphicsDevice.Viewport;
        var sliderWidth = 200;
        var sliderHeight = 10;
        var startY = 200;
        var padding = 40;

        _musicVolumeSlider = new Rectangle(
            (viewport.Width - sliderWidth) / 2,
            startY,
            sliderWidth,
            sliderHeight
        );

        _sfxVolumeSlider = new Rectangle(
            (viewport.Width - sliderWidth) / 2,
            startY + padding,
            sliderWidth,
            sliderHeight
        );

        _masterVolumeSlider = new Rectangle(
            (viewport.Width - sliderWidth) / 2,
            startY + padding * 2,
            sliderWidth,
            sliderHeight
        );
    }

    public void Toggle() => _isVisible = !_isVisible;

    public void Update(GameTime gameTime, MouseState mouseState, MouseState prevMouseState)
    {
        if (!_isVisible)
            return;

        var mousePos = new Point(mouseState.X, mouseState.Y);

        // Check for mouse clicks on sliders
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            if (_musicVolumeSlider.Contains(mousePos))
            {
                _isDraggingMusic = true;
            }
            else if (_sfxVolumeSlider.Contains(mousePos))
            {
                _isDraggingSfx = true;
            }
            else if (_masterVolumeSlider.Contains(mousePos))
            {
                _isDraggingMaster = true;
            }
        }
        else
        {
            _isDraggingMusic = _isDraggingSfx = _isDraggingMaster = false;
        }

        // Update slider values when dragging
        if (_isDraggingMusic)
        {
            var value = MathHelper.Clamp((float)(mousePos.X - _musicVolumeSlider.X) / _musicVolumeSlider.Width, 0, 1);
            _audioManager.SetMusicVolume(value);
        }

        if (_isDraggingSfx)
        {
            var value = MathHelper.Clamp((float)(mousePos.X - _sfxVolumeSlider.X) / _sfxVolumeSlider.Width, 0, 1);
            _audioManager.SoundEffectVolume = value;
        }

        if (_isDraggingMaster)
        {
            var value = MathHelper.Clamp((float)(mousePos.X - _masterVolumeSlider.X) / _masterVolumeSlider.Width, 0, 1);
            _audioManager.SetMasterVolume(value);
        }
    }

    public void Draw()
    {
        if (!_isVisible)
            return;

        // Draw background
        var viewport = _spriteBatch.GraphicsDevice.Viewport;
        _spriteBatch.Draw(_sliderTexture, new Rectangle(0, 0, viewport.Width, viewport.Height), new Color(0, 0, 0, 150));

        // Draw title
        var title = "Audio Settings";
        var titleSize = _font.MeasureString(title);
        _spriteBatch.DrawString(
            _font,
            title,
            new Vector2((viewport.Width - titleSize.X) / 2, 150),
            Color.White
        );

        // Draw music volume slider
        _spriteBatch.Draw(_sliderTexture, _musicVolumeSlider, Color.DarkGray);
        var knobX = _musicVolumeSlider.X + (int)(_musicVolumeSlider.Width * _audioManager.MusicVolume);
        _spriteBatch.Draw(
            _sliderKnobTexture,
            new Rectangle(knobX - 5, _musicVolumeSlider.Y - 5, 10, 20),
            Color.White
        );
        _spriteBatch.DrawString(
            _font,
            "Music Volume",
            new Vector2(_musicVolumeSlider.X, _musicVolumeSlider.Y - 25),
            Color.White
        );

        // Draw SFX volume slider
        _spriteBatch.Draw(_sliderTexture, _sfxVolumeSlider, Color.DarkGray);
        knobX = _sfxVolumeSlider.X + (int)(_sfxVolumeSlider.Width * _audioManager.SoundEffectVolume);
        _spriteBatch.Draw(
            _sliderKnobTexture,
            new Rectangle(knobX - 5, _sfxVolumeSlider.Y - 5, 10, 20),
            Color.White
        );
        _spriteBatch.DrawString(
            _font,
            "Sound Effects Volume",
            new Vector2(_sfxVolumeSlider.X, _sfxVolumeSlider.Y - 25),
            Color.White
        );

        // Draw master volume slider
        _spriteBatch.Draw(_sliderTexture, _masterVolumeSlider, Color.DarkGray);
        knobX = _masterVolumeSlider.X + (int)(_masterVolumeSlider.Width * SoundEffect.MasterVolume);
        _spriteBatch.Draw(
            _sliderKnobTexture,
            new Rectangle(knobX - 5, _masterVolumeSlider.Y - 5, 10, 20),
            Color.White
        );
        _spriteBatch.DrawString(
            _font,
            "Master Volume",
            new Vector2(_masterVolumeSlider.X, _masterVolumeSlider.Y - 25),
            Color.White
        );

        // Draw instructions
        _spriteBatch.DrawString(
            _font,
            "Press ESC to return",
            new Vector2((viewport.Width - _font.MeasureString("Press ESC to return").X) / 2, 350),
            Color.LightGray
        );
    }
}