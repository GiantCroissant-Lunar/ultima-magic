using Godot;
using UltimaMagic.Gameplay;

namespace UltimaMagic.Data;

public enum ItemType
{
    Consumable,
    Equipment,
    KeyItem
}

[GlobalClass]
public partial class Item : Resource, IInventoryItem
{
    [Export]
    public string Name { get; set; } = string.Empty;

    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = string.Empty;

    [Export]
    public Texture2D? Icon { get; set; }

    [Export]
    public ItemType ItemType { get; set; }
}
