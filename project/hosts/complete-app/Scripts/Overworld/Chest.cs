using Godot;
using UltimaMagic.Data;
using UltimaMagic.UI;

namespace UltimaMagic.Overworld;

public partial class Chest : StaticBody2D, IInteractable
{
    private Sprite2D _sprite = null!;
    private bool _isOpened;

    [Export]
    public string DisplayName { get; set; } = "Chest";

    [Export]
    public string ClosedDialogueId { get; set; } = string.Empty;

    [Export]
    public string OpenedDialogueId { get; set; } = string.Empty;

    [Export]
    public string ItemName { get; set; } = "Healing Herb";

    [Export]
    public Color ClosedTint { get; set; } = new(0.92f, 0.74f, 0.22f, 1.0f);

    [Export]
    public Color OpenedTint { get; set; } = new(0.52f, 0.52f, 0.52f, 1.0f);

    public string InteractionPrompt => _isOpened ? "Inspect" : "Open";

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        AddToGroup(OverworldGrid.InteractableGroup);
        AddToGroup(OverworldGrid.TileBlockerGroup);
        UpdateVisual();
    }

    public void Interact(Player player)
    {
        if (!_isOpened)
        {
            _isOpened = true;
            UpdateVisual();
            ShowDialogue(ClosedDialogueId, true);
            return;
        }

        ShowDialogue(OpenedDialogueId, false);
    }

    private void ShowDialogue(string dialogueId, bool includeRewardText)
    {
        var entry = DialogueDatabase.GetEntry(dialogueId);
        var lines = includeRewardText
            ? AddRewardLine(entry.Lines)
            : entry.Lines;

        DialogueBox.Instance?.ShowDialogue(string.IsNullOrWhiteSpace(entry.Name) ? DisplayName : entry.Name, lines);
    }

    private void UpdateVisual()
    {
        _sprite.Modulate = _isOpened ? OpenedTint : ClosedTint;
    }

    private string[] AddRewardLine(string[] lines)
    {
        var rewardLine = $"You received {ItemName}.";
        var rewardLines = new string[lines.Length + 1];
        lines.CopyTo(rewardLines, 0);
        rewardLines[^1] = rewardLine;
        return rewardLines;
    }
}
