# Issue 3: Implement Player Character and Movement on Overworld

## Summary

Create the player character for the overworld and implement grid-based (tile-aligned) movement controlled by keyboard input. The player should move one tile at a time in four directions, with smooth interpolated movement between tiles.

## Motivation

The player character is the core of the Ultima-style overworld experience. Grid-based movement gives the classic RPG feel while allowing precise tile interactions for encounters, NPC dialogue, and map navigation.

## Acceptance Criteria

- [ ] A `Player` scene is created with the following node structure:
  ```
  Player (CharacterBody2D)
  ├── Sprite2D          # Player sprite (placeholder)
  ├── CollisionShape2D  # For physics interactions
  ├── AnimationPlayer   # For walk animations (optional at this stage)
  └── RayCast2D         # For detecting obstacles in movement direction
  ```
- [ ] A placeholder player sprite is created (a simple colored character, 16×16 or 32×32 pixels to match tile size).
- [ ] The player moves in 4 directions using arrow keys or WASD.
- [ ] Movement is **grid-based**: the player snaps to tile positions.
- [ ] Movement between tiles is **smoothly interpolated** (tweened) so it does not look jerky.
- [ ] The player **cannot walk through** non-walkable tiles (water, mountains) — collision detection via `RayCast2D` or tilemap collision.
- [ ] A `Camera2D` follows the player smoothly with optional edge smoothing.
- [ ] The player's position is tracked in tile coordinates (e.g., `Vector2I(5, 10)`).
- [ ] A `Player.cs` script manages movement state and exposes:
  - `TilePosition` — current tile coordinates
  - `IsMoving` — whether the player is currently in a movement tween
  - `StepsTaken` — counter for steps (used later by encounter system)
- [ ] Movement input is ignored while the player is already moving (no input buffering needed yet).

## Technical Details

### Player.cs

```csharp
using Godot;

namespace UltimaMagic.Overworld;

public partial class Player : CharacterBody2D
{
    [Export] public int TileSize { get; set; } = 32;
    [Export] public float MoveSpeed { get; set; } = 4.0f; // tiles per second

    public Vector2I TilePosition { get; private set; }
    public bool IsMoving { get; private set; }
    public int StepsTaken { get; private set; }

    private Tween _moveTween;

    public override void _Ready()
    {
        TilePosition = new Vector2I(
            (int)(Position.X / TileSize),
            (int)(Position.Y / TileSize)
        );
    }

    public override void _Process(double delta)
    {
        if (IsMoving) return;

        var direction = Vector2I.Zero;
        if (Input.IsActionPressed("move_up"))    direction = Vector2I.Up;
        if (Input.IsActionPressed("move_down"))  direction = Vector2I.Down;
        if (Input.IsActionPressed("move_left"))  direction = Vector2I.Left;
        if (Input.IsActionPressed("move_right")) direction = Vector2I.Right;

        if (direction != Vector2I.Zero)
            TryMove(direction);
    }

    private void TryMove(Vector2I direction)
    {
        // Use RayCast2D to check if the target tile is walkable
        // If walkable, start tween to move smoothly
    }
}
```

### Input Map

Add the following input actions in `project.godot`:

```ini
[input]
move_up={ "events": [KeyEvent(Key.W), KeyEvent(Key.Up)] }
move_down={ "events": [KeyEvent(Key.S), KeyEvent(Key.Down)] }
move_left={ "events": [KeyEvent(Key.A), KeyEvent(Key.Left)] }
move_right={ "events": [KeyEvent(Key.D), KeyEvent(Key.Right)] }
interact={ "events": [KeyEvent(Key.E), KeyEvent(Key.Enter)] }
```

### Movement Tween

```csharp
private void MoveTo(Vector2I targetTile)
{
    IsMoving = true;
    TilePosition = targetTile;
    var targetPos = new Vector2(targetTile.X * TileSize, targetTile.Y * TileSize);

    _moveTween = CreateTween();
    _moveTween.TweenProperty(this, "position", targetPos, 1.0 / MoveSpeed);
    _moveTween.Finished += () =>
    {
        IsMoving = false;
        StepsTaken++;
    };
}
```

## Labels

`overworld`, `player`, `movement`, `priority-critical`
