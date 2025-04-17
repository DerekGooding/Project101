using System.Collections.Generic;

namespace Project1.Dialogue;

// Manages multiple dialogue trees
public class Database
{
    private readonly Dictionary<string, DialogueTree> _dialogueTrees = [];
    private readonly List<Trigger> _triggers = [];

    public void AddDialogueTree(string id, DialogueTree tree) => _dialogueTrees[id] = tree;

    public DialogueTree GetDialogueTree(string id) => _dialogueTrees.TryGetValue(id, out var tree) ? tree : null;

    public void AddTrigger(Trigger trigger) => _triggers.Add(trigger);

    public Trigger GetTriggerAtPosition(Point position) => _triggers.FirstOrDefault(t => t.Position == position);
}
