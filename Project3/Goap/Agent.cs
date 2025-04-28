namespace Project3.Goap;
public class Agent(Vector2 position, Texture2D texture)
{
    public Vector2 Position { get; set; } = position;
    public float Speed { get; set; } = 100;
    public float Energy { get; set; } = 100;
    public bool HasResource { get; set; } = false;
    public int CarryingAmount { get; set; } = 0;
    public string CurrentState { get; set; } = "Idle";

    private GOAPPlanner _planner = new GOAPPlanner();
    private readonly List<GOAPAction> _availableActions =
        [
            new GatherResourceAction(),
            new DeliverResourceAction(),
            new RestAction(),
            new ExploreAction()
        ];
    private List<GOAPAction>? _currentPlan = null;
    private int _currentActionIndex = 0;
    private readonly Random _random = Random.Shared;
    private Texture2D _texture = texture;

    public void Update(GameTime gameTime, WorldState worldState)
    {
        // Create current world state
        var currentState = new Dictionary<string, object>
        {
            {"hasResource", HasResource},
            {"hasEnergy", Energy > 50},
            {"isResting", CurrentState == "Resting"},
            {"deliveredResource", false},
            {"explored", false}
        };

        // If no plan or current action completed, create a new plan
        if (_currentPlan == null || _currentActionIndex >= _currentPlan.Count ||
            (_currentActionIndex < _currentPlan.Count &&
            _currentPlan[_currentActionIndex].Perform(this, gameTime, worldState)))
        {
            CreateNewPlan(currentState, worldState);
        }

        // Execute current action
        if (_currentPlan != null && _currentActionIndex < _currentPlan.Count)
        {
            var currentAction = _currentPlan[_currentActionIndex];

            if (currentAction is GatherResourceAction)
            {
                CurrentState = "Gathering";

                // Find closest resource if no target
                if (currentAction.Target == null || currentAction.Target is not Resource r || r.Amount <= 0)
                {
                    Resource? closestResource = null;
                    var closestDistance = float.MaxValue;

                    foreach (var resource in worldState.Resources)
                    {
                        if (resource.Amount > 0)
                        {
                            var distance = Vector2.Distance(Position, resource.Position);
                            if (distance < closestDistance)
                            {
                                closestResource = resource;
                                closestDistance = distance;
                            }
                        }
                    }

                    currentAction.Target = closestResource;
                }
            }
            else if (currentAction is DeliverResourceAction or RestAction)
            {
                if (currentAction.Target is null or not Home)
                {
                    Home? closestHome = null;
                    var closestDistance = float.MaxValue;

                    foreach (var home in worldState.Homes)
                    {
                        var distance = Vector2.Distance(Position, home.Position);
                        if (distance < closestDistance)
                        {
                            closestHome = home;
                            closestDistance = distance;
                        }
                    }

                    currentAction.Target = closestHome;
                }

                if (currentAction is DeliverResourceAction)
                    CurrentState = "Delivering";
            }

            currentAction.Perform(this, gameTime, worldState);
        }
        else
        {
            var exploreAction = new ExploreAction();
            exploreAction.Perform(this, gameTime, worldState);
        }

        Energy -= 0.05f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        Energy = MathHelper.Clamp(Energy, 0, 100);
    }

    private void CreateNewPlan(Dictionary<string, object> currentState, WorldState worldState)
    {
        var goalState = new Dictionary<string, object>();

        if (Energy < 30)
        {
            goalState["hasEnergy"] = true;
        }
        else if (HasResource)
        {
            goalState["deliveredResource"] = true;
        }
        else if (worldState.Resources.Any(r => r.Amount > 0))
        {
            goalState["hasResource"] = true;
        }
        else
        {
            goalState["explored"] = true;
        }

        _currentPlan = _planner.Plan(this, _availableActions, currentState, goalState);
        _currentActionIndex = 0;

        _currentPlan ??= [new ExploreAction()];
    }
}
