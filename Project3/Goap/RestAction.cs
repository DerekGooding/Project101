namespace Project3.Goap;

public class RestAction : GOAPAction
{
    public RestAction()
    {
        Preconditions["isResting"] = false;
        Effects["isResting"] = true;
        Effects["hasEnergy"] = true;
        Cost = 1.0f;
    }

    public override bool CheckPreconditions(Agent agent, Dictionary<string, object> worldState) => agent.Energy < 40 && Target != null;

    public override bool Perform(Agent agent, GameTime gameTime, WorldState worldState)
    {
        if (Target is Home home)
        {
            // Move towards home
            var direction = home.Position - agent.Position;

            if (direction.Length() < 5)
            {
                // Rest
                agent.Energy += 20 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                agent.CurrentState = "Resting";

                if (agent.Energy >= 100)
                {
                    agent.Energy = 100;
                    return true;
                }
            }
            else
            {
                // Move towards home
                if (direction.Length() > 0)
                    direction.Normalize();

                agent.Position += direction * agent.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                agent.Energy -= 0.05f;
                agent.CurrentState = "Going to rest";
            }
        }

        return false;
    }
}
