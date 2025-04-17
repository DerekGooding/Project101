namespace Project1.Combat;
public class Enemy(string id, string name, int health, int attackPower, int defense, Point position, int experienceValue = 10)
{
    public string Id { get; } = id;
    public string Name { get; } = name;
    public int Health { get; private set; } = health;
    public int MaxHealth { get; } = health;
    public int AttackPower { get; } = attackPower;
    public int Defense { get; } = defense;
    public Point GridPosition { get; set; } = position;
    public int FacingDirection { get; set; } = 2; // Default facing south
    public int ExperienceValue { get; } = experienceValue;
    public bool IsAlive => Health > 0;
    public float AttackCooldown { get; } = 1.5f; // Seconds between attacks
    public float CurrentCooldown { get; private set; } = 0;

    public void TakeDamage(int amount)
    {
        var actualDamage = Math.Max(1, amount - Defense);
        Health = Math.Max(0, Health - actualDamage);
    }

    public int CalculateAttackDamage() => AttackPower;

    public void Update(GameTime gameTime)
    {
        if (CurrentCooldown > 0)
        {
            CurrentCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public bool CanAttack() => CurrentCooldown <= 0;

    public void ResetAttackCooldown() => CurrentCooldown = AttackCooldown;
}
