namespace Project3.Goap;

public class GOAPPlanner
{
    public List<GOAPAction>? Plan(Agent agent, List<GOAPAction> availableActions, Dictionary<string, object> worldState, Dictionary<string, object> goalState)
    {
        var currentState = new Dictionary<string, object>(worldState);

        var usableActions = availableActions
            .Where(a => a.CheckPreconditions(agent, worldState))
            .ToList();

        if (usableActions.Count == 0)
            return null;

        return BuildPlan(agent, usableActions, currentState, goalState);
    }

    private List<GOAPAction>? BuildPlan(Agent agent,
                                      List<GOAPAction> availableActions,
                                      Dictionary<string, object> worldState,
                                      Dictionary<string, object> goalState)
    {
        var allGoalsMet = true;
        foreach (var goal in goalState)
        {
            if (!worldState.TryGetValue(goal.Key, out var value) || !value.Equals(goal.Value))
            {
                allGoalsMet = false;
                break;
            }
        }

        if (allGoalsMet)
            return [];

        var bestPlan = new List<GOAPAction>();
        var bestCost = float.MaxValue;

        foreach (var action in availableActions)
        {
            var actionAchievesGoal = false;
            foreach (var effect in action.Effects)
            {
                if (goalState.TryGetValue(effect.Key, out var goalValue) &&
                    goalValue.Equals(effect.Value))
                {
                    actionAchievesGoal = true;
                    break;
                }
            }

            if (!actionAchievesGoal)
                continue;

            var subgoal = new Dictionary<string, object>();
            foreach (var precondition in action.Preconditions)
            {
                if (!worldState.TryGetValue(precondition.Key, out var value) ||
                    !value.Equals(precondition.Value))
                {
                    subgoal[precondition.Key] = precondition.Value;
                }
            }

            var reducedActions = new List<GOAPAction>(availableActions);
            reducedActions.Remove(action);

            var subplan = BuildPlan(agent, reducedActions, worldState, subgoal);

            if (subplan != null)
            {
                var planCost = subplan.Sum(a => a.Cost) + action.Cost;

                if (planCost < bestCost)
                {
                    bestPlan = [.. subplan, action];
                    bestCost = planCost;
                }
            }
        }

        return bestPlan.Count != 0 ? bestPlan : null;
    }
}
