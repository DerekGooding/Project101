using System;

namespace Project1.Inventory;

public class Player
{
    public Inventory Inventory { get; } = new Inventory(20);
    public int Health { get; private set; } = 100;
    public int MaxHealth { get; } = 100;

    public void Heal(int amount) => Health = Math.Min(Health + amount, MaxHealth);

    public void TakeDamage(int amount) => Health = Math.Max(Health - amount, 0);
}
