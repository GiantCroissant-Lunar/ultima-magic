# Issue 5: Implement Random Encounter System

## GitHub Issue Draft

- **Issue title:** Implement random encounter system
- **Suggested labels:** `encounter`, `overworld`, `battle`, `priority-critical`
- **Depends on:** #3
- **Agent handoff:** This issue is scoped so one coding agent can implement it independently once dependencies are done. Preserve placeholder art and temporary data where the acceptance criteria explicitly allow it.

### Suggested starter files

- `project/hosts/complete-app/Scripts/Autoload/EncounterManager.cs`
- `project/hosts/complete-app/Scripts/Data/`
- `project/hosts/complete-app/Scenes/Overworld/`

## Summary

Create a random encounter system that triggers battles while the player walks on the overworld. Different terrain types should have different encounter rates, and the encounter data (which enemies, difficulty) should be configurable per map region.

## Motivation

Random encounters are the bridge between the Ultima-style overworld and the Might & Magic-style battle system. This is a critical system that connects the two halves of the game.

## Acceptance Criteria

- [ ] An `EncounterManager` autoload/singleton is created to manage encounter logic.
- [ ] After each player step on the overworld, a random encounter check is performed.
- [ ] The encounter probability is determined by:
  - **Base encounter rate** (configurable per map/region)
  - **Tile encounter multiplier** (from the tile's custom data, e.g., forest = 2.0×, road = 0.3×, town = 0.0×)
  - **Steps since last encounter** (probability increases with more steps to prevent long dry spells)
- [ ] Encounter zones/regions can be defined to specify which enemy groups appear in which areas.
- [ ] An `EncounterData` resource class stores:
  - List of possible enemy groups
  - Relative weight/probability of each group
  - Minimum and maximum number of enemies
- [ ] An `EncounterZone` resource or node marks regions on the map with their associated `EncounterData`.
- [ ] When an encounter triggers, the system:
  1. Selects an enemy group from the zone's encounter table
  2. Emits a signal (`EncounterTriggered`) with the encounter details
  3. The `SceneManager` picks up the signal to start the battle transition (implemented in Issue #7)
- [ ] A debug overlay (toggled with a key like `F1`) shows:
  - Current encounter probability
  - Steps since last encounter
  - Current encounter zone name
- [ ] Town tiles and safe zones have `0.0` encounter rate (no battles in towns).

## Technical Details

### EncounterManager.cs

```csharp
using Godot;

namespace UltimaMagic.Autoload;

public partial class EncounterManager : Node
{
    [Signal]
    public delegate void EncounterTriggeredEventHandler(EncounterResult encounter);

    [Export] public float BaseEncounterRate { get; set; } = 0.05f; // 5% base per step
    [Export] public int GuaranteedSafeSteps { get; set; } = 5;     // No encounters for first N steps

    private int _stepsSinceLastEncounter;
    private RandomNumberGenerator _rng = new();

    public void OnPlayerStep(Vector2I tilePosition, float tileEncounterMultiplier)
    {
        _stepsSinceLastEncounter++;

        if (_stepsSinceLastEncounter <= GuaranteedSafeSteps) return;
        if (tileEncounterMultiplier <= 0f) return;

        // Increasing probability: base * multiplier * (1 + steps/20)
        float probability = BaseEncounterRate
            * tileEncounterMultiplier
            * (1.0f + (_stepsSinceLastEncounter - GuaranteedSafeSteps) / 20.0f);

        if (_rng.Randf() < probability)
        {
            _stepsSinceLastEncounter = 0;
            var encounter = RollEncounter(/* zone data */);
            EmitSignal(SignalName.EncounterTriggered, encounter);
        }
    }
}
```

### EncounterData Resource

```csharp
using Godot;

namespace UltimaMagic.Data;

[GlobalClass]
public partial class EncounterData : Resource
{
    [Export] public string ZoneName { get; set; } = "Unknown";
    [Export] public EnemyGroup[] PossibleGroups { get; set; }
}

[GlobalClass]
public partial class EnemyGroup : Resource
{
    [Export] public string[] EnemyTypes { get; set; }
    [Export] public int MinCount { get; set; } = 1;
    [Export] public int MaxCount { get; set; } = 3;
    [Export] public float Weight { get; set; } = 1.0f;
}
```

### Integration with Player (Issue #3)

Connect the player's `StepsTaken` signal to the `EncounterManager`:

```csharp
// In Player.cs, emit signal after each step:
[Signal]
public delegate void StepTakenEventHandler(Vector2I tilePosition, float encounterMultiplier);
```

## Labels

`encounter`, `overworld`, `battle`, `priority-critical`
