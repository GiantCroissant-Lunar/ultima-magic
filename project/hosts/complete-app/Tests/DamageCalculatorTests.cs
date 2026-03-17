using UltimaMagic.Gameplay;

namespace UltimaMagic.Tests;

public sealed class DamageCalculatorTests
{
    [Fact]
    public void CalculatePhysicalDamage_UsesEffectiveStatsFormula()
    {
        var attacker = new TestCombatant { EffectiveStrength = 12, EffectiveDefense = 4, EffectiveIntelligence = 8, EffectiveLuck = 5 };
        var defender = new TestCombatant { EffectiveStrength = 6, EffectiveDefense = 10, EffectiveIntelligence = 3, EffectiveLuck = 2 };

        var damage = DamageCalculator.CalculatePhysicalDamage(attacker, defender, 3);

        Assert.Equal(31, damage);
    }

    [Fact]
    public void CalculateMagicalDamage_UsesIntelligenceFormula()
    {
        var attacker = new TestCombatant { EffectiveStrength = 8, EffectiveDefense = 4, EffectiveIntelligence = 9, EffectiveLuck = 5 };
        var defender = new TestCombatant { EffectiveStrength = 6, EffectiveDefense = 7, EffectiveIntelligence = 10, EffectiveLuck = 2 };

        var damage = DamageCalculator.CalculateMagicalDamage(attacker, defender, 4);

        Assert.Equal(33, damage);
    }

    [Fact]
    public void CalculateDamage_NeverReturnsLessThanOne()
    {
        var attacker = new TestCombatant { EffectiveStrength = 1, EffectiveDefense = 1, EffectiveIntelligence = 1, EffectiveLuck = 0 };
        var defender = new TestCombatant { EffectiveStrength = 1, EffectiveDefense = 50, EffectiveIntelligence = 50, EffectiveLuck = 0 };

        Assert.Equal(1, DamageCalculator.CalculatePhysicalDamage(attacker, defender, 1));
        Assert.Equal(1, DamageCalculator.CalculateMagicalDamage(attacker, defender, 1));
    }

    [Fact]
    public void RollCritical_UsesLuckAsPercentChance()
    {
        var attacker = new TestCombatant { EffectiveStrength = 10, EffectiveDefense = 8, EffectiveIntelligence = 6, EffectiveLuck = 15 };

        Assert.True(DamageCalculator.RollCritical(attacker, 0.10f));
        Assert.False(DamageCalculator.RollCritical(attacker, 0.15f));
    }

    [Fact]
    public void ApplyCriticalHit_MultipliesDamageByOnePointFive()
    {
        Assert.Equal(15, DamageCalculator.ApplyCriticalHit(10));
    }

    private sealed class TestCombatant : ICombatantStats
    {
        public int EffectiveStrength { get; init; }

        public int EffectiveDefense { get; init; }

        public int EffectiveIntelligence { get; init; }

        public int EffectiveLuck { get; init; }
    }
}
