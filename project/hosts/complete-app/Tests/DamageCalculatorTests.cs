using UltimaMagic.Gameplay;

namespace UltimaMagic.Tests;

public sealed class DamageCalculatorTests
{
    [Fact]
    public void CalculatePhysicalDamage_UsesEffectiveStatsFormula()
    {
        var attacker = new TestCombatant { EffectiveStrength = 12, EffectiveDefense = 4, Intelligence = 8, Luck = 5 };
        var defender = new TestCombatant { EffectiveStrength = 6, EffectiveDefense = 10, Intelligence = 3, Luck = 2 };

        var damage = DamageCalculator.CalculatePhysicalDamage(attacker, defender, 3);

        Assert.Equal(31, damage);
    }

    [Fact]
    public void CalculateMagicalDamage_UsesIntelligenceFormula()
    {
        var attacker = new TestCombatant { EffectiveStrength = 8, EffectiveDefense = 4, Intelligence = 9, Luck = 5 };
        var defender = new TestCombatant { EffectiveStrength = 6, EffectiveDefense = 7, Intelligence = 10, Luck = 2 };

        var damage = DamageCalculator.CalculateMagicalDamage(attacker, defender, 4);

        Assert.Equal(33, damage);
    }

    [Fact]
    public void CalculateDamage_NeverReturnsLessThanOne()
    {
        var attacker = new TestCombatant { EffectiveStrength = 1, EffectiveDefense = 1, Intelligence = 1, Luck = 0 };
        var defender = new TestCombatant { EffectiveStrength = 1, EffectiveDefense = 50, Intelligence = 50, Luck = 0 };

        Assert.Equal(1, DamageCalculator.CalculatePhysicalDamage(attacker, defender, 1));
        Assert.Equal(1, DamageCalculator.CalculateMagicalDamage(attacker, defender, 1));
    }

    [Fact]
    public void RollCritical_UsesLuckAsPercentChance()
    {
        var attacker = new TestCombatant { EffectiveStrength = 10, EffectiveDefense = 8, Intelligence = 6, Luck = 15 };

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

        public int Intelligence { get; init; }

        public int Luck { get; init; }
    }
}
