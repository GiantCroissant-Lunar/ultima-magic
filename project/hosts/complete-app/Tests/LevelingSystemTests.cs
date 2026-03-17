using UltimaMagic.Gameplay;

namespace UltimaMagic.Tests;

public sealed class LevelingSystemTests
{
    [Fact]
    public void ApplyLevelUps_IncreasesLevelAndStatsAtThresholds()
    {
        var state = new CharacterProgressionState
        {
            Level = 1,
            Experience = 300,
            Hp = 100,
            MaxHp = 100,
            Mp = 30,
            MaxMp = 30,
            Strength = 10,
            Defense = 8,
            Intelligence = 6,
            Agility = 7,
            Luck = 5
        };

        var growth = new LevelGrowth
        {
            MaxHp = 12,
            MaxMp = 4,
            Strength = 2,
            Defense = 1,
            Intelligence = 1,
            Agility = 1,
            Luck = 1
        };

        var levelsGained = LevelingSystem.ApplyLevelUps(state, growth);

        Assert.Equal(2, levelsGained);
        Assert.Equal(3, state.Level);
        Assert.Equal(124, state.Hp);
        Assert.Equal(124, state.MaxHp);
        Assert.Equal(38, state.Mp);
        Assert.Equal(38, state.MaxMp);
        Assert.Equal(14, state.Strength);
        Assert.Equal(10, state.Defense);
        Assert.Equal(8, state.Intelligence);
        Assert.Equal(9, state.Agility);
        Assert.Equal(7, state.Luck);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 100)]
    [InlineData(3, 300)]
    [InlineData(4, 600)]
    [InlineData(5, 1000)]
    [InlineData(6, 1500)]
    public void ExperienceForLevel_UsesConfiguredThresholds(int level, int expectedExperience)
    {
        Assert.Equal(expectedExperience, LevelTable.ExperienceForLevel(level));
    }
}
