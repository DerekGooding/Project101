namespace Project1.Dialogue;

public class Line(string text, string speaker = "")
{
    public string Text { get; } = text;
    public string Speaker { get; } = speaker;
    public List<Option> Options { get; } = [];

    public Line AddOption(string text, string nextDialogueId)
    {
        Options.Add(new Option(text, nextDialogueId));
        return this;
    }
}
