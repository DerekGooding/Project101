using Project1.Combat;
using Project1.Dungeon;
using System;

namespace Project1.Inventory;

public class Player(Controller controller)
{
    public Inventory Inventory { get; } = new Inventory(20);
    public int Health { get; private set; } = 100;
    public int MaxHealth { get; private set; } = 100;
    public int AttackPower { get; private set; } = 10;
    public int Defense { get; private set; } = 5;
    public int Experience { get; private set; } = 0;
    public int Level { get; private set; } = 1;
    public Point GridPosition { get; set; } = new Point(1, 1);
    public float AttackCooldown { get; } = 0.5f;
    public float CurrentCooldown { get; private set; } = 0;
    private Controller _controller = controller;

    // Add event for when player levels up
    public event Action<int> LevelUp;

    public void Heal(int amount) => Health = Math.Min(Health + amount, MaxHealth);

    public void TakeDamage(int amount)
    {
        var actualDamage = Math.Max(1, amount - Defense);
        Health = Math.Max(Health - actualDamage, 0);
    }

    public int CalculateAttackDamage()
    {
        // Get equipped weapon (if any)
        var equippedWeapon = Inventory.GetEquippedWeapon();
        var weaponBonus = equippedWeapon?.AttackValue ?? 0;

        // Base attack + weapon bonus
        return AttackPower + weaponBonus;
    }

    public void GainExperience(int amount)
    {
        Experience += amount;

        // Simple leveling formula - check if player should level up
        var expNeeded = Level * 100; // 100 exp for level 1, 200 for level 2, etc.

        if (Experience >= expNeeded)
        {
            Level++;
            // Increase stats with level
            MaxHealth += 10;
            Health += 10;
            AttackPower += 2;
            Defense += 1;

            // Notify subscribers about level up
            LevelUp?.Invoke(Level);
        }
    }

    public void Update(GameTime gameTime)
    {
        // Update attack cooldown
        if (CurrentCooldown > 0)
        {
            CurrentCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public bool CanAttack() => CurrentCooldown <= 0;

    public void ResetAttackCooldown() => CurrentCooldown = AttackCooldown;

    public void Attack(Enemy enemy)
    {
        if (CanAttack())
        {
            int damage = CalculateAttackDamage();
            enemy.TakeDamage(damage);
            ResetAttackCooldown();
        }
    }

    public void EquipWeapon(Weapon weapon) => Inventory.EquipWeapon(weapon);

    public Point FacingOffset => _controller.FacingOffset();
}
