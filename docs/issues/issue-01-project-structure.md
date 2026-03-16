# Issue 1: Set Up C# Project Structure and Solution

## Summary

Establish the foundational C# project structure for the Godot game. This includes creating the `.sln` and `.csproj` files, organizing the directory layout for scripts, scenes, and resources, and setting up autoload singletons that will be used across the project.

## Motivation

A well-organized project structure is essential before any game logic can be written. Godot 4.6 Mono projects need a properly configured C# solution so that scripts compile, autoloads register, and the build pipeline works end-to-end.

## Acceptance Criteria

- [ ] A `complete-app.sln` solution file exists at `project/hosts/complete-app/` and can be opened in an IDE (Rider, VS Code, Visual Studio).
- [ ] A `complete-app.csproj` file exists and targets `.NET 8.0` (or the version required by Godot 4.6 Mono).
- [ ] The following directory structure is created under `project/hosts/complete-app/`:
  ```
  Scripts/
  ├── Autoload/          # Singleton scripts (GameManager, SceneManager, etc.)
  ├── Overworld/         # Overworld-related scripts
  ├── Battle/            # Battle-related scripts
  ├── Data/              # Data models, enums, constants
  └── UI/                # UI-related scripts
  Scenes/
  ├── Overworld/         # Overworld scenes (.tscn)
  ├── Battle/            # Battle scenes (.tscn)
  └── UI/                # UI scenes (.tscn)
  Resources/
  ├── Tilesets/          # Tile set resources
  ├── Sprites/           # Sprite textures
  └── Audio/             # Sound effects and music
  ```
- [ ] A `GameManager.cs` autoload singleton is created with basic lifecycle methods (`_Ready`, `_Process`).
- [ ] A `SceneManager.cs` autoload singleton is created to handle scene transitions.
- [ ] Both autoloads are registered in `project.godot` under `[autoload]`.
- [ ] The project builds successfully with `dotnet build` from the `complete-app` directory.
- [ ] The NUKE build script in `build/nuke/` is updated to include a target that builds the Godot C# project.

## Technical Details

### GameManager.cs (Autoload)

```csharp
using Godot;

namespace UltimaMagic.Autoload;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

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
    }
}
```

### SceneManager.cs (Autoload)

```csharp
using Godot;

namespace UltimaMagic.Autoload;

public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void ChangeScene(string scenePath)
    {
        GetTree().ChangeSceneToFile(scenePath);
    }
}
```

### project.godot additions

```ini
[autoload]
GameManager="*res://Scripts/Autoload/GameManager.cs"
SceneManager="*res://Scripts/Autoload/SceneManager.cs"
```

## Labels

`setup`, `infrastructure`, `priority-critical`
