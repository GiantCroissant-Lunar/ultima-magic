namespace UltimaMagic.Gameplay;

public interface ICombatantStats
{
    int EffectiveStrength { get; }

    int EffectiveDefense { get; }

    int Intelligence { get; }

    int Luck { get; }
}
