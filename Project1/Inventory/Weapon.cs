namespace Project1.Inventory;

public class Weapon(string id, string name, string description, int attackValue, float attackSpeed = 1.0f) : Item(id, name, description, ItemType.Weapon)
{
    public int AttackValue { get; } = attackValue;
    public float AttackSpeed { get; } = attackSpeed;

    public override void Use(Player player) => player.EquipWeapon(this);
}
