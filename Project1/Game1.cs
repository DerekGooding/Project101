using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Project1;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private VertexBuffer _cubeVertexBuffer;
    private IndexBuffer _cubeIndexBuffer;
    private Texture2D _wallTexture;

    private BasicEffect _basicEffect;
    private DungeonMap _map;
    private DungeonCrawlerController _player;
    private DungeonCamera _camera;

    private const float TileSize = 2f;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _map = new DungeonMap();
        _player = new DungeonCrawlerController(new Point(2, 2), _map);
        _camera = new DungeonCamera();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        var vertices = CreateCubeVertices(TileSize);
        _cubeVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
        _cubeVertexBuffer.SetData(vertices);

        _cubeIndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, CubeIndices.Length, BufferUsage.WriteOnly);
        _cubeIndexBuffer.SetData(CubeIndices);

        _wallTexture = Content.Load<Texture2D>("StoneTexture");

        _basicEffect = new BasicEffect(GraphicsDevice)
        {
            LightingEnabled = false,
            TextureEnabled = true,
            VertexColorEnabled = false
        };
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        _player.Update(gameTime);
        _camera.Update(_player.GetWorldPosition(TileSize), _player.Rotation);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        DrawDungeon(_camera.Projection, _camera.View, TileSize);

        base.Draw(gameTime);
    }

    private void DrawDungeon(Matrix projection, Matrix view, float tileSize)
    {
        GraphicsDevice.SetVertexBuffer(_cubeVertexBuffer);
        GraphicsDevice.Indices = _cubeIndexBuffer;

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
}
