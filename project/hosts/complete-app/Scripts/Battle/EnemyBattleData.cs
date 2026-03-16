using Godot;

namespace UltimaMagic.Battle;

public sealed class EnemyBattleData
{
    public string Name { get; set; } = "Enemy";

    public Texture2D? Sprite { get; set; }

    public Color Tint { get; set; } = Colors.White;
}
