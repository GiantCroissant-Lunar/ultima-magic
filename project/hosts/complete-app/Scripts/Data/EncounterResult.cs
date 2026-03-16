using System;
using Godot;

namespace UltimaMagic.Data;

[GlobalClass]
public partial class EncounterResult : Resource
{
    [Export]
    public string ZoneName { get; set; } = "Unknown";

    [Export]
    public string TerrainType { get; set; } = "unknown";

    [Export]
    public string[] EnemyTypes { get; set; } = Array.Empty<string>();

    [Export]
    public int EnemyCount { get; set; }

    [Export]
    public Vector2I PlayerReturnPosition { get; set; } = Vector2I.Zero;
}
