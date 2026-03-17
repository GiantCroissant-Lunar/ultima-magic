namespace UltimaMagic.Gameplay;

public static class LevelTable
{
    public static int ExperienceForLevel(int level) => level switch
    {
        <= 1 => 0,
        2 => 100,
        3 => 300,
        4 => 600,
        5 => 1000,
        _ => 1000 + (level - 5) * 500
    };
}
