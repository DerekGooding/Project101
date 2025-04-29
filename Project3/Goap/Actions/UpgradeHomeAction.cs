namespace Project3.Goap.Actions;

public class UpgradeHomeAction : GOAPAction
{
    private const int REQUIRED_RESOURCES = 30;
    private float _upgradeTimer = 0;
    private const float UPGRADE_TIME = 5.0f;

    public UpgradeHomeAction()
    {
        Preconditions["hasResource"] = true;
        Preconditions["isResting"] = false;
        Effects["upgradedHome"] = true;
        Effects["hasResource"] = false;
        Cost = 2.5f;
    }

    public override bool CheckPreconditions(Agent agent, Dictionary<string, object> worldState)
        => agent.HasResource && agent.CarryingAmount >= REQUIRED_RESOURCES && agent.Energy > 25;

    public override bool Perform(Agent agent, GameTime gameTime, WorldState worldState)
    {
        if (Target is Home home)
        {
            var direction = home.Position - agent.Position;

            if (direction.Length() < 5)
            {
                // We've reached the home to upgrade
                _upgradeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                agent.CurrentState = "Upgrading Home";

                if (_upgradeTimer >= UPGRADE_TIME)
                {
                    // Upgrade complete
                    home.Level++;
                    home.StorageCapacity += 50;
                    home.RestEfficiency += 0.5f;

                    // Use resources
                    agent.CarryingAmount -= REQUIRED_RESOURCES;
                    if (agent.CarryingAmount <= 0)
                    {
                        agent.CarryingAmount = 0;
                        agent.HasResource = false;
                    }

                    agent.Energy -= 15;
                    _upgradeTimer = 0;

                    return true; // Action completed
                }

                // Still upgrading
                agent.Energy -= 0.2f;
                return false;
            }
            else
            {
                // Moving to home
                if (direction.Length() > 0)
                    direction.Normalize();

                agent.Position += direction * agent.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                agent.Energy -= 0.1f;
                agent.CurrentState = "Going to upgrade home";
                return false;
            }
        }
        else
        {
            // Find an upgradable home
            Home? bestHomeToUpgrade = null;
            var lowestLevel = int.MaxValue;

            foreach (var h in worldState.Homes)
            {
                if (h.Level < lowestLevel)
                {
                    bestHomeToUpgrade = h;
                    lowestLevel = h.Level;
                }
            }

            Target = bestHomeToUpgrade;
            return false; // Need to try again with a valid target
        }
    }
}