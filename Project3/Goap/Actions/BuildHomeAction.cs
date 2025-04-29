namespace Project3.Goap.Actions;

public class BuildHomeAction : GOAPAction
{
    private readonly Random _random = Random.Shared;
    private Vector2 _buildLocation;
    private bool _locationSelected = false;
    private const int REQUIRED_RESOURCES = 50;

    public BuildHomeAction()
    {
        Preconditions["hasResource"] = true;
        Preconditions["isResting"] = false;
        Effects["builtHome"] = true;
        Effects["hasResource"] = false;
        Cost = 3.0f; // More expensive than other actions
    }

    public override bool CheckPreconditions(Agent agent, Dictionary<string, object> worldState)
        => agent.HasResource && agent.CarryingAmount >= REQUIRED_RESOURCES && agent.Energy > 30;

    public override bool Perform(Agent agent, GameTime gameTime, WorldState worldState)
    {
        if (!_locationSelected)
        {
            bool validLocation;
            var attempts = 0;

            do
            {
                validLocation = true;
                _buildLocation = new Vector2(
                    _random.Next(100, 1180),
                    _random.Next(100, 620));

                foreach (var home in worldState.Homes)
                {
                    if (Vector2.Distance(home.Position, _buildLocation) < 150)
                    {
                        validLocation = false;
                        break;
                    }
                }

                attempts++;
            } while (!validLocation && attempts < 20);

            _locationSelected = true;
            agent.CurrentState = "Building Home";
        }

        var direction = _buildLocation - agent.Position;

        if (direction.Length() < 5)
        {
            var newHome = new Home(_buildLocation);
            worldState.Homes.Add(newHome);

            agent.CarryingAmount -= REQUIRED_RESOURCES;
            if (agent.CarryingAmount <= 0)
            {
                agent.CarryingAmount = 0;
                agent.HasResource = false;
            }

            agent.Energy -= 20;

            _locationSelected = false;
            return true; // Action completed
        }
        else
        {
            if (direction.Length() > 0)
                direction.Normalize();

            agent.Position += direction * agent.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            agent.Energy -= 0.15f; // Building requires more energy
            return false; // Still moving to build location
        }
    }
}