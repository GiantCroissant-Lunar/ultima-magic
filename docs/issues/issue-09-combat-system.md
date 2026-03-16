# Issue 9: Implement Turn-Based Combat System

## GitHub Issue Draft

- **Issue title:** Implement turn-based combat system
- **Suggested labels:** `combat`, `battle`, `turn-based`, `priority-critical`
- **Depends on:** #6, #8
- **Agent handoff:** This issue is scoped so one coding agent can implement it independently once dependencies are done. Preserve placeholder art and temporary data where the acceptance criteria explicitly allow it.

### Suggested starter files

- `project/hosts/complete-app/Scripts/Battle/`
- `project/hosts/complete-app/Scenes/UI/`
- `project/hosts/complete-app/Scenes/Battle/BattleScene.tscn`

## Summary

Create the core turn-based combat loop for the first-person battle scene. The player selects actions from a menu, enemies take their turns, and the battle continues until one side is defeated or the player flees. The player controls a single character.

## Motivation

The combat system is the gameplay heart of the Might & Magic-style battles. A well-designed turn-based loop with clear action menus, feedback, and pacing creates engaging combat encounters.

## Acceptance Criteria

- [ ] A `BattleCombatManager` class manages the battle loop with the following states:
  - `PlayerTurn` — waiting for player input
  - `PlayerAction` — executing the player's chosen action (animation + damage)
  - `EnemyTurn` — enemies execute their actions sequentially
  - `Victory` — all enemies defeated
  - `Defeat` — player HP reaches 0
  - `Fled` — player successfully fled the battle
- [ ] A battle action menu (UI) is displayed during `PlayerTurn` with options:
  - **Attack** — physical attack on a selected enemy
  - **Magic** — opens a sub-menu of available spells (placeholder: 1-2 spells)
  - **Defend** — reduces incoming damage this turn by 50%
  - **Item** — opens inventory to use a consumable item
  - **Flee** — attempt to escape the battle (success based on Agility comparison)
- [ ] The action menu is navigable with keyboard (arrow keys + Enter to select).
- [ ] When **Attack** is selected, the player chooses a target enemy (cycle through alive enemies with left/right arrows).
- [ ] Turn order is determined by **Agility** (higher agility acts first, with some randomness).
- [ ] Each action displays visual feedback:
  - Damage numbers appear briefly on the targeted enemy or player
  - Enemy sprite flashes or shakes when hit
  - A text log at the bottom shows action descriptions (e.g., "Hero attacks Goblin for 15 damage!")
- [ ] After all enemies are defeated:
  - Experience points are awarded and displayed
  - Level-up is checked and shown if applicable
  - Loot/items are shown (if any)
  - Player is returned to the overworld via Scene Transition (Issue #7)
- [ ] If the player is defeated (HP = 0):
  - A "Game Over" message is shown
  - Option to return to title screen or reload last save (placeholder)
- [ ] The **Defend** action sets a flag that halves damage until the player's next turn.
- [ ] The **Flee** action has a success chance based on `player.Agility / averageEnemyAgility`.

## Technical Details

### Battle State Machine

```csharp
using Godot;

namespace UltimaMagic.Battle;

public partial class BattleCombatManager : Node
{
    public enum BattleState
    {
        Start,
        PlayerTurn,
        PlayerAction,
        EnemyTurn,
        Victory,
        Defeat,
        Fled
    }

    public BattleState CurrentState { get; private set; } = BattleState.Start;
    public CharacterStats PlayerStats { get; set; }
    public EnemyBattleData[] Enemies { get; set; }

    private bool _isDefending;
    private int _targetedEnemyIndex;

    public void StartBattle(CharacterStats player, EnemyBattleData[] enemies)
    {
        PlayerStats = player;
        Enemies = enemies;
        DetermineTurnOrder();
        TransitionTo(BattleState.PlayerTurn);
    }

    private void TransitionTo(BattleState newState)
    {
        CurrentState = newState;
        switch (newState)
        {
            case BattleState.PlayerTurn:
                ShowActionMenu();
                break;
            case BattleState.PlayerAction:
                ExecutePlayerAction();
                break;
            case BattleState.EnemyTurn:
                ExecuteEnemyTurns();
                break;
            case BattleState.Victory:
                HandleVictory();
                break;
            case BattleState.Defeat:
                HandleDefeat();
                break;
        }
    }

    private void DetermineTurnOrder()
    {
        // Compare player Agility vs enemy Agility
        // Higher agility + random factor goes first
    }
}
```

### Battle Action Menu (UI)

```
┌─────────────────────────────────┐
│  ┌─────────┐                    │
│  │  ATTACK  │  ← selected       │
│  │  MAGIC   │                    │
│  │  DEFEND  │                    │
│  │  ITEM    │                    │
│  │  FLEE    │                    │
│  └─────────┘                    │
│                                  │
│  Hero  HP: 85/100  MP: 20/30    │
│  ─────────────────────────────  │
│  Goblin attacks Hero for 15!    │
└─────────────────────────────────┘
```

### BattleActionMenu.cs

```csharp
using Godot;

namespace UltimaMagic.UI;

public partial class BattleActionMenu : Control
{
    [Signal]
    public delegate void ActionSelectedEventHandler(BattleAction action);

    public enum BattleAction
    {
        Attack,
        Magic,
        Defend,
        Item,
        Flee
    }

    private int _selectedIndex;
    private readonly BattleAction[] _actions = {
        BattleAction.Attack,
        BattleAction.Magic,
        BattleAction.Defend,
        BattleAction.Item,
        BattleAction.Flee
    };

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("move_up"))
            _selectedIndex = Mathf.Max(0, _selectedIndex - 1);
        if (@event.IsActionPressed("move_down"))
            _selectedIndex = Mathf.Min(_actions.Length - 1, _selectedIndex + 1);
        if (@event.IsActionPressed("interact"))
            EmitSignal(SignalName.ActionSelected, (int)_actions[_selectedIndex]);

        UpdateHighlight();
    }
}
```

### Battle Log

```csharp
using Godot;

namespace UltimaMagic.UI;

public partial class BattleLog : Control
{
    [Export] public RichTextLabel LogLabel { get; set; }

    public void AddMessage(string message)
    {
        LogLabel.Text += message + "\n";
        // Auto-scroll to bottom
    }

    public void Clear()
    {
        LogLabel.Text = "";
    }
}
```

### Victory Flow

1. "Victory!" message displayed
2. Show: "Gained {X} experience points"
3. If level up: "Hero reached Level {N}!" with stat increases
4. If loot: "Found: {item names}"
5. Press any key → transition back to overworld

## Labels

`combat`, `battle`, `turn-based`, `priority-critical`
