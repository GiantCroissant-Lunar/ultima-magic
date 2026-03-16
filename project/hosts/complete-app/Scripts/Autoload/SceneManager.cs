using Godot;

namespace UltimaMagic.Autoload;

public partial class SceneManager : Node
{
    public static SceneManager? Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void ChangeScene(string scenePath)
    {
        GetTree().ChangeSceneToFile(scenePath);
    }
}
