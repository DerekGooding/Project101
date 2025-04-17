namespace Project2.Additions;

public class Addition(string name, float[] damageMultipliers, float[] hitTimings)
{
    public string Name { get; } = name;
    public float[] DamageMultipliers { get; } = damageMultipliers;
    public float[] HitTimings { get; } = hitTimings;
}
