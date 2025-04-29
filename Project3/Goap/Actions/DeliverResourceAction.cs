namespace Project3.Goap.Actions;

public class DeliverResourceAction : GOAPAction
{
    public DeliverResourceAction()
    {
        Preconditions["hasResource"] = true;
        Preconditions["isResting"] = false;
        Effects["hasResource"] = false;
        Effects["deliveredResource"] = true;
        Cost = 1.0f;
    }

    public override bool CheckPreconditions(Agent agent, Dictionary<string, object> worldState) => agent.HasResource && agent.Energy > 5;

    public override bool Perform(Agent agent, GameTime gameTime, WorldState worldState)
    {
        if (Target is Home home)
        {
            var direction = home.Position - agent.Position;

            if (direction.Length() < 5)
            {
                home.StoredResources += agent.CarryingAmount;
                agent.CarryingAmount = 0;
                agent.HasResource = false;
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
            Home? closestHome = null;
            var closestDistance = float.MaxValue;

            foreach (var h in worldState.Homes)
            {
                var distance = Vector2.Distance(agent.Position, h.Position);
                if (distance < closestDistance)
                {
                    closestHome = h;
                    closestDistance = distance;
                }
            }

            Target = closestHome;
            return false; // Need to try again with a valid target
        }
    }
}