namespace Project3.Goap;

public class ExploreAction : GOAPAction
{
    private readonly Random _random = Random.Shared;
    private Vector2 _targetPosition;
    private float _timeUntilNewTarget = 0;

    public ExploreAction()
    {
        Preconditions["isResting"] = false;
        Effects["explored"] = true;
        Cost = 2.0f;
    }

    public override bool CheckPreconditions(Agent agent, Dictionary<string, object> worldState) => agent.Energy > 20;

    public override bool Perform(Agent agent, GameTime gameTime, WorldState worldState)
    {
        _timeUntilNewTarget -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_timeUntilNewTarget <= 0 || Vector2.Distance(agent.Position, _targetPosition) < 5)
        {
            _targetPosition = new Vector2(
                _random.Next(50, 1230),
                _random.Next(50, 670));
            _timeUntilNewTarget = 5.0f;
        }

        var direction = _targetPosition - agent.Position;
        if (direction.Length() > 0)
            direction.Normalize();

        agent.Position += direction * agent.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        agent.Energy -= 0.1f;
        agent.CurrentState = "Exploring";

        return false;
    }
}
