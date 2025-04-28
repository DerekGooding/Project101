namespace Project3.Goap;

public class GOAPPlanner
{
    public List<GOAPAction>? Plan(Agent agent, List<GOAPAction> availableActions, Dictionary<string, object> worldState, Dictionary<string, object> goalState)
    {
        var currentState = new Dictionary<string, object>(worldState);

        var usableActions = new List<GOAPAction>();
        usableActions.AddRange(availableActions);

        var plan = new List<GOAPAction>();
        var goalAchieved = false;

        while (!goalAchieved)
        {
            GOAPAction? bestAction = null;
            var bestCost = float.MaxValue;

            foreach (var action in usableActions)
            {
                if (action.CheckPreconditions(agent, currentState))
                {
                    var actionIsUseful = false;
                    foreach (var effect in action.Effects)
                    {
                        if (goalState.TryGetValue(effect.Key, out var value) && value.Equals(effect.Value))
                        {
                            actionIsUseful = true;
                            break;
                        }
                    }

                    if (actionIsUseful && action.Cost < bestCost)
                    {
                        bestAction = action;
                        bestCost = action.Cost;
                    }
                }
            }

            if (bestAction == null)
                return null;

            plan.Add(bestAction);
            usableActions.Remove(bestAction);

            foreach (var effect in bestAction.Effects)
            {
                currentState[effect.Key] = effect.Value;
            }

            goalAchieved = true;
            foreach (var goal in goalState)
            {
                if (!currentState.TryGetValue(goal.Key, out var value) || !value.Equals(goal.Value))
                {
                    goalAchieved = false;
                    break;
                }
            }

            if (plan.Count > 10)
                return null;
        }

        return plan;
    }
}
