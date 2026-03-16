# Issue 4: Add Overworld NPCs and Interactable Objects

## GitHub Issue Draft

- **Issue title:** Add overworld NPCs and interactable objects
- **Suggested labels:** `overworld`, `npc`, `interaction`, `priority-medium`
- **Depends on:** #3
- **Agent handoff:** This issue is scoped so one coding agent can implement it independently once dependencies are done. Preserve placeholder art and temporary data where the acceptance criteria explicitly allow it.

### Suggested starter files

- `project/hosts/complete-app/Scenes/Overworld/`
- `project/hosts/complete-app/Scripts/Overworld/`
- `project/hosts/complete-app/Scenes/UI/`

## Summary

Create an NPC system and interactable objects for the overworld. NPCs should stand on tiles with optional movement patterns and display dialogue when the player interacts with them. Interactable objects (signs, chests, doors) should respond to player interaction.

## Motivation

NPCs and interactable objects bring the overworld to life and provide gameplay hooks — quest givers, shops, lore, and environmental progression. This is a key part of the Ultima-style exploration experience.

## Acceptance Criteria

- [ ] An `NPC` scene is created with the following structure:
  ```
  NPC (CharacterBody2D)
  ├── Sprite2D          # NPC appearance
  ├── CollisionShape2D  # Blocks player movement
  └── InteractionArea (Area2D)
      └── CollisionShape2D  # Detects player proximity
  ```
- [ ] NPCs block movement (the player cannot walk through them).
- [ ] NPCs can have one of the following movement patterns:
  - `Stationary` — stays in place
  - `Patrol` — walks between 2-4 waypoints
  - `Random` — randomly moves to adjacent tiles periodically
- [ ] When the player faces an NPC and presses the interact key (`E` or `Enter`), a dialogue box appears.
- [ ] A simple dialogue system displays text line-by-line in a UI panel at the bottom of the screen.
- [ ] Dialogue data is stored in a resource file (e.g., JSON or Godot `.tres` resource), not hardcoded.
- [ ] An `Interactable` base class or interface is created that NPCs and objects implement.
- [ ] At least two types of interactable objects are implemented:
  - **Sign** — displays a text message when interacted with
  - **Chest** — opens and gives an item (placeholder item for now)
- [ ] At least 3 NPCs and 2 interactable objects are placed on the sample overworld map.

## Technical Details

### NPC.cs

```csharp
using Godot;

namespace UltimaMagic.Overworld;

public partial class Npc : CharacterBody2D
{
    [Export] public string NpcName { get; set; } = "Villager";
    [Export] public string[] DialogueLines { get; set; } = { "Hello, traveler!" };
    [Export] public MovementPattern Pattern { get; set; } = MovementPattern.Stationary;

    public enum MovementPattern
    {
        Stationary,
        Patrol,
        Random
    }
}
```

### IInteractable Interface

```csharp
namespace UltimaMagic.Overworld;

public interface IInteractable
{
    string InteractionPrompt { get; }
    void Interact(Player player);
}
```

### DialogueBox.cs (UI)

```csharp
using Godot;

namespace UltimaMagic.UI;

public partial class DialogueBox : Control
{
    [Export] public Label SpeakerLabel { get; set; }
    [Export] public Label TextLabel { get; set; }

    private string[] _lines;
    private int _currentLine;

    public void ShowDialogue(string speaker, string[] lines) { /* ... */ }
    public void AdvanceLine() { /* ... */ }
    public void Close() { /* ... */ }
}
```

### Dialogue Data (JSON)

```json
{
  "elder": {
    "name": "Village Elder",
    "lines": [
      "Welcome to our village, traveler.",
      "Beware the forests to the north — monsters lurk there.",
      "May the light guide your path."
    ]
  }
}
```

## Labels

`overworld`, `npc`, `interaction`, `priority-medium`
