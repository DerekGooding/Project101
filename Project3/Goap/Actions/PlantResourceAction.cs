namespace Project3.Goap.Actions;

public class PlantResourceAction : GOAPAction
{
    private readonly Random _random = Random.Shared;
    private Vector2 _plantLocation;
    private bool _locationSelected = false;
    private const int REQUIRED_RESOURCES = 10;
    private float _growthTimer = 0;
    private const float GROWTH_TIME = 10.0f; // Time in seconds before the plant becomes a resource

    public PlantResourceAction()
    {
        Preconditions["hasResource"] = true;
        Preconditions["isResting"] = false;
        Effects["plantedResource"] = true;
        Effects["hasResource"] = false; // Uses up some resources
        Cost = 2.0f;
    }

    public override bool CheckPreconditions(Agent agent, Dictionary<string, object> worldState)
        => agent.HasResource && agent.CarryingAmount >= REQUIRED_RESOURCES && agent.Energy > 20;

    public override bool Perform(Agent agent, GameTime gameTime, WorldState worldState)
    {
        if (!_locationSelected)
        {
            bool validLocation;
            var attempts = 0;

            do
            {
                validLocation = true;
                _plantLocation = new Vector2(
                    _random.Next(100, 1180),
                    _random.Next(100, 620));

                foreach (var resource in worldState.Resources)
                {
                    if (Vector2.Distance(resource.Position, _plantLocation) < 100)
                    {
                        validLocation = false;
                        break;
                    }
                }

                attempts++;
            } while (!validLocation && attempts < 20);

            _locationSelected = true;
            agent.CurrentState = "Planting";
        }

        var direction = _plantLocation - agent.Position;

        if (direction.Length() < 5)
        {
            _growthTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_growthTimer >= GROWTH_TIME)
            {
                var newResource = new Resource(_plantLocation, _random.Next(30, 60));
                worldState.Resources.Add(newResource);

                agent.CarryingAmount -= REQUIRED_RESOURCES;
                if (agent.CarryingAmount <= 0)
                {
                    agent.CarryingAmount = 0;
                    agent.HasResource = false;
                }

                agent.Energy -= 10;

                _locationSelected = false;
                _growthTimer = 0;

                return true; // Action completed
            }

            agent.Energy -= 0.1f;
            return false;
        }
        else
        {
            if (direction.Length() > 0)
                direction.Normalize();

            agent.Position += direction * agent.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            agent.Energy -= 0.1f;
            return false; // Still moving to plant location
        }
    }
}