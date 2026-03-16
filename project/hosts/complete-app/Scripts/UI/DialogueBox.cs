using Godot;

namespace UltimaMagic.UI;

public partial class DialogueBox : Control
{
    public static DialogueBox? Instance { get; private set; }

    [Export]
    public Label? SpeakerLabel { get; set; }

    [Export]
    public Label? TextLabel { get; set; }

    private string[] _lines = [];
    private int _currentLine;

    public bool IsOpen => Visible;

    public override void _Ready()
    {
        Instance = this;
        SpeakerLabel ??= GetNodeOrNull<Label>("Panel/MarginContainer/VBoxContainer/SpeakerLabel");
        TextLabel ??= GetNodeOrNull<Label>("Panel/MarginContainer/VBoxContainer/TextLabel");
        if (SpeakerLabel == null || TextLabel == null)
        {
            GD.PushError("DialogueBox is missing its label references. Check DialogueBox.tscn node paths or exported references.");
        }

        Hide();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowDialogue(string speaker, string[] lines)
    {
        if (SpeakerLabel == null || TextLabel == null)
        {
            GD.PushError("DialogueBox cannot show dialogue because its label references are not configured.");
            return;
        }

        _lines = lines.Length > 0 ? lines : ["..."];
        _currentLine = 0;
        SpeakerLabel.Text = speaker;
        TextLabel.Text = _lines[_currentLine];
        Show();
    }

    public void AdvanceLine()
    {
        if (!IsOpen || TextLabel == null)
        {
            return;
        }

        _currentLine++;
        if (_currentLine >= _lines.Length)
        {
            Close();
            return;
        }

        TextLabel.Text = _lines[_currentLine];
    }

    public void Close()
    {
        _lines = [];
        _currentLine = 0;
        Hide();
    }
}
