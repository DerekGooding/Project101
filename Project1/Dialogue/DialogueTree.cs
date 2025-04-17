using System.Collections.Generic;

namespace Project1.Dialogue;

// A collection of related dialogue lines
public class DialogueTree
{
    private readonly Dictionary<string, Line> _dialogues = [];

    public Line AddDialogue(string id, string text, string speaker = "")
    {
        var dialogue = new Line(text, speaker);
        _dialogues[id] = dialogue;
        return dialogue;
    }

    public Line GetDialogue(string id) => _dialogues.TryGetValue(id, out var dialogue) ? dialogue : null;
}
