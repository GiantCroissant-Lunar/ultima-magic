using Godot;

namespace UltimaMagic.Data;

public sealed class PlayerStateSnapshot
{
    public int CurrentHp { get; set; } = 30;

    public int CurrentMp { get; set; } = 10;

    public Vector2I ReturnPosition { get; set; } = Vector2I.Zero;
}
