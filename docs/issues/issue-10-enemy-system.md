# Issue 10: Create Enemy System with Basic AI

## GitHub Issue Draft

- **Issue title:** Create enemy system with basic AI
- **Suggested labels:** `enemy`, `ai`, `battle`, `data-model`, `priority-critical`
- **Depends on:** #8, #9
- **Agent handoff:** This issue is scoped so one coding agent can implement it independently once dependencies are done. Preserve placeholder art and temporary data where the acceptance criteria explicitly allow it.

### Suggested starter files

- `project/hosts/complete-app/Scripts/Data/`
- `project/hosts/complete-app/Scripts/Battle/`
- `project/hosts/complete-app/Resources/Enemies/`

## Summary

Implement the enemy data model, a bestiary of enemy types, and basic AI behavior for enemies during combat. Enemies should have stats, abilities, sprites, and a simple decision-making system for choosing actions during their turn.

## Motivation

Enemies are the other side of combat. Without varied enemies with different stats and behaviors, battles become repetitive. A well-designed enemy system with basic AI creates interesting tactical decisions for the player.

## Acceptance Criteria

- [ ] An `EnemyData` resource class is created with:
  - **Name** (string)
  - **Stats**: HP, MaxHP, Strength, Defense, Intelligence, Agility (matching player stat structure)
  - **Sprite** (Texture2D) — the enemy's battle sprite
  - **Abilities** — list of actions the enemy can perform
  - **ExperienceReward** (int) — XP given when defeated
  - **LootTable** — possible item drops with drop rates
- [ ] An `EnemyAbility` resource defines enemy actions:
  - Name (e.g., "Bite", "Fireball", "Heal")
  - Type: `Physical`, `Magical`, `Heal`, `Buff`
  - Power (int) — base damage/heal amount
  - Weight (float) — how likely the AI is to choose this ability
- [ ] An `EnemyBattleData` runtime class tracks an enemy's state during battle:
  - Current HP
  - Status effects (placeholder for future)
  - Reference to base `EnemyData`
  - `IsAlive` property
- [ ] A **basic AI system** determines enemy actions each turn:
  - **Random weighted selection** — choose from available abilities based on weights
  - **Low HP healing** — if the enemy has a heal ability and HP < 30%, increase heal weight
  - **Target selection** — currently always targets the player (single character), but structured to support multiple targets later
- [ ] At least **5 enemy types** are created as `.tres` resource files:
  1. **Slime** — low stats, only basic attack, common in starting areas
  2. **Goblin** — moderate stats, has "Stab" (physical) attack
  3. **Wolf** — high agility, has "Bite" (physical) with chance for extra turn
  4. **Skeleton** — balanced stats, has "Slash" and "Bone Throw" (ranged physical)
  5. **Dark Mage** — high intelligence, has "Fireball" (magical) and "Heal Self"
