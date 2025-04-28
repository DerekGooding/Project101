namespace Project3.Goap;

public class GatherResourceAction : GOAPAction
{
    public GatherResourceAction()
    {
        Preconditions["hasResource"] = false;
        Preconditions["isResting"] = false;
        Effects["hasResource"] = true;
        Cost = 1.0f;
    }

    public override bool CheckPreconditions(Agent agent, Dictionary<string, object> worldState) => !agent.HasResource && agent.Energy > 10 && Target != null;

    public override bool Perform(Agent agent, GameTime gameTime, WorldState worldState)
    {
        if (Target is Resource resource)
        {
            var direction = resource.Position - agent.Position;

            if (direction.Length() < 5)
            {
                if (resource.Amount > 0)
                {
                    var amount = Math.Min(resource.Amount, 20);
                    resource.Amount -= amount;
                    agent.CarryingAmount += amount;
                    agent.HasResource = true;
                    agent.Energy -= 5;
                    return true;
                }
            }
            else
            {
                if (direction.Length() > 0)
                    direction.Normalize();

                agent.Position += direction * agent.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                agent.Energy -= 0.1f;
            }
        }

        return false;
    }
}
