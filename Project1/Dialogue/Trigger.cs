namespace Project1.Dialogue;

// Dialogue trigger in the dungeon
public class Trigger(Point position, string dialogueTreeId, string startDialogueId)
{
    public Point Position { get; } = position;
    public string DialogueTreeId { get; } = dialogueTreeId;
    public string StartDialogueId { get; } = startDialogueId;
}
