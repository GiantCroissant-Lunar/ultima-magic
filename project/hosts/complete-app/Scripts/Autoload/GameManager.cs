using Godot;

namespace UltimaMagic.Autoload;

public partial class GameManager : Node
{
    public static GameManager? Instance { get; private set; }

    public enum GameState
    {
        Overworld,
        Battle,
        Menu,
        Cutscene
    }

    public GameState CurrentState { get; set; } = GameState.Overworld;

    public override void _Ready()
    {
        Instance = this;
        SetProcess(false);
    }

    public override void _Process(double delta)
    {
    }
}
