using Godot;
using UltimaMagic.Gameplay;

namespace UltimaMagic.Data;

[GlobalClass]
public partial class CharacterStats : Resource, ICombatantStats
{
    [Export]
    public string CharacterName { get; set; } = "Hero";

    [Export]
    public int Level { get; set; } = 1;

    [Export]
    public int Experience { get; set; }

    [ExportGroup("Health")]
    [Export]
    public int Hp { get; set; } = 100;

    [Export]
    public int MaxHp { get; set; } = 100;

    [Export]
    public int Mp { get; set; } = 30;

    [Export]
    public int MaxMp { get; set; } = 30;

    [ExportGroup("Attributes")]
    [Export]
    public int Strength { get; set; } = 10;

    [Export]
    public int Defense { get; set; } = 8;

    [Export]
    public int Intelligence { get; set; } = 6;

    [Export]
    public int Agility { get; set; } = 7;

    [Export]
    public int Luck { get; set; } = 5;

    [ExportGroup("Level Growth")]
    [Export]
    public int MaxHpPerLevel { get; set; } = 12;

    [Export]
    public int MaxMpPerLevel { get; set; } = 4;

    [Export]
    public int StrengthPerLevel { get; set; } = 2;

    [Export]
    public int DefensePerLevel { get; set; } = 1;

    [Export]
    public int IntelligencePerLevel { get; set; } = 1;

    [Export]
    public int AgilityPerLevel { get; set; } = 1;

    [Export]
    public int LuckPerLevel { get; set; } = 1;

    [ExportGroup("Equipment")]
    [Export]
    public EquipmentItem? Weapon { get; set; }

    [Export]
    public EquipmentItem? Armor { get; set; }

    [Export]
    public EquipmentItem? Accessory { get; set; }

    public int EffectiveAttackPower => 1 + (Weapon?.AttackPowerBonus ?? 0);

    public int EffectiveStrength => Strength + GetTotalBonus(item => item.StrengthBonus);

    public int EffectiveDefense => Defense + GetTotalBonus(item => item.DefenseBonus);

    public int EffectiveIntelligence => Intelligence + GetTotalBonus(item => item.IntelligenceBonus);

    public int EffectiveAgility => Agility + GetTotalBonus(item => item.AgilityBonus);

    public int EffectiveLuck => Luck + GetTotalBonus(item => item.LuckBonus);

    public int EffectiveMaxHp => MaxHp + GetTotalBonus(item => item.MaxHpBonus);

    public int EffectiveMaxMp => MaxMp + GetTotalBonus(item => item.MaxMpBonus);

    public bool IsAlive => Hp > 0;

    public void TakeDamage(int amount) => Hp = Mathf.Max(0, Hp - Math.Max(0, amount));

    public void Heal(int amount) => Hp = Mathf.Min(EffectiveMaxHp, Hp + Math.Max(0, amount));

    public void RestoreMp(int amount) => Mp = Mathf.Min(EffectiveMaxMp, Mp + Math.Max(0, amount));

    public EquipmentItem? Equip(EquipmentItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var previousItem = item.Slot switch
        {
            EquipmentSlot.Weapon => Weapon,
            EquipmentSlot.Armor => Armor,
            EquipmentSlot.Accessory => Accessory,
            _ => throw new ArgumentOutOfRangeException(nameof(item))
        };

        switch (item.Slot)
        {
            case EquipmentSlot.Weapon:
                Weapon = item;
                break;
            case EquipmentSlot.Armor:
                Armor = item;
                break;
            case EquipmentSlot.Accessory:
                Accessory = item;
                break;
        }

        ClampVitals();
        return previousItem;
    }

    public EquipmentItem? Unequip(EquipmentSlot slot)
    {
        EquipmentItem? removedItem = slot switch
        {
            EquipmentSlot.Weapon => Weapon,
            EquipmentSlot.Armor => Armor,
            EquipmentSlot.Accessory => Accessory,
            _ => throw new ArgumentOutOfRangeException(nameof(slot))
        };

        switch (slot)
        {
            case EquipmentSlot.Weapon:
                Weapon = null;
                break;
            case EquipmentSlot.Armor:
                Armor = null;
                break;
            case EquipmentSlot.Accessory:
                Accessory = null;
                break;
        }

        ClampVitals();
        return removedItem;
    }

    public void AddExperience(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        Experience += amount;
        LevelUp();
    }

    public int LevelUp()
    {
        var progressionState = new CharacterProgressionState
        {
            Level = Level,
            Experience = Experience,
            Hp = Hp,
            MaxHp = MaxHp,
            Mp = Mp,
            MaxMp = MaxMp,
            Strength = Strength,
            Defense = Defense,
            Intelligence = Intelligence,
            Agility = Agility,
            Luck = Luck
        };

        var levelsGained = LevelingSystem.ApplyLevelUps(progressionState, new LevelGrowth
        {
            MaxHp = MaxHpPerLevel,
            MaxMp = MaxMpPerLevel,
            Strength = StrengthPerLevel,
            Defense = DefensePerLevel,
            Intelligence = IntelligencePerLevel,
            Agility = AgilityPerLevel,
            Luck = LuckPerLevel
        });

        Level = progressionState.Level;
        Hp = progressionState.Hp;
        MaxHp = progressionState.MaxHp;
        Mp = progressionState.Mp;
        MaxMp = progressionState.MaxMp;
        Strength = progressionState.Strength;
        Defense = progressionState.Defense;
        Intelligence = progressionState.Intelligence;
        Agility = progressionState.Agility;
        Luck = progressionState.Luck;
        ClampVitals();

        return levelsGained;
    }

    private int GetTotalBonus(Func<EquipmentItem, int> selector)
    {
        return GetEquippedItems().Sum(selector);
    }

    private IEnumerable<EquipmentItem> GetEquippedItems()
    {
        if (Weapon != null)
        {
            yield return Weapon;
        }

        if (Armor != null)
        {
            yield return Armor;
        }

        if (Accessory != null)
        {
            yield return Accessory;
        }
    }

    private void ClampVitals()
    {
        Hp = Mathf.Clamp(Hp, 0, EffectiveMaxHp);
        Mp = Mathf.Clamp(Mp, 0, EffectiveMaxMp);
    }
}
