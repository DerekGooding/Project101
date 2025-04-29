namespace Project3.Goap;

public class Home(Vector2 position)
{
    public Vector2 Position { get; set; } = position;
    public int StoredResources { get; set; }
    public int Level { get; set; } = 1;
    public int StorageCapacity { get; set; } = 100;
    public float RestEfficiency { get; set; } = 1.0f;
}
