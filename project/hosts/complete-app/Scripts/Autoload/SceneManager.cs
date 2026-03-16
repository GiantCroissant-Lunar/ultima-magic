using System;
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
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            throw new ArgumentException("Scene path must not be null or empty.", nameof(scenePath));
        }

        if (!ResourceLoader.Exists(scenePath))
        {
            throw new ArgumentException($"Scene path does not exist: {scenePath}", nameof(scenePath));
        }

        GetTree().ChangeSceneToFile(scenePath);
    }
}
