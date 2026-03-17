using Godot;
using UltimaMagic.Data;
using GameplayDamageCalculator = UltimaMagic.Gameplay.DamageCalculator;

namespace UltimaMagic.Battle;

public static class DamageCalculator
{
    public static int CalculatePhysicalDamage(CharacterStats attacker, CharacterStats defender, int attackPower)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        ArgumentNullException.ThrowIfNull(defender);
        return GameplayDamageCalculator.CalculatePhysicalDamage(attacker, defender, attackPower);
    }

    public static int CalculateMagicalDamage(CharacterStats attacker, CharacterStats defender, int spellPower)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        ArgumentNullException.ThrowIfNull(defender);
        return GameplayDamageCalculator.CalculateMagicalDamage(attacker, defender, spellPower);
    }

    public static int ApplyCriticalHit(int damage) => GameplayDamageCalculator.ApplyCriticalHit(damage);

    public static bool RollCritical(CharacterStats attacker)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        return GD.Randf() < Mathf.Clamp(attacker.EffectiveLuck / 100.0f, 0.0f, 1.0f);
    }

    public static bool RollCritical(CharacterStats attacker, float roll)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        return GameplayDamageCalculator.RollCritical(attacker, roll);
    }
}
