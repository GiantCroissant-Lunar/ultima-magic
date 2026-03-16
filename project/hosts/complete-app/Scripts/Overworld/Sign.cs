using Godot;
using UltimaMagic.Data;
using UltimaMagic.UI;

namespace UltimaMagic.Overworld;

public partial class Sign : StaticBody2D, IInteractable
{
    [Export]
    public string DisplayName { get; set; } = "Sign";

    [Export]
    public string DialogueId { get; set; } = string.Empty;

    [Export]
    public Color SpriteTint { get; set; } = new(0.72f, 0.47f, 0.2f, 1.0f);

    public string InteractionPrompt => "Read";

    public override void _Ready()
    {
        GetNode<Sprite2D>("Sprite2D").Modulate = SpriteTint;
        AddToGroup(OverworldGrid.InteractableGroup);
        AddToGroup(OverworldGrid.TileBlockerGroup);
    }

    public void Interact(Player player)
    {
        var entry = DialogueDatabase.GetEntry(DialogueId);
        DialogueBox.Instance?.ShowDialogue(string.IsNullOrWhiteSpace(entry.Name) ? DisplayName : entry.Name, entry.Lines);
    }
}
