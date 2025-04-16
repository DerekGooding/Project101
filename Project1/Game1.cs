using Project1.Dialogue;
using Project1.Dungeon;

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

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

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
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
        Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        // Update dialogue first - if active, don't update player movement
        if (_dialogueManager.IsActive)
        {
            _dialogueManager.Update(gameTime);
        }
        else
        {
            _player.Update(gameTime);

            // Check for dialogue triggers
            var trigger = _dialogueDatabase.GetTriggerAtPosition(_player.GridPosition);
            if (trigger != null && Keyboard.GetState().IsKeyDown(Keys.E))
            {
                var tree = _dialogueDatabase.GetDialogueTree(trigger.DialogueTreeId);
                if (tree != null)
                {
                    _dialogueManager.StartDialogue(tree, trigger.StartDialogueId);
                }
            }
        }

        _camera.Update(_player.GetWorldPosition(TileSize), _player.Rotation);

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
        _spriteBatch.End();

        if (_dialogueManager.IsActive)
        {
            _spriteBatch.Begin();
            _dialogueManager.Draw();
            _spriteBatch.End();
        }

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
}
