namespace UltimaMagic.Gameplay;

public static class LevelingSystem
{
    public static int ApplyLevelUps(CharacterProgressionState state, LevelGrowth growth)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(growth);

        var levelsGained = 0;
        while (state.Experience >= LevelTable.ExperienceForLevel(state.Level + 1))
        {
            state.Level++;
            state.MaxHp += growth.MaxHp;
            state.Hp = Math.Min(state.MaxHp, state.Hp + growth.MaxHp);
            state.MaxMp += growth.MaxMp;
            state.Mp = Math.Min(state.MaxMp, state.Mp + growth.MaxMp);
            state.Strength += growth.Strength;
            state.Defense += growth.Defense;
            state.Intelligence += growth.Intelligence;
            state.Agility += growth.Agility;
            state.Luck += growth.Luck;
            levelsGained++;
        }

        return levelsGained;
    }
}
