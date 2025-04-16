using System.Collections.Generic;

namespace Project1.Dialogue;

// Manages multiple dialogue trees
public class Database
{
    private readonly Dictionary<string, Tree> _dialogueTrees = [];
    private readonly List<Trigger> _triggers = [];

    public void AddDialogueTree(string id, Tree tree) => _dialogueTrees[id] = tree;

    public Tree GetDialogueTree(string id) => _dialogueTrees.TryGetValue(id, out var tree) ? tree : null;

    public void AddTrigger(Trigger trigger) => _triggers.Add(trigger);

    public Trigger GetTriggerAtPosition(Point position) => _triggers.FirstOrDefault(t => t.Position == position);
}
