using Project1.Dialogue;
using Project1.Dungeon;
using Project1.Inventory;
using Project1.Inventory.Items;

namespace Project1;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private VertexBuffer _cubeVertexBuffer;
    private IndexBuffer _cubeIndexBuffer;
    private Texture2D _wallTexture;
    private Texture2D _floorTexture;
    private SpriteFont _compassFont;

    private VertexPositionColorTexture[] _floorVertices;
    private short[] _floorIndices;

    private BasicEffect _basicEffect;
    private Map _map;
    private Controller _player;
    private Camera _camera;

    private Manager _dialogueManager;
    private Database _dialogueDatabase;

    private Player _playerCharacter;
    private Inventory.Inventory _inventory;
    private InventoryUI _inventoryUI;
    private ItemDatabase _itemDatabase;
    private ItemPickupManager _itemPickupManager;
    private Texture2D _itemsTexture;
    private KeyboardState _previousKeyboardState;

    private Minimap _minimap;

    private const float TileSize = 2f;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720
        };
        _graphics.ApplyChanges();

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _map = new Map();
        _player = new Controller(new Point(1, 1), _map);
        _camera = new Camera();

        _playerCharacter = new Player();
        _inventory = _playerCharacter.Inventory;

        _itemDatabase = CreateItemDatabase();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _minimap = new Minimap(this, _map, _spriteBatch);

        _dialogueManager = new Manager(this, _spriteBatch, Content.Load<SpriteFont>("CompassFont"));
        _dialogueDatabase = CreateDialogueDatabase();

        var vertices = CreateCubeVertices(TileSize);
        _cubeVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
        _cubeVertexBuffer.SetData(vertices);

        _cubeIndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, CubeIndices.Length, BufferUsage.WriteOnly);
        _cubeIndexBuffer.SetData(CubeIndices);

        _floorVertices = CreateFloorQuad(TileSize);
        _floorIndices = [0, 1, 2, 0, 2, 3];

        _wallTexture = Content.Load<Texture2D>("StoneTexture");
        _floorTexture = Content.Load<Texture2D>("RockyTexture");
        _compassFont = Content.Load<SpriteFont>("CompassFont");

        _basicEffect = new BasicEffect(GraphicsDevice)
        {
            LightingEnabled = false,
        };

        _itemsTexture = new Texture2D(GraphicsDevice, 32, 32);
        var colorData = new Color[32 * 32];
        for (var i = 0; i < colorData.Length; i++)
            colorData[i] = Color.White;
        _itemsTexture.SetData(colorData);

        _inventoryUI = new InventoryUI(this, _spriteBatch, _inventory, _compassFont, _itemsTexture);

        _itemPickupManager = new ItemPickupManager(_playerCharacter, _spriteBatch, _compassFont, _itemsTexture);

        _itemPickupManager.AddPickup(new ItemPickup(new Point(1, 1), _itemDatabase.GetItem("health_potion_small"), 3));
        _itemPickupManager.AddPickup(new ItemPickup(new Point(3, 3), _itemDatabase.GetItem("rusty_key")));
        _itemPickupManager.AddPickup(new ItemPickup(new Point(1, 5), _itemDatabase.GetItem("gold_coin"), 25));

        _inventory.AddItem(_itemDatabase.GetItem("sword"));
    }

    protected override void Update(GameTime gameTime)
    {
        var keyState = Keyboard.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || IsKeyPressed(Keys.Escape, keyState)) Exit();

        if (IsKeyPressed(Keys.I, keyState)) _inventoryUI.Toggle();

        if (_inventoryUI.IsVisible)
        {
            _inventoryUI.Update(gameTime, keyState);
        }
        else if (_dialogueManager.IsActive)
        {
            _dialogueManager.Update(gameTime, keyState);
        }
        else
        {
            _player.Update(gameTime, keyState);
            _camera.Update(_player.GetWorldPosition(TileSize), _player.Rotation);
            _minimap.UpdateExplored(_player.GridPosition);

            _itemPickupManager.CheckPickups(_player.GridPosition);

            if (IsKeyPressed(Keys.F, keyState))
            {
                _itemPickupManager.CheckPickups(_player.GridPosition);
            }

            var trigger = _dialogueDatabase.GetTriggerAtPosition(_player.GridPosition);
            if (trigger != null && IsKeyPressed(Keys.E, keyState))
            {
                var tree = _dialogueDatabase.GetDialogueTree(trigger.DialogueTreeId);
                if (tree != null)
                {
                    _dialogueManager.StartDialogue(tree, trigger.StartDialogueId);
                }
            }
        }

        _previousKeyboardState = keyState;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        DrawFloor(_camera.View, _camera.Projection);

        DrawDungeon(_camera.Projection, _camera.View, TileSize);

        _spriteBatch.Begin();
        DrawCompass(_spriteBatch);
        if (!_dialogueManager.IsActive)
        {
            DrawDialogueTriggerIndicators();
        }
        DrawHUD();
        _itemPickupManager.DrawPickupIndicators(_player.GridPosition);

        _minimap.Draw(_player.GridPosition);
        if (_dialogueManager.IsActive)
        {
            _dialogueManager.Draw();
        }

        if (_inventoryUI.IsVisible)
        {
            _inventoryUI.Draw();
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawDungeon(Matrix projection, Matrix view, float tileSize)
    {
        GraphicsDevice.SetVertexBuffer(_cubeVertexBuffer);
        GraphicsDevice.Indices = _cubeIndexBuffer;

        _basicEffect.TextureEnabled = true;
        _basicEffect.VertexColorEnabled = false;
        _basicEffect.Texture = _wallTexture;
        _basicEffect.Projection = projection;
        _basicEffect.View = view;

        for (var y = 0; y < _map.Height; y++)
        {
            for (var x = 0; x < _map.Width; x++)
            {
                if (!_map.IsWalkable(new Point(x, y)))
                {
                    _basicEffect.World = Matrix.CreateTranslation(new Vector3(x * tileSize, 0, y * tileSize));

                    foreach (var pass in _basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, CubeIndices.Length / 3);
                    }
                }
            }
        }
    }

    private void DrawCompass(SpriteBatch spriteBatch)
    {
        var direction = _player.FacingDirection switch
        {
            0 => "N",
            3 => "E",
            2 => "S",
            1 => "W",
            _ => "?"
        };

        var position = new Vector2(20, 20);

        spriteBatch.DrawString(_compassFont, direction, position, Color.White);
    }

    public static VertexPositionTexture[] CreateCubeVertices(float size)
    {
        var s = size / 2f;

        return
        [
        // Front face (z = -s)
        new VertexPositionTexture(new Vector3(-s, -s, -s), new Vector2(0, 1)),
        new VertexPositionTexture(new Vector3(s, -s, -s), new Vector2(1, 1)),
        new VertexPositionTexture(new Vector3(s, s, -s), new Vector2(1, 0)),
        new VertexPositionTexture(new Vector3(-s, s, -s), new Vector2(0, 0)),

        // Back face (z = s)
        new VertexPositionTexture(new Vector3(s, -s, s), new Vector2(0, 1)),
        new VertexPositionTexture(new Vector3(-s, -s, s), new Vector2(1, 1)),
        new VertexPositionTexture(new Vector3(-s, s, s), new Vector2(1, 0)),
        new VertexPositionTexture(new Vector3(s, s, s), new Vector2(0, 0)),

        // Top face (y = s)
        new VertexPositionTexture(new Vector3(-s, s, -s), new Vector2(0, 1)),
        new VertexPositionTexture(new Vector3(s, s, -s), new Vector2(1, 1)),
        new VertexPositionTexture(new Vector3(s, s, s), new Vector2(1, 0)),
        new VertexPositionTexture(new Vector3(-s, s, s), new Vector2(0, 0)),

        // Bottom face (y = -s)
        new VertexPositionTexture(new Vector3(-s, -s, s), new Vector2(0, 1)),
        new VertexPositionTexture(new Vector3(s, -s, s), new Vector2(1, 1)),
        new VertexPositionTexture(new Vector3(s, -s, -s), new Vector2(1, 0)),
        new VertexPositionTexture(new Vector3(-s, -s, -s), new Vector2(0, 0)),

        // Left face (x = -s)
        new VertexPositionTexture(new Vector3(-s, -s, s), new Vector2(0, 1)),
        new VertexPositionTexture(new Vector3(-s, -s, -s), new Vector2(1, 1)),
        new VertexPositionTexture(new Vector3(-s, s, -s), new Vector2(1, 0)),
        new VertexPositionTexture(new Vector3(-s, s, s), new Vector2(0, 0)),

        // Right face (x = s)
        new VertexPositionTexture(new Vector3(s, -s, -s), new Vector2(0, 1)),
        new VertexPositionTexture(new Vector3(s, -s, s), new Vector2(1, 1)),
        new VertexPositionTexture(new Vector3(s, s, s), new Vector2(1, 0)),
        new VertexPositionTexture(new Vector3(s, s, -s), new Vector2(0, 0))
        ];
    }

    public static readonly short[] CubeIndices =
    [
    0, 1, 2, 0, 2, 3,       // Front face
    4, 5, 6, 4, 6, 7,       // Back face
    8, 9, 10, 8, 10, 11,    // Top face
    12, 13, 14, 12, 14, 15, // Bottom face
    16, 17, 18, 16, 18, 19, // Left face
    20, 21, 22, 20, 22, 23  // Right face
    ];

    private VertexPositionColorTexture[] CreateFloorQuad(float size)
    {
        var half = size / 2f;
        // Make sure the floor is very slightly below 0 to avoid z-fighting
        const float y = -1.0f;

        return
        [
        new(new Vector3(-half, y, -half), Color.White, new Vector2(0, 0)),
        new(new Vector3(half, y, -half), Color.White, new Vector2(1, 0)),
        new(new Vector3(half, y, half), Color.White, new Vector2(1, 1)),
        new(new Vector3(-half, y, half), Color.White, new Vector2(0, 1))
        ];
    }

    private void DrawFloor(Matrix view, Matrix projection)
    {
        // Basic effect without textures, just vertex colors
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        _basicEffect.World = Matrix.Identity;
        _basicEffect.View = view;
        _basicEffect.Projection = projection;
        _basicEffect.TextureEnabled = true;
        _basicEffect.Texture = _floorTexture;
        _basicEffect.VertexColorEnabled = true;

        foreach (var y in Enumerable.Range(0, _map.Height))
        {
            foreach (var x in Enumerable.Range(0, _map.Width))
            {
                if (_map.IsWalkable(new Point(x, y)))
                {
                    _basicEffect.World = Matrix.CreateTranslation(new Vector3(x * TileSize, 0, y * TileSize));

                    foreach (var pass in _basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawUserIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            _floorVertices,
                            0,
                            _floorVertices.Length,
                            _floorIndices,
                            0,
                            _floorIndices.Length / 3);
                    }
                }
            }
        }
    }

    private Database CreateDialogueDatabase()
    {
        var database = new Database();

        // Create a simple dialogue tree
        var welcomeTree = new Tree();
        welcomeTree.AddDialogue("start", "Welcome to the dungeon, brave adventurer!", "Guardian")
            .AddOption("Who are you?", "who")
            .AddOption("What is this place?", "place")
            .AddOption("I'll be on my way.", "END");

        welcomeTree.AddDialogue("who", "I am the guardian of this dungeon. I've been here for centuries.", "Guardian")
            .AddOption("What is this place?", "place")
            .AddOption("I'll be on my way.", "END");

        welcomeTree.AddDialogue("place", "This is the Dungeon of Eternal Shadows. Many treasures lie within... and many dangers.", "Guardian")
            .AddOption("Who are you?", "who")
            .AddOption("I'll be on my way.", "END");

        database.AddDialogueTree("welcome", welcomeTree);

        // Add a trigger in the dungeon
        database.AddTrigger(new Trigger(new Point(2, 1), "welcome", "start"));

        return database;
    }

    private void DrawDialogueTriggerIndicators()
    {
        var position = _player.GridPosition;
        var trigger = _dialogueDatabase.GetTriggerAtPosition(position);

        if (trigger != null)
        {
            var screenPos = new Vector2(GraphicsDevice.Viewport.Width / 2, 100);
            _spriteBatch.DrawString(_compassFont, "Press E to talk", screenPos, Color.Yellow);
        }
    }

    private ItemDatabase CreateItemDatabase()
    {
        var database = new ItemDatabase();

        // Register some basic items
        database.RegisterItem(new HealthPotion("health_potion_small", "Small Health Potion", 20));
        database.RegisterItem(new HealthPotion("health_potion_large", "Large Health Potion", 50));
        database.RegisterItem(new Key("rusty_key", "Rusty Key", "door_1"));
        database.RegisterItem(new Item("gold_coin", "Gold Coin", "A shiny gold coin", ItemType.Treasure, true, 99));
        database.RegisterItem(new Item("sword", "Iron Sword", "A basic iron sword", ItemType.Weapon));
        database.RegisterItem(new Item("shield", "Wooden Shield", "A sturdy wooden shield", ItemType.Armor));

        return database;
    }

    private bool IsKeyPressed(Keys key, KeyboardState currentState) => currentState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);

    private void DrawHUD()
    {
        // Draw health bar
        var healthBarWidth = 200;
        var healthBarHeight = 20;
        var healthBarX = 20;
        var healthBarY = GraphicsDevice.Viewport.Height - healthBarHeight - 20;

        // Draw health bar background
        var backgroundRect = new Rectangle(healthBarX, healthBarY, healthBarWidth, healthBarHeight);
        _spriteBatch.Draw(_wallTexture, backgroundRect, Color.DarkRed);

        // Draw current health
        var healthPercentage = (float)_playerCharacter.Health / _playerCharacter.MaxHealth;
        var healthRect = new Rectangle(healthBarX, healthBarY, (int)(healthBarWidth * healthPercentage), healthBarHeight);
        _spriteBatch.Draw(_wallTexture, healthRect, Color.Red);

        // Draw health text
        var healthText = $"{_playerCharacter.Health}/{_playerCharacter.MaxHealth}";
        var textSize = _compassFont.MeasureString(healthText);
        var textPosition = new Vector2(
            healthBarX + ((healthBarWidth - textSize.X) / 2),
            healthBarY + ((healthBarHeight - textSize.Y) / 2)
        );
        _spriteBatch.DrawString(_compassFont, healthText, textPosition, Color.White);

        // Draw inventory hint
        var inventoryHint = "Press I for Inventory";
        var hintPosition = new Vector2(
            GraphicsDevice.Viewport.Width - _compassFont.MeasureString(inventoryHint).X - 20,
            20
        );
        _spriteBatch.DrawString(_compassFont, inventoryHint, hintPosition, Color.LightGray);
    }
}
