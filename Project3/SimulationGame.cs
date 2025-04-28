using Project3.Goap;

namespace Project3;

public class SimulationGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private List<Agent> _agents;
    private Texture2D _agentTexture;
    private Texture2D _resourceTexture;
    private Texture2D _homeTexture;
    private SpriteFont _font;
    private List<Resource> _resources;
    private List<Home> _homes;
    private readonly Random _random = Random.Shared;

    public SimulationGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
    }

    protected override void Initialize()
    {
        _agents = [];
        _resources = [];
        _homes = [];

        for (var i = 0; i < 10; i++)
        {
            _resources.Add(new Resource(
                new Vector2(_random.Next(100, _graphics.PreferredBackBufferWidth - 100),
                _random.Next(100, _graphics.PreferredBackBufferHeight - 100)),
                _random.Next(50, 100)));
        }

        for (var i = 0; i < 3; i++)
        {
            _homes.Add(new Home(
                new Vector2(_random.Next(100, _graphics.PreferredBackBufferWidth - 100),
                _random.Next(100, _graphics.PreferredBackBufferHeight - 100))));
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _agentTexture = Content.Load<Texture2D>("agent");
        _resourceTexture = Content.Load<Texture2D>("resource");
        _homeTexture = Content.Load<Texture2D>("home");
        _font = Content.Load<SpriteFont>("font");

        for (var i = 0; i < 5; i++)
        {
            var agent = new Agent(
                new Vector2(_random.Next(100, _graphics.PreferredBackBufferWidth - 100),
                _random.Next(100, _graphics.PreferredBackBufferHeight - 100)),
                _agentTexture);

            _agents.Add(agent);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var worldState = new WorldState
        {
            Resources = _resources,
            Homes = _homes
        };

        foreach (var agent in _agents)
        {
            agent.Update(gameTime, worldState);
        }

        _resources.RemoveAll(r => r.Amount <= 0);

        if (_random.NextDouble() < 0.01 && _resources.Count < 15)
        {
            _resources.Add(new Resource(
                new Vector2(_random.Next(100, _graphics.PreferredBackBufferWidth - 100),
                _random.Next(100, _graphics.PreferredBackBufferHeight - 100)),
                _random.Next(50, 100)));
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.ForestGreen);

        _spriteBatch.Begin();

        foreach (var home in _homes)
        {
            _spriteBatch.Draw(_homeTexture, home.Position - new Vector2(_homeTexture.Width / 2, _homeTexture.Height / 2), Color.White);
            _spriteBatch.DrawString(_font, $"Storage: {home.StoredResources}", home.Position + new Vector2(-30, -40), Color.White);
        }

        foreach (var resource in _resources)
        {
            _spriteBatch.Draw(_resourceTexture, resource.Position - new Vector2(_resourceTexture.Width / 2, _resourceTexture.Height / 2), Color.White);
            _spriteBatch.DrawString(_font, $"{resource.Amount}", resource.Position, Color.White);
        }

        foreach (var agent in _agents)
        {
            _spriteBatch.Draw(_agentTexture, agent.Position - new Vector2(_agentTexture.Width / 2, _agentTexture.Height / 2), Color.White);
            _spriteBatch.DrawString(_font, $"{agent.CurrentState}", agent.Position + new Vector2(-30, -20), Color.White);
            _spriteBatch.DrawString(_font, $"Carrying: {agent.CarryingAmount}", agent.Position + new Vector2(-30, -40), Color.White);
            _spriteBatch.DrawString(_font, $"Energy: {agent.Energy:F0}", agent.Position + new Vector2(-30, -60), Color.White);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
