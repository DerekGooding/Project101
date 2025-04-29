using Project3.Goap.Actions;

namespace Project3.Goap;
public class Agent(Vector2 position, Texture2D texture)
{
    public Vector2 Position { get; set; } = position;
    public float Speed { get; set; } = 100;
    public float Energy { get; set; } = 100;
    public bool HasResource { get; set; }
    public int CarryingAmount { get; set; }
    public string CurrentState { get; set; } = "Idle";

    private readonly GOAPPlanner _planner = new();
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
    private readonly Texture2D _texture = texture;
    private double _planningTimer = 0;

    public void Update(GameTime gameTime, WorldState worldState)
    {
        var currentState = new Dictionary<string, object>
        {
            {"hasResource", HasResource},
            {"hasEnergy", Energy > 50},
            {"isResting", CurrentState == "Resting"},
            {"deliveredResource", false},
            {"explored", false},
            {"builtHome", false},
        };

        _planningTimer -= gameTime.ElapsedGameTime.TotalSeconds;

        var needNewPlan = _currentPlan == null ||
                           _currentActionIndex >= _currentPlan.Count ||
                           _planningTimer <= 0;

        if (_currentPlan != null && _currentActionIndex < _currentPlan.Count)
        {
            var currentAction = _currentPlan[_currentActionIndex];
            if (currentAction.Perform(this, gameTime, worldState))
            {
                _currentActionIndex++;

                if (_currentActionIndex >= _currentPlan.Count)
                {
                    needNewPlan = true;
                }
            }

            if (!currentAction.CheckPreconditions(this, currentState))
            {
                needNewPlan = true;
            }
        }
        else
        {
            needNewPlan = true;
        }

        if (needNewPlan)
        {
            CreateNewPlan(currentState, worldState);
            _planningTimer = 5.0; // Replan every 5 seconds
        }

        if (_currentPlan != null && _currentActionIndex < _currentPlan.Count)
        {
            var currentAction = _currentPlan[_currentActionIndex];

            UpdateActionTarget(currentAction, worldState);

            currentAction.Perform(this, gameTime, worldState);
        }
        else
        {
            var exploreAction = new ExploreAction();
            UpdateActionTarget(exploreAction, worldState);
            exploreAction.Perform(this, gameTime, worldState);
        }

        Energy -= 0.05f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        Energy = MathHelper.Clamp(Energy, 0, 100);
    }

    private void UpdateActionTarget(GOAPAction action, WorldState worldState)
    {
        if (action is GatherResourceAction)
        {
            CurrentState = "Gathering";

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

            action.Target = closestResource;
        }
        else if (action is DeliverResourceAction)
        {
            CurrentState = "Delivering";

            FindClosestHome(action, worldState);
        }
        else if (action is RestAction)
        {
            CurrentState = "Going to rest";

            FindClosestHome(action, worldState);
        }
        else if (action is BuildHomeAction)
        {
            CurrentState = "Building Home";
        }
        else if (action is ExploreAction)
        {
            CurrentState = "Exploring";
        }
    }

    private void FindClosestHome(GOAPAction action, WorldState worldState)
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

        action.Target = closestHome;
    }

    private void CreateNewPlan(Dictionary<string, object> currentState, WorldState worldState)
    {
        var goalState = new Dictionary<string, object>();

        if (Energy < 30)
        {
            goalState["hasEnergy"] = true;  // Need to rest
        }
        else if (HasResource)
        {
            goalState["deliveredResource"] = true;  // Need to deliver resources
        }
        else if (worldState.Resources.Any(r => r.Amount > 0))
        {
            goalState["hasResource"] = true;  // Need to collect resources
        }
        else
        {
            goalState["explored"] = true;  // Nothing else to do, explore
        }

        _currentPlan = _planner.Plan(this, _availableActions, currentState, goalState);
        _currentActionIndex = 0;

        _currentPlan ??= [new ExploreAction()];
    }
}