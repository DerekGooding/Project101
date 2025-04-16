namespace Project1.Dialogue;

public class Option(string text, string nextDialogueId)
{
    public string Text { get; } = text;
    public string NextDialogueId { get; } = nextDialogueId;
}
