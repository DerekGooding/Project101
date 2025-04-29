namespace Project3.Goap.Actions;

public class RestAction : GOAPAction
{
    public RestAction()
    {
        Preconditions["isResting"] = false;
        Effects["isResting"] = true;
        Effects["hasEnergy"] = true;
        Cost = 1.0f;
    }

    public override bool CheckPreconditions(Agent agent, Dictionary<string, object> worldState) => agent.Energy < 40;

    public override bool Perform(Agent agent, GameTime gameTime, WorldState worldState)
    {
        if (Target is Home home)
        {
            var direction = home.Position - agent.Position;

            if (direction.Length() < 5)
            {
                agent.Energy += 20 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                agent.CurrentState = "Resting";

                if (agent.Energy >= 100)
                {
                    agent.Energy = 100;
                    return true; // Action completed when energy is full
                }

                return false; // Still resting
            }
            else
            {
                if (direction.Length() > 0)
                    direction.Normalize();

                agent.Position += direction * agent.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                agent.Energy -= 0.05f;
                agent.CurrentState = "Going to rest";
                return false; // Still moving to home
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