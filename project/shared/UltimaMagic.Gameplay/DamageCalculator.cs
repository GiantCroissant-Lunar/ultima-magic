namespace UltimaMagic.Gameplay;

public static class DamageCalculator
{
    /// <summary>
    /// Physical damage formula: damage = attacker.EffectiveStrength * attackPower - defender.EffectiveDefense / 2.
    /// Magical damage formula: damage = attacker.Intelligence * spellPower - defender.Intelligence * 3 / 10.
    /// Critical hits multiply the computed damage by 1.5, and all attacks deal at least 1 damage.
    /// </summary>
    public static int CalculatePhysicalDamage(ICombatantStats attacker, ICombatantStats defender, int attackPower)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        ArgumentNullException.ThrowIfNull(defender);

        var rawDamage = attacker.EffectiveStrength * attackPower - defender.EffectiveDefense / 2;
        return Math.Max(1, rawDamage);
    }

    public static int CalculateMagicalDamage(ICombatantStats attacker, ICombatantStats defender, int spellPower)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        ArgumentNullException.ThrowIfNull(defender);

        var rawDamage = attacker.Intelligence * spellPower - defender.Intelligence * 3 / 10;
        return Math.Max(1, rawDamage);
    }

    public static int ApplyCriticalHit(int damage) => Math.Max(1, (damage * 3 + 1) / 2);

    public static bool RollCritical(ICombatantStats attacker)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        return RollCritical(attacker, Random.Shared.NextSingle());
    }

    public static bool RollCritical(ICombatantStats attacker, float roll)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        return roll < Math.Clamp(attacker.Luck / 100.0f, 0.0f, 1.0f);
    }
}
