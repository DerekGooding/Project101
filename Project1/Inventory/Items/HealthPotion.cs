namespace Project1.Inventory.Items;

public class HealthPotion(string id, string name, int healAmount)
    : Item(id, name, $"Restores {healAmount} health", ItemType.Potion, true, 5)
{
    private readonly int _healAmount = healAmount;

    public override void Use(Player player) => player.Heal(_healAmount);
}