- [ ] Placeholder sprites are created for each enemy (simple colored shapes, 64×64 or 128×128 pixels).
- [ ] A `Bestiary` singleton or resource collection allows looking up enemy data by name/ID.
- [ ] Enemy death removes them from the battle and triggers their defeat animation (from Issue #6).
- [ ] Loot is rolled after battle based on each defeated enemy's loot table.

## Technical Details

### EnemyData.cs

```csharp
using Godot;

namespace UltimaMagic.Data;

[GlobalClass]
public partial class EnemyData : Resource
{
    [Export] public string EnemyName { get; set; } = "Unknown";
    [Export] public Texture2D Sprite { get; set; }

    [ExportGroup("Stats")]
    [Export] public int MaxHp { get; set; } = 20;
    [Export] public int Strength { get; set; } = 5;
    [Export] public int Defense { get; set; } = 3;
    [Export] public int Intelligence { get; set; } = 2;
    [Export] public int Agility { get; set; } = 4;

    [ExportGroup("Rewards")]
    [Export] public int ExperienceReward { get; set; } = 10;
    [Export] public LootEntry[] LootTable { get; set; }

    [ExportGroup("Abilities")]
    [Export] public EnemyAbility[] Abilities { get; set; }
}
```

### EnemyAbility.cs

```csharp
using Godot;

namespace UltimaMagic.Data;

[GlobalClass]
public partial class EnemyAbility : Resource
{
    [Export] public string AbilityName { get; set; } = "Attack";

    [Export] public AbilityType Type { get; set; } = AbilityType.Physical;
    [Export] public int Power { get; set; } = 10;
    [Export] public float AiWeight { get; set; } = 1.0f;

    public enum AbilityType
    {
        Physical,
        Magical,
        Heal,
        Buff
    }
}
```

### EnemyBattleData.cs (Runtime)

```csharp
using Godot;

namespace UltimaMagic.Battle;

public class EnemyBattleData
{
    public EnemyData BaseData { get; }
    public int CurrentHp { get; set; }
    public bool IsAlive => CurrentHp > 0;
    public Texture2D Sprite => BaseData.Sprite;

    public EnemyBattleData(EnemyData data)
    {
        BaseData = data;
        CurrentHp = data.MaxHp;
    }

    public void TakeDamage(int amount)
    {
        CurrentHp = Mathf.Max(0, CurrentHp - amount);
    }
}
```

### EnemyAI.cs

```csharp
using Godot;
using System.Linq;

namespace UltimaMagic.Battle;

public static class EnemyAi
{
    public static EnemyAbility ChooseAction(EnemyBattleData enemy)
    {
        var abilities = enemy.BaseData.Abilities;
        if (abilities == null || abilities.Length == 0)
            return CreateBasicAttack();

        // Adjust weights based on situation
        var weights = abilities.Select(a => AdjustWeight(a, enemy)).ToArray();
        float totalWeight = weights.Sum();

        // Weighted random selection
        float roll = GD.Randf() * totalWeight;
        float cumulative = 0;
        for (int i = 0; i < abilities.Length; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative)
                return abilities[i];
        }

        return abilities[^1]; // Fallback
    }

    private static float AdjustWeight(EnemyAbility ability, EnemyBattleData enemy)
    {
        float weight = ability.AiWeight;

        // Increase heal weight when low HP
        if (ability.Type == EnemyAbility.AbilityType.Heal)
        {
            float hpPercent = (float)enemy.CurrentHp / enemy.BaseData.MaxHp;
            if (hpPercent < 0.3f) weight *= 3.0f;
            else if (hpPercent > 0.7f) weight *= 0.1f; // Don't heal when healthy
        }

        return weight;
    }

    private static EnemyAbility CreateBasicAttack()
    {
        return new EnemyAbility
        {
            AbilityName = "Attack",
            Type = EnemyAbility.AbilityType.Physical,
            Power = 5,
            AiWeight = 1.0f
        };
    }
}
```

### Enemy Stat Blocks

| Enemy | HP | STR | DEF | INT | AGI | XP | Abilities |
|-------|----|-----|-----|-----|-----|----|-----------|
| Slime | 15 | 3 | 2 | 1 | 2 | 5 | Tackle (Phys, 5) |
| Goblin | 25 | 6 | 4 | 2 | 5 | 12 | Stab (Phys, 8) |
| Wolf | 20 | 7 | 3 | 1 | 9 | 15 | Bite (Phys, 10) |
| Skeleton | 30 | 8 | 6 | 3 | 4 | 20 | Slash (Phys, 9), Bone Throw (Phys, 7) |
| Dark Mage | 22 | 3 | 4 | 10 | 5 | 30 | Fireball (Magic, 12), Heal Self (Heal, 15) |

### Bestiary.cs

```csharp
using Godot;
using System.Collections.Generic;

namespace UltimaMagic.Data;

public partial class Bestiary : Node
{
    private Dictionary<string, EnemyData> _enemies = new();

    public override void _Ready()
    {
        LoadAllEnemies();
    }

    private void LoadAllEnemies()
    {
        // Load all .tres files from Resources/Enemies/
        var dir = DirAccess.Open("res://Resources/Enemies/");
        if (dir == null) return;

        dir.ListDirBegin();
        string fileName;
        while ((fileName = dir.GetNext()) != "")
        {
            if (fileName.EndsWith(".tres"))
            {
                var enemy = GD.Load<EnemyData>($"res://Resources/Enemies/{fileName}");
                if (enemy != null)
                    _enemies[enemy.EnemyName] = enemy;
            }
        }
    }

    public EnemyData GetEnemy(string name) => _enemies.GetValueOrDefault(name);
}
```

## Labels

`enemy`, `ai`, `battle`, `data-model`, `priority-critical`
