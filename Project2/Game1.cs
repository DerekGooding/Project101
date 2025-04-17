using Project2.Additions;

namespace Project2;
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private SpriteFont _titleFont;
    private SpriteFont _damageFont;
    private Texture2D _pixel;

    // Game state
    private GameState _currentState = GameState.Menu;
    private KeyboardState _prevKeyboardState;

    // Menu variables
    private int _selectedAdditionIndex = 0;
    private List<Addition> _additions = new List<Addition>();

    // Combat variables
    private Addition _currentAddition;
    private int _currentHitIndex = 0;
    private float _hitTimer = 0f;
    private float _hitWindow = 0.2f; // Time window for successful hit (in seconds)
    private bool _hitSuccessful = false;
    private bool _additionComplete = false;
    private int _finalDamage = 0;
    private float _damageDisplayTimer = 0f;
    private float _damageDisplayDuration = 1.5f;
    private Vector2 _damagePosition;
    private bool _displayingDamage = false;
    private readonly Random _random = Random.Shared;
    private int _baseDamage = 100;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Configure window size
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
    }

    protected override void Initialize()
    {
        // Create addition moves
        _additions.Add(new Addition("Double Slash", [1.0f, 1.2f], [0.6f]));
        _additions.Add(new Addition("Volcano", [1.0f, 1.3f, 1.5f], [0.5f, 1.2f]));
        _additions.Add(new Addition("Harding Slash", [1.0f, 1.2f, 1.4f, 1.8f], [0.4f, 0.9f, 1.5f]));
        _additions.Add(new Addition("Moon Strike", [1.0f, 1.1f, 1.3f, 1.5f, 2.0f], [0.3f, 0.7f, 1.1f, 1.6f]));
        _additions.Add(new Addition("Madness Hero", [1.1f, 1.2f, 1.4f, 1.6f, 1.8f, 2.5f], [0.3f, 0.6f, 0.9f, 1.2f, 1.8f]));

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load fonts
        _font = Content.Load<SpriteFont>("Font");
        _titleFont = Content.Load<SpriteFont>("TitleFont");
        _damageFont = Content.Load<SpriteFont>("DamageFont");

        // Create a 1x1 white texture for drawing rectangles
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        switch (_currentState)
        {
            case GameState.Menu:
                UpdateMenu(keyboardState, deltaTime);
                break;

            case GameState.Combat:
                UpdateCombat(keyboardState, deltaTime);
                break;

            case GameState.Result:
                UpdateResult(keyboardState, deltaTime);
                break;
        }

        _prevKeyboardState = keyboardState;
        base.Update(gameTime);
    }

    private void UpdateMenu(KeyboardState keyboardState, float deltaTime)
    {
        // Navigate menu
        if (IsKeyPressed(keyboardState, Keys.Down))
        {
            _selectedAdditionIndex = (_selectedAdditionIndex + 1) % _additions.Count;
        }
        else if (IsKeyPressed(keyboardState, Keys.Up))
        {
            _selectedAdditionIndex = (_selectedAdditionIndex - 1 + _additions.Count) % _additions.Count;
        }

        // Select addition
        if (IsKeyPressed(keyboardState, Keys.Enter) || IsKeyPressed(keyboardState, Keys.Space))
        {
            _currentAddition = _additions[_selectedAdditionIndex];
            _currentHitIndex = 0;
            _hitTimer = 0f;
            _additionComplete = false;
            _currentState = GameState.Combat;
        }
    }

    private void UpdateCombat(KeyboardState keyboardState, float deltaTime)
    {
        // Return to menu if ESC is pressed
        if (IsKeyPressed(keyboardState, Keys.Escape))
        {
            _currentState = GameState.Menu;
            return;
        }

        if (_additionComplete)
        {
            if (IsKeyPressed(keyboardState, Keys.Enter) || IsKeyPressed(keyboardState, Keys.Space))
            {
                _currentState = GameState.Menu;
            }
            return;
        }

        _hitTimer += deltaTime;

        // Check for hit timing
        if (_currentHitIndex < _currentAddition.HitTimings.Length)
        {
            var targetTime = _currentAddition.HitTimings[_currentHitIndex];

            // Auto-fail if we exceed the target time by too much
            if (_hitTimer > targetTime + (_hitWindow * 1.5f))
            {
                // Failed the sequence
                FinishAddition(false);
                return;
            }

            // Check if Space was pressed
            if (IsKeyPressed(keyboardState, Keys.Space))
            {
                if (Math.Abs(_hitTimer - targetTime) < _hitWindow)
                {
                    // Successful hit
                    _hitSuccessful = true;
                    _currentHitIndex++;

                    // Check if addition is complete
                    if (_currentHitIndex >= _currentAddition.HitTimings.Length)
                    {
                        FinishAddition(true);
                    }
                    else
                    {
                        // Reset timer for next hit
                        _hitTimer = 0f;
                    }
                }
                else
                {
                    // Failed hit timing
                    FinishAddition(false);
                }
            }
        }
    }

    private void UpdateResult(KeyboardState keyboardState, float deltaTime)
    {
        // Update damage display timer
        if (_displayingDamage)
        {
            _damageDisplayTimer += deltaTime;

            // Animate damage position (move upward)
            _damagePosition.Y -= 50f * deltaTime;

            // Check if display duration is complete
            if (_damageDisplayTimer >= _damageDisplayDuration)
            {
                _displayingDamage = false;
                _currentState = GameState.Menu;
            }
        }
    }

    private void FinishAddition(bool success)
    {
        _additionComplete = true;

        // Calculate damage
        if (success)
        {
            // Full completion bonus
            _finalDamage = (int)(_baseDamage * _currentAddition.DamageMultipliers[_currentAddition.DamageMultipliers.Length - 1]);
        }
        else if (_currentHitIndex > 0)
        {
            // Partial completion
            _finalDamage = (int)(_baseDamage * _currentAddition.DamageMultipliers[_currentHitIndex - 1]);
        }
        else
        {
            // Failed the first hit
            _finalDamage = _baseDamage;
        }

        // Add some randomness (±10%)
        _finalDamage = (int)(_finalDamage * (0.9f + (_random.Next(20) / 100f)));

        _damagePosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
        _damageDisplayTimer = 0f;
        _displayingDamage = true;
        _currentState = GameState.Result;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        switch (_currentState)
        {
            case GameState.Menu:
                DrawMenu();
                break;

            case GameState.Combat:
                DrawCombat();
                break;

            case GameState.Result:
                DrawResult();
                break;
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawMenu()
    {
        var titlePos = new Vector2(GraphicsDevice.Viewport.Width / 2, 100);
        var titleSize = _titleFont.MeasureString("ADDITION SYSTEM");

        // Draw title
        _spriteBatch.DrawString(_titleFont, "ADDITION SYSTEM",
            new Vector2(titlePos.X - (titleSize.X / 2), titlePos.Y), Color.White);

        // Draw instructions
        _spriteBatch.DrawString(_font, "Select an Addition and press ENTER or SPACE to begin:",
            new Vector2(200, 180), Color.White);

        // Draw additions menu
        for (var i = 0; i < _additions.Count; i++)
        {
            var itemColor = (i == _selectedAdditionIndex) ? Color.Yellow : Color.White;
            var difficultyStars = new string('★', _additions[i].DamageMultipliers.Length);

            _spriteBatch.DrawString(_font, _additions[i].Name + " - Difficulty: " + difficultyStars,
                new Vector2(250, 220 + (i * 40)), itemColor);
        }

        // Draw controls help
        _spriteBatch.DrawString(_font, "Controls: UP/DOWN to select, ENTER/SPACE to confirm, ESC to quit",
            new Vector2(200, GraphicsDevice.Viewport.Height - 100), Color.Gray);
    }

    private void DrawCombat()
    {
        var titlePos = new Vector2(GraphicsDevice.Viewport.Width / 2, 100);
        var titleSize = _titleFont.MeasureString(_currentAddition.Name);

        // Draw addition name
        _spriteBatch.DrawString(_titleFont, _currentAddition.Name,
            new Vector2(titlePos.X - (titleSize.X / 2), titlePos.Y), Color.White);

        // Draw timing bar background
        var barWidth = 600;
        var barHeight = 30;
        var barX = (GraphicsDevice.Viewport.Width - barWidth) / 2;
        var barY = 300;

        _spriteBatch.Draw(_pixel, new Rectangle(barX, barY, barWidth, barHeight), Color.DarkGray);

        // Draw timing markers
        for (var i = 0; i < _currentAddition.HitTimings.Length; i++)
        {
            var markerX = barX + (int)(_currentAddition.HitTimings[i] * barWidth / 2);
            var markerColor = (i < _currentHitIndex) ? Color.Green : Color.Gold;

            _spriteBatch.Draw(_pixel, new Rectangle(markerX - 2, barY - 5, 4, barHeight + 10), markerColor);
        }

        // Draw cursor position based on timer
        if (!_additionComplete)
        {
            var cursorX = barX + (int)(_hitTimer * barWidth / 2);
            cursorX = Math.Min(cursorX, barX + barWidth); // Clamp to bar width

            _spriteBatch.Draw(_pixel, new Rectangle(cursorX - 3, barY - 10, 6, barHeight + 20), Color.Red);
        }

        // Draw instructions
        var instructions = _additionComplete
            ? "Press ENTER or SPACE to return to menu"
            : "Press SPACE when the cursor aligns with the markers!";

        var instructionSize = _font.MeasureString(instructions);
        _spriteBatch.DrawString(_font, instructions,
            new Vector2((GraphicsDevice.Viewport.Width - instructionSize.X) / 2, 400), Color.White);

        // Draw addition status
        if (_additionComplete)
        {
            var resultText = _currentHitIndex == _currentAddition.HitTimings.Length
                ? "PERFECT ADDITION!"
                : "ADDITION FAILED!";

            var resultColor = _currentHitIndex == _currentAddition.HitTimings.Length
                ? Color.Green
                : Color.Red;

            var resultSize = _titleFont.MeasureString(resultText);
            _spriteBatch.DrawString(_titleFont, resultText,
                new Vector2((GraphicsDevice.Viewport.Width - resultSize.X) / 2, 200), resultColor);
        }
    }

    private void DrawResult()
    {
        // Draw damage display
        if (_displayingDamage)
        {
            var alpha = 1.0f - (_damageDisplayTimer / _damageDisplayDuration);
            var damageColor = new Color(1.0f, 0.3f, 0.3f) * alpha;

            var damageText = _finalDamage.ToString();
            var textSize = _damageFont.MeasureString(damageText);

            _spriteBatch.DrawString(_damageFont, damageText,
                new Vector2(_damagePosition.X - (textSize.X / 2), _damagePosition.Y), damageColor);
        }
    }

    private bool IsKeyPressed(KeyboardState currentKeyboardState, Keys key) => currentKeyboardState.IsKeyDown(key) && !_prevKeyboardState.IsKeyDown(key);
}
