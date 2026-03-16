# Issue 8: Design Character Stats and Data Models

## Summary

Implement the player character's RPG data model including stats, equipment, inventory, and leveling. These data models form the backbone of the combat system and progression mechanics.

## Motivation

A well-designed data model is essential before implementing the combat system. The character stats determine damage calculations, turn order, and progression — all core to the RPG experience.

## Acceptance Criteria

- [ ] A `CharacterStats` resource class is created with the following attributes:
  - **Name** (string)
  - **Level** (int, starts at 1)
  - **Experience** (int)
  - **HP** / **MaxHP** (int)
  - **MP** / **MaxMP** (int)
  - **Strength** (int) — affects physical damage
  - **Defense** (int) — reduces incoming physical damage
  - **Intelligence** (int) — affects magic damage and MP
  - **Agility** (int) — affects turn order and dodge chance
  - **Luck** (int) — affects critical hit chance and loot
- [ ] An `Equipment` system is created with slots:
  - Weapon (affects attack damage)
  - Armor (affects defense)
  - Accessory (various bonuses)
- [ ] An `Item` base resource class is created with:
  - Name, Description, Icon (Texture2D)
  - `ItemType` enum: `Consumable`, `Equipment`, `KeyItem`
- [ ] `ConsumableItem` extends `Item` with `Use(CharacterStats target)` for healing potions, etc.
- [ ] `EquipmentItem` extends `Item` with stat modifiers.
- [ ] An `Inventory` class manages a list of items with add/remove/count operations.
- [ ] A **leveling system** is implemented:
  - Experience thresholds for each level (e.g., Level 2 = 100 XP, Level 3 = 300 XP, etc.)
  - Stat increases on level up (configurable per stat)
  - `LevelUp()` method that checks experience and applies stat gains
- [ ] A **damage formula** is defined and documented:
  - Physical: `damage = attacker.EffectiveStrength * attackPower - defender.EffectiveDefense / 2`
  - Magical: `damage = attacker.Intelligence * spellPower - defender.Intelligence * 3 / 10`
  - Critical hit: `damage * 1.5` (chance based on Luck)
  - Minimum damage is always 1 (attacks never deal zero damage)
- [ ] Default starting character stats are defined as a `.tres` resource file.
- [ ] Unit tests or a test scene verifies:
  - Damage calculations produce expected results
  - Level-up correctly increases stats
  - Inventory add/remove works correctly

## Technical Details

### CharacterStats.cs

```csharp
using Godot;

namespace UltimaMagic.Data;

[GlobalClass]
public partial class CharacterStats : Resource
{
    [Export] public string CharacterName { get; set; } = "Hero";
    [Export] public int Level { get; set; } = 1;
    [Export] public int Experience { get; set; } = 0;

    [ExportGroup("Health")]
    [Export] public int Hp { get; set; } = 100;
    [Export] public int MaxHp { get; set; } = 100;
    [Export] public int Mp { get; set; } = 30;
    [Export] public int MaxMp { get; set; } = 30;

    [ExportGroup("Attributes")]
    [Export] public int Strength { get; set; } = 10;
    [Export] public int Defense { get; set; } = 8;
    [Export] public int Intelligence { get; set; } = 6;
    [Export] public int Agility { get; set; } = 7;
    [Export] public int Luck { get; set; } = 5;

    [ExportGroup("Equipment")]
    [Export] public EquipmentItem Weapon { get; set; }
    [Export] public EquipmentItem Armor { get; set; }
    [Export] public EquipmentItem Accessory { get; set; }

    public int EffectiveStrength => Strength + (Weapon?.StrengthBonus ?? 0);
    public int EffectiveDefense => Defense + (Armor?.DefenseBonus ?? 0);

    public bool IsAlive => Hp > 0;

    public void TakeDamage(int amount)
    {
        Hp = Mathf.Max(0, Hp - amount);
    }

    public void Heal(int amount)
    {
        Hp = Mathf.Min(MaxHp, Hp + amount);
    }

    public void RestoreMp(int amount)
    {
        Mp = Mathf.Min(MaxMp, Mp + amount);
    }
}
```

### EquipmentItem.cs

```csharp
using Godot;

namespace UltimaMagic.Data;

[GlobalClass]
public partial class EquipmentItem : Resource
{
    [Export] public string ItemName { get; set; }
    [Export] public string Description { get; set; }
    [Export] public Texture2D Icon { get; set; }

    [Export] public EquipmentSlot Slot { get; set; }

    [Export] public int StrengthBonus { get; set; }
    [Export] public int DefenseBonus { get; set; }
    [Export] public int IntelligenceBonus { get; set; }
    [Export] public int AgilityBonus { get; set; }
    [Export] public int AttackPower { get; set; }

    public enum EquipmentSlot
    {
        Weapon,
        Armor,
        Accessory
    }
}
```

### Damage Calculation

```csharp
namespace UltimaMagic.Battle;

public static class DamageCalculator
{
    public static int CalculatePhysicalDamage(
        CharacterStats attacker, CharacterStats defender, int attackPower)
    {
        int raw = attacker.EffectiveStrength * attackPower
                - defender.EffectiveDefense / 2;
        return Mathf.Max(1, raw); // Minimum 1 damage
    }

    public static int CalculateMagicalDamage(
        CharacterStats attacker, CharacterStats defender, int spellPower)
    {
        int raw = attacker.Intelligence * spellPower
                - defender.Intelligence * 3 / 10;
        return Mathf.Max(1, raw);
    }

    public static bool RollCritical(CharacterStats attacker)
    {
        float chance = attacker.Luck / 100.0f; // 5 Luck = 5% crit
        return GD.Randf() < chance;
    }
}
```

### Experience Table

```csharp
public static class LevelTable
{
    public static int ExperienceForLevel(int level) => level switch
    {
        1 => 0,
        2 => 100,
        3 => 300,
        4 => 600,
        5 => 1000,
        _ => 1000 + (level - 5) * 500
    };
}
```

## Labels

`data-model`, `character`, `rpg`, `priority-critical`
