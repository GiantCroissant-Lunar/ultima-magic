using System;
using Godot;

namespace UltimaMagic.Data;

public sealed class BattleResult
{
    public bool PlayerWon { get; set; }

    public int ExperienceGained { get; set; }

    public string[] ItemsGained { get; set; } = Array.Empty<string>();

    public int RemainingHp { get; set; }

    public int RemainingMp { get; set; }

    public Vector2I PlayerReturnPosition { get; set; } = Vector2I.Zero;
}
