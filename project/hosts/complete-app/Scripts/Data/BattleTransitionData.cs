namespace UltimaMagic.Data;

public sealed class BattleTransitionData
{
    public EncounterResult Encounter { get; set; } = new();

    public PlayerStateSnapshot PlayerState { get; set; } = new();
}
