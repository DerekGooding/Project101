using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace Project1;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private VertexBuffer _cubeVertexBuffer;
    private IndexBuffer _cubeIndexBuffer;

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
        _cubeVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
        _cubeVertexBuffer.SetData(vertices);

        _cubeIndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, CubeIndices.Length, BufferUsage.WriteOnly);
        _cubeIndexBuffer.SetData(CubeIndices);

        _basicEffect = new BasicEffect(GraphicsDevice)
        {
            LightingEnabled = false,
            TextureEnabled = false,
            VertexColorEnabled = true // Important: you're using VertexPositionColor!
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

        _basicEffect.Projection = projection;
        _basicEffect.View = view;

        foreach (var y in Enumerable.Range(0, _map.Height))
        {
            foreach (var x in Enumerable.Range(0, _map.Width))
            {
                if (!_map.IsWalkable(new Point(x, y)))
                {
                    var world = Matrix.CreateTranslation(new Vector3(x * tileSize, 0, y * tileSize));
                    _basicEffect.World = world;

                    foreach (var pass in _basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, CubeIndices.Length / 3);
                    }
                }
            }
        }
    }


    public static VertexPositionColor[] CreateCubeVertices(float size)
    {
        var s = size / 2f;
        return
        [
        new VertexPositionColor(new Vector3(-s, -s, -s), Color.Red),
        new VertexPositionColor(new Vector3(s, -s, -s), Color.White),
        new VertexPositionColor(new Vector3(s, s, -s), Color.White),
        new VertexPositionColor(new Vector3(-s, s, -s), Color.White),
        new VertexPositionColor(new Vector3(-s, -s, s), Color.White),
        new VertexPositionColor(new Vector3(s, -s, s), Color.White),
        new VertexPositionColor(new Vector3(s, s, s), Color.White),
        new VertexPositionColor(new Vector3(-s, s, s), Color.White),
        ];
    }

    public static readonly short[] CubeIndices =
[
    0, 1, 2, 2, 3, 0, // Front face
    4, 5, 6, 6, 7, 4, // Back face
    4, 5, 1, 1, 0, 4, // Bottom face
    3, 2, 6, 6, 7, 3, // Top face
    4, 0, 3, 3, 7, 4, // Left face
    1, 5, 6, 6, 2, 1  // Right face
    ];
}
