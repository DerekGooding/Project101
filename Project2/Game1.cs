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
    private Texture2D _squareTexture;

    // Game state
    private GameState _currentState = GameState.Menu;
    private KeyboardState _prevKeyboardState;

    // Menu variables
    private int _selectedAdditionIndex = 0;
    private List<Addition> _additions = new();

    // Combat variables
    private Addition _currentAddition;
    private int _currentHitIndex = 0;
    private float _hitTimer = 0f;
    private float _hitWindow = 0.15f; // Time window for successful hit (in seconds)
    private bool _hitSuccessful = false;
    private bool _additionComplete = false;
    private int _finalDamage = 0;
    private float _damageDisplayTimer = 0f;
    private float _damageDisplayDuration = 1.5f;
    private Vector2 _damagePosition;
    private bool _displayingDamage = false;
    private Random _random = Random.Shared;
    private int _baseDamage = 100;

    // Timing square variables
    private float _rotationAngle = 0f;
    private float _rotationSpeed = 2f; // Rotation speed in radians per second
    private float _squareSize = 200f; // Size of the static square
    private float _movingSquareSize = 500f; // Initial size of the moving square
    private float _shrinkRate = 400f; // How fast the square shrinks (pixels per second)
    private Color _staticSquareColor = new(50, 50, 200, 150);
    private Color _movingSquareColor = new(200, 50, 50, 150);
    private Color _hitZoneColor = new(50, 200, 50, 100);

    // Feedback message
    private string _feedbackMessage = "";
    private float _feedbackTimer = 0f;
    private float _feedbackDuration = 1.0f;
    private Color _feedbackColor = Color.Red;

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

        // Create a square texture
        _squareTexture = new Texture2D(GraphicsDevice, 1, 1);
        _squareTexture.SetData([Color.White]);
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

        if (_feedbackTimer > 0)
        {
            _feedbackTimer -= deltaTime;
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
            _movingSquareSize = 500f; // Reset size for the first hit
            _rotationAngle = 0f;
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

        // Update rotation
        _rotationAngle += _rotationSpeed * deltaTime;

        // Shrink the moving square
        _movingSquareSize -= _shrinkRate * deltaTime;

        // Update hit timer
        _hitTimer += deltaTime;

        // Check for hit timing
        if (_currentHitIndex < _currentAddition.HitTimings.Length)
        {
            var targetTime = _currentAddition.HitTimings[_currentHitIndex];

            // Auto-fail if the square gets too small
            if (_movingSquareSize < _squareSize * 0.5f)
            {
                // Show "Too Late" message
                ShowFeedbackMessage("TOO LATE", Color.Red);

                // Failed the sequence
                FinishAddition(false);
                return;
            }

            // Check if Space was pressed
            if (IsKeyPressed(keyboardState, Keys.Space))
            {
                var sizeRatio = _movingSquareSize / _squareSize;

                // Hit is successful if the moving square is close to the static square size
                if (sizeRatio is >= 0.9f and <= 1.1f)
                {
                    // Successful hit
                    _hitSuccessful = true;
                    ShowFeedbackMessage("GOOD", Color.Green);
                    _currentHitIndex++;

                    // Check if addition is complete
                    if (_currentHitIndex >= _currentAddition.HitTimings.Length)
                    {
                        FinishAddition(true);
                    }
                    else
                    {
                        // Reset for next hit, start at a larger size for more challenging hits
                        _movingSquareSize = 500f + (_currentHitIndex * 50f);
                        _rotationAngle = 0f;
                        _hitTimer = 0f;

                        // Increase rotation speed for harder hits
                        _rotationSpeed = 2.0f + (_currentHitIndex * 0.4f);
                    }
                }
                else
                {
                    if (sizeRatio > 1.1f)
                    {
                        ShowFeedbackMessage("TOO SOON", Color.Red);
                    }
                    else
                    {
                        ShowFeedbackMessage("TOO LATE", Color.Red);
                    }
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

    private void ShowFeedbackMessage(string message, Color color)
    {
        _feedbackMessage = message;
        _feedbackColor = color;
        _feedbackTimer = _feedbackDuration;
    }

    private void FinishAddition(bool success)
    {
        _additionComplete = true;

        // Calculate damage
        if (success)
        {
            // Full completion bonus
            _finalDamage = (int)(_baseDamage * _currentAddition.DamageMultipliers[^1]);
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
        var centerScreen = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

        // Draw addition name
        var titleSize = _titleFont.MeasureString(_currentAddition.Name);
        _spriteBatch.DrawString(_titleFont, _currentAddition.Name,
            new Vector2(centerScreen.X - (titleSize.X / 2), 100), Color.White);

        // Draw hit counter
        var hitCounterText = $"Hit {_currentHitIndex + 1}/{_currentAddition.HitTimings.Length}";
        var counterSize = _font.MeasureString(hitCounterText);
        _spriteBatch.DrawString(_font, hitCounterText,
            new Vector2(centerScreen.X - (counterSize.X / 2), 140), Color.White);

        // Draw hit zone (the green zone that indicates good timing)
        var hitZoneSize = _squareSize * 1.1f;
        DrawSquare(centerScreen, hitZoneSize, 0f, _hitZoneColor);

        // Draw static square (target)
        DrawSquare(centerScreen, _squareSize, 0f, _staticSquareColor);

        // Draw moving square (player needs to time this to match the static square)
        if (!_additionComplete)
        {
            DrawSquareFrame(centerScreen, _movingSquareSize, _rotationAngle, _movingSquareColor);
        }

        // Draw feedback message if active
        if (_feedbackTimer > 0)
        {
            // Calculate alpha based on remaining time
            var alpha = _feedbackTimer / _feedbackDuration;
            var fadeColor = new Color(_feedbackColor.R, _feedbackColor.G, _feedbackColor.B, (byte)(255 * alpha));

            var feedbackSize = _titleFont.MeasureString(_feedbackMessage);
            _spriteBatch.DrawString(_titleFont, _feedbackMessage,
                new Vector2(centerScreen.X - (feedbackSize.X / 2), centerScreen.Y - 100), fadeColor);
        }

        // Draw instructions
        var instructions = _additionComplete
            ? "Press ENTER or SPACE to return to menu"
            : "Press SPACE when the red square aligns with the blue square!";

        var instructionSize = _font.MeasureString(instructions);
        _spriteBatch.DrawString(_font, instructions,
            new Vector2(centerScreen.X - (instructionSize.X / 2), centerScreen.Y + 200), Color.White);

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
                new Vector2(centerScreen.X - (resultSize.X / 2), 200), resultColor);
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

    private void DrawSquare(Vector2 center, float size, float rotation, Color color)
    {
        // Calculate the position for a square centered at the given position
        var halfSize = size / 2;

        // Store the current SpriteBatch transform
        var originalTransform = Matrix.CreateTranslation(new Vector3(-center, 0)) *
                           Matrix.CreateRotationZ(rotation) *
                           Matrix.CreateTranslation(new Vector3(center, 0));

        // Apply transform for rotation
        _spriteBatch.End();
        _spriteBatch.Begin(transformMatrix: originalTransform);

        // Draw the square
        _spriteBatch.Draw(_squareTexture,
            new Rectangle((int)(center.X - halfSize), (int)(center.Y - halfSize), (int)size, (int)size),
            color);

        // Restore original transform
        _spriteBatch.End();
        _spriteBatch.Begin();
    }

    private void DrawSquareFrame(Vector2 center, float size, float rotation, Color color, float thickness = 10f)
    {
        // Calculate the position for a square centered at the given position
        var halfSize = size / 2;

        // Store the current SpriteBatch transform for rotation
        var originalTransform = Matrix.CreateTranslation(new Vector3(-center, 0)) *
                        Matrix.CreateRotationZ(rotation) *
                        Matrix.CreateTranslation(new Vector3(center, 0));

        // Apply transform for rotation
        _spriteBatch.End();
        _spriteBatch.Begin(transformMatrix: originalTransform);

        // Draw the top edge
        _spriteBatch.Draw(_pixel,
            new Rectangle((int)(center.X - halfSize), (int)(center.Y - halfSize),
                         (int)size, (int)thickness),
            color);

        // Draw the bottom edge
        _spriteBatch.Draw(_pixel,
            new Rectangle((int)(center.X - halfSize), (int)(center.Y + halfSize - thickness),
                         (int)size, (int)thickness),
            color);

        // Draw the left edge
        _spriteBatch.Draw(_pixel,
            new Rectangle((int)(center.X - halfSize), (int)(center.Y - halfSize),
                         (int)thickness, (int)size),
            color);

        // Draw the right edge
        _spriteBatch.Draw(_pixel,
            new Rectangle((int)(center.X + halfSize - thickness), (int)(center.Y - halfSize),
                         (int)thickness, (int)size),
            color);

        // Restore original transform
        _spriteBatch.End();
        _spriteBatch.Begin();
    }

    private bool IsKeyPressed(KeyboardState currentKeyboardState, Keys key) => currentKeyboardState.IsKeyDown(key) && !_prevKeyboardState.IsKeyDown(key);
}
