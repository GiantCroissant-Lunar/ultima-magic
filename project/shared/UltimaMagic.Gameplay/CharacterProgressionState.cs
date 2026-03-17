namespace UltimaMagic.Gameplay;

public sealed class CharacterProgressionState
{
    public int Level { get; set; } = 1;

    public int Experience { get; set; }

    public int Hp { get; set; } = 100;

    public int MaxHp { get; set; } = 100;

    public int Mp { get; set; } = 30;

    public int MaxMp { get; set; } = 30;

    public int Strength { get; set; } = 10;

    public int Defense { get; set; } = 8;

    public int Intelligence { get; set; } = 6;

    public int Agility { get; set; } = 7;

    public int Luck { get; set; } = 5;
}
