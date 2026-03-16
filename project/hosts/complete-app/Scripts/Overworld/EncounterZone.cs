using Godot;
using UltimaMagic.Data;

namespace UltimaMagic.Overworld;

[GlobalClass]
public partial class EncounterZone : Node
{
    [Export]
    public string ZoneName { get; set; } = "Unknown";

    [Export]
    public float BaseEncounterRate { get; set; } = 0.05f;

    [Export]
    public int Priority { get; set; }

    [Export]
    public Rect2I TileRegion { get; set; } = new(0, 0, 1, 1);

    [Export]
    public EncounterData? EncounterData { get; set; }

    public bool Contains(Vector2I tilePosition)
    {
        return TileRegion.HasPoint(tilePosition);
    }
}
