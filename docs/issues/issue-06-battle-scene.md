# Issue 6: Create First-Person Battle Scene

## Summary

Build the Might & Magic-style first-person battle scene. This is a 3D scene where the player views enemies from a first-person perspective. Enemies are displayed as 2D sprites (billboards) in a 3D environment, similar to classic Might & Magic games.

## Motivation

The first-person battle view is the second core pillar of the game. When the player encounters enemies on the overworld, they are transported into this view to fight. The classic Might & Magic aesthetic uses 2D enemy sprites rendered in a 3D perspective space.

## Acceptance Criteria

- [ ] A `BattleScene.tscn` 3D scene is created under `Scenes/Battle/`.
- [ ] The scene contains a `Camera3D` positioned at the player's viewpoint (first-person perspective).
- [ ] A battle background/environment is rendered:
  - A simple ground plane (textured or colored)
  - A sky or background (can be a `WorldEnvironment` with a solid color or gradient)
  - Optional: simple walls or environmental props to give depth
- [ ] Enemy display positions are defined — up to 4 enemy slots arranged in a row facing the player.
- [ ] Each enemy slot uses a `Sprite3D` node that:
  - Always faces the camera (billboard mode)
  - Displays the enemy's sprite texture
  - Scales appropriately based on distance from camera
- [ ] A placeholder enemy sprite is created (a simple colored shape, 64×64 or 128×128 pixels).
- [ ] The battle scene can display 1-4 enemies at configurable positions.
- [ ] A `BattleScene.cs` script manages:
  - Loading enemy data and sprites into the enemy slots
  - Highlighting the currently targeted enemy (e.g., a pulsing outline or color shift)
  - Enemy death animation (fade out or fall over)
- [ ] The scene is visually distinct from the overworld (different lighting, perspective).
- [ ] The scene loads and renders correctly when run standalone in the Godot editor.

## Technical Details

### Scene Structure

```
BattleScene (Node3D)
├── WorldEnvironment
├── DirectionalLight3D
├── Camera3D                    # First-person viewpoint
├── GroundPlane (MeshInstance3D) # Ground beneath enemies
├── EnemySlots (Node3D)
│   ├── EnemySlot1 (Sprite3D)  # Billboard sprite
│   ├── EnemySlot2 (Sprite3D)
│   ├── EnemySlot3 (Sprite3D)
│   └── EnemySlot4 (Sprite3D)
└── BattleUI (CanvasLayer)      # 2D UI overlay (implemented in Issue #9)
```

### BattleScene.cs

```csharp
using Godot;

namespace UltimaMagic.Battle;

public partial class BattleScene : Node3D
{
    [Export] public Sprite3D[] EnemySlots { get; set; }

    private int _targetedEnemyIndex;

    public override void _Ready()
    {
        // Initialize enemy slots, hide unused ones
    }

    public void LoadEnemies(EnemyBattleData[] enemies)
    {
        for (int i = 0; i < EnemySlots.Length; i++)
        {
            if (i < enemies.Length)
            {
                EnemySlots[i].Texture = enemies[i].Sprite;
                EnemySlots[i].Visible = true;
            }
            else
            {
                EnemySlots[i].Visible = false;
            }
        }
    }

    public void TargetEnemy(int index)
    {
        // Highlight the targeted enemy
        _targetedEnemyIndex = index;
    }

    public void OnEnemyDefeated(int index)
    {
        // Play death animation on the enemy sprite
    }
}
```

### Camera Setup

- **Position:** `Vector3(0, 1.6, 0)` — roughly eye height
- **Rotation:** Looking forward along -Z axis
- **FOV:** ~70° for a classic feel

### Enemy Slot Positions

Arrange enemies in an arc facing the player:

```
Slot 0: Vector3(-2.0, 0.5, -4.0)  — far left
Slot 1: Vector3(-0.7, 0.5, -3.5)  — center left
Slot 2: Vector3( 0.7, 0.5, -3.5)  — center right
Slot 3: Vector3( 2.0, 0.5, -4.0)  — far right
```

### Billboard Sprite3D Settings

- `Billboard` mode: `Billboard.Enabled` (always faces camera)
- `Shaded`: `false` (keep the 2D sprite look)
- `Double Sided`: `true`
- `Alpha Cut`: `Discard` (for transparency)

## Labels

`battle`, `3d`, `scene`, `priority-critical`
