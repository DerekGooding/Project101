namespace Project1.Dialogue;

// Manages the dialogue state and rendering
public class DialogueManager
{
    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _font;
    private readonly SpriteFont _optionFont;
    private readonly Texture2D _dialogueBoxTexture;
    private readonly Texture2D _optionBoxTexture;

    private DialogueTree _currentTree;
    private Line _currentLine;
    private int _selectedOption = 0;
    private bool _isActive = false;
    private KeyboardState _previousKeyboardState;

    // Dimensions and positions
    private readonly Rectangle _dialogueBoxRect;
    private readonly Color _dialogueBoxColor = new(0, 0, 0, 200);
    private readonly Color _textColor = Color.White;
    private readonly Color _speakerColor = Color.Yellow;
    private readonly Color _selectedOptionColor = Color.LightBlue;
    private readonly int _textPadding = 20;

    public bool IsActive => _isActive;

    public DialogueManager(Game game, SpriteBatch spriteBatch, SpriteFont font, SpriteFont optionFont = null)
    {
        _spriteBatch = spriteBatch;
        _font = font;
        _optionFont = optionFont ?? font;

        // Create a 1x1 white texture for drawing boxes
        _dialogueBoxTexture = new Texture2D(game.GraphicsDevice, 1, 1);
        _dialogueBoxTexture.SetData([Color.White]);
        _optionBoxTexture = _dialogueBoxTexture;

        // Set dialogue box to bottom third of screen
        var viewport = game.GraphicsDevice.Viewport;
        _dialogueBoxRect = new Rectangle(
            _textPadding,
            viewport.Height - (viewport.Height / 3) - _textPadding,
            viewport.Width - (_textPadding * 2),
            viewport.Height / 3
        );
    }

    public void StartDialogue(DialogueTree tree, string startDialogueId)
    {
        _currentTree = tree;
        _currentLine = tree.GetDialogue(startDialogueId);
        _selectedOption = 0;
        _isActive = true;
    }

    public void EndDialogue()
    {
        _isActive = false;
        _currentTree = null;
        _currentLine = null;
    }

    public void Update(GameTime gameTime, KeyboardState keyboardState)
    {
        if (!_isActive || _currentLine == null)
            return;

        // Handle selecting options
        if (_currentLine.Options.Count > 0)
        {
            if (IsKeyPressed(Keys.Down, keyboardState) && _selectedOption < _currentLine.Options.Count - 1)
            {
                _selectedOption++;
            }
            else if (IsKeyPressed(Keys.Up, keyboardState) && _selectedOption > 0)
            {
                _selectedOption--;
            }
            else if (IsKeyPressed(Keys.Enter, keyboardState) || IsKeyPressed(Keys.Space, keyboardState))
            {
                var nextDialogueId = _currentLine.Options[_selectedOption].NextDialogueId;
                if (nextDialogueId == "END")
                {
                    EndDialogue();
                }
                else
                {
                    _currentLine = _currentTree.GetDialogue(nextDialogueId);
                    _selectedOption = 0;
                }
            }
        }
        else
        {
            // Without options, any key advances
            if (IsKeyPressed(Keys.Enter, keyboardState) || IsKeyPressed(Keys.Space, keyboardState))
            {
                EndDialogue();
            }
        }

        _previousKeyboardState = keyboardState;
    }

    public void Draw()
    {
        if (!_isActive || _currentLine == null)
            return;

        // Draw dialogue box background
        _spriteBatch.Draw(_dialogueBoxTexture, _dialogueBoxRect, _dialogueBoxColor);

        // Draw speaker name if available
        var textPosition = new Vector2(_dialogueBoxRect.X + _textPadding, _dialogueBoxRect.Y + _textPadding);
        if (!string.IsNullOrEmpty(_currentLine.Speaker))
        {
            _spriteBatch.DrawString(_font, _currentLine.Speaker + ":", textPosition, _speakerColor);
            textPosition.Y += _font.MeasureString(_currentLine.Speaker).Y + 5;
        }

        // Draw dialogue text
        _spriteBatch.DrawString(_font, _currentLine.Text, textPosition, _textColor);

        // Draw options if available
        if (_currentLine.Options.Count > 0)
        {
            textPosition.Y += _font.MeasureString(_currentLine.Text).Y + 20;

            for (var i = 0; i < _currentLine.Options.Count; i++)
            {
                var optionText = $"> {_currentLine.Options[i].Text}";
                var color = i == _selectedOption ? _selectedOptionColor : _textColor;
                _spriteBatch.DrawString(_optionFont, optionText, textPosition, color);
                textPosition.Y += _optionFont.MeasureString(optionText).Y + 5;
            }
        }
        else
        {
            // Draw prompt to continue
            textPosition.Y = _dialogueBoxRect.Bottom - _font.LineSpacing - _textPadding;
            _spriteBatch.DrawString(_font, "Press Space to continue...", textPosition, _textColor * 0.7f);
        }
    }

    private bool IsKeyPressed(Keys key, KeyboardState currentKeyboardState)
        => currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
}
