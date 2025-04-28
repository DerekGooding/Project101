namespace Project3.Goap;

public abstract class GOAPAction
{
    public Dictionary<string, object> Preconditions { get; protected set; } = [];

    public Dictionary<string, object> Effects { get; protected set; } = [];

    public float Cost { get; protected set; } = 1.0f;

    public object? Target { get; set; }

    public abstract bool CheckPreconditions(Agent agent, Dictionary<string, object> worldState);

    public abstract bool Perform(Agent agent, GameTime gameTime, WorldState worldState);
}
