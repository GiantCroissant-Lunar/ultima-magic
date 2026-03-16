# Issue 7: Implement Scene Transition Between Overworld and Battle

## Summary

Create the system that transitions the game between the 2D overworld and the 3D first-person battle scene. This includes a visual transition effect (fade to black), passing encounter data from the overworld to the battle scene, and returning to the overworld after the battle ends.

## Motivation

Smooth scene transitions are critical for player experience. The shift from top-down 2D to first-person 3D is a defining feature of this game, and the transition needs to feel intentional and polished rather than jarring.

## Acceptance Criteria

- [ ] The `SceneManager` (from Issue #1) is extended with battle transition methods.
- [ ] A **fade-to-black** transition effect is implemented:
  1. Screen fades to black (0.5s)
  2. Scene switches from Overworld to BattleScene (or vice versa)
  3. Screen fades in from black (0.5s)
- [ ] The transition effect uses a `CanvasLayer` with a `ColorRect` that is always on top.
- [ ] `TransitionData` is passed from the overworld to the battle scene containing:
  - Enemy group data (types, count)
  - Player state (HP, MP, position to return to)
  - Terrain/environment context (for battle background selection)
- [ ] After battle completion, the game transitions back to the overworld:
  - Player is placed at their pre-battle tile position
  - Player state is updated with post-battle results (HP/MP changes, items gained)
  - The encounter counter resets (guaranteed safe steps after battle)
- [ ] The `GameManager.CurrentState` is updated during transitions:
  - `Overworld` → `Battle` when encounter triggers
  - `Battle` → `Overworld` when battle ends
- [ ] Input is disabled during the transition to prevent movement/actions.
- [ ] The transition works correctly in both directions (overworld→battle and battle→overworld).

## Technical Details

### SceneManager.cs Updates

```csharp
using Godot;

namespace UltimaMagic.Autoload;

public partial class SceneManager : Node
{
    private ColorRect _fadeRect;
    private AnimationPlayer _transitionPlayer;

    public override void _Ready()
    {
        Instance = this;
        SetupTransitionOverlay();
    }

    private void SetupTransitionOverlay()
    {
        var canvas = new CanvasLayer { Layer = 100 };
        _fadeRect = new ColorRect
        {
            Color = new Color(0, 0, 0, 0),
            AnchorRight = 1,
            AnchorBottom = 1,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        canvas.AddChild(_fadeRect);
        AddChild(canvas);
    }

    public async void TransitionToBattle(EncounterResult encounterData)
    {
        GameManager.Instance.CurrentState = GameManager.GameState.Battle;

        // Fade out
        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 1.0f, 0.5f);
        await ToSignal(tween, Tween.SignalName.Finished);

        // Store overworld state
        StoreOverworldState();

        // Change scene
        GetTree().ChangeSceneToFile("res://Scenes/Battle/BattleScene.tscn");

        // Wait a frame for the scene to load
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // Pass encounter data to battle scene
        var battleScene = GetTree().CurrentScene as BattleScene;
        battleScene?.Initialize(encounterData);

        // Fade in
        tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 0.0f, 0.5f);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    public async void TransitionToOverworld(BattleResult battleResult)
    {
        // Fade out
        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 1.0f, 0.5f);
        await ToSignal(tween, Tween.SignalName.Finished);

        // Change scene
        GetTree().ChangeSceneToFile("res://Scenes/Overworld/Overworld.tscn");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // Restore overworld state
        RestoreOverworldState(battleResult);

        GameManager.Instance.CurrentState = GameManager.GameState.Overworld;

        // Fade in
        tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 0.0f, 0.5f);
    }
}
```

### TransitionData Classes

```csharp
namespace UltimaMagic.Data;

public class EncounterResult
{
    public string[] EnemyTypes { get; set; }
    public int EnemyCount { get; set; }
    public string TerrainType { get; set; }  // For background selection
    public Vector2I PlayerReturnPosition { get; set; }
}

public class BattleResult
{
    public bool PlayerWon { get; set; }
    public int ExperienceGained { get; set; }
    public string[] ItemsGained { get; set; }
    public int RemainingHp { get; set; }
    public int RemainingMp { get; set; }
}
```

## Labels

`scene-transition`, `overworld`, `battle`, `priority-critical`
