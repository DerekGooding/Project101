namespace Project1.Inventory.Items;

public class Key(string id, string name, string doorId) : Item(id, name, "Opens a locked door", ItemType.Key)
{
    public string DoorId { get; } = doorId;
}
