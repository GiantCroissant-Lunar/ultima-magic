using System;
using Godot;

namespace UltimaMagic.Data;

[GlobalClass]
public partial class EnemyGroup : Resource
{
    [Export]
    public string[] EnemyTypes { get; set; } = Array.Empty<string>();

    [Export]
    public int MinCount { get; set; } = 1;

    [Export]
    public int MaxCount { get; set; } = 3;

    [Export]
    public float Weight { get; set; } = 1.0f;
}
