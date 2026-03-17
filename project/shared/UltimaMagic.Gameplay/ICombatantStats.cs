namespace UltimaMagic.Gameplay;

public interface ICombatantStats
{
    int EffectiveStrength { get; }

    int EffectiveDefense { get; }

    int EffectiveIntelligence { get; }

    int EffectiveLuck { get; }
}
