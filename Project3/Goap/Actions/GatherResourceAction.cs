namespace Project3.Goap.Actions;

public class GatherResourceAction : GOAPAction
{
    public GatherResourceAction()
    {
        Preconditions["hasResource"] = false;
        Preconditions["isResting"] = false;
        Effects["hasResource"] = true;
        Cost = 1.0f;
    }

    public override bool CheckPreconditions(Agent agent, Dictionary<string, object> worldState) => !agent.HasResource && agent.Energy > 10;

    public override bool Perform(Agent agent, GameTime gameTime, WorldState worldState)
    {
        if (Target is Resource resource)
        {
            if (resource.Amount <= 0)
            {
                Resource? closestResource = null;
                var closestDistance = float.MaxValue;

                foreach (var r in worldState.Resources)
                {
                    if (r.Amount > 0)
                    {
                        var distance = Vector2.Distance(agent.Position, r.Position);
                        if (distance < closestDistance)
                        {
                            closestResource = r;
                            closestDistance = distance;
                        }
                    }
                }

                Target = closestResource;
                return false;
            }

            var direction = resource.Position - agent.Position;

            if (direction.Length() < 5)
            {
                var amount = Math.Min(resource.Amount, 20);
                resource.Amount -= amount;
                agent.CarryingAmount += amount;
                agent.HasResource = true;
                agent.Energy -= 5;
                return true; // Action completed successfully
            }
            else
            {
                if (direction.Length() > 0)
                    direction.Normalize();

                agent.Position += direction * agent.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                agent.Energy -= 0.1f;
                return false; // Action still in progress
            }
        }
        else
        {
            return true; // Return true to force replanning
        }
    }
}