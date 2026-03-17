using Godot;

namespace UltimaMagic.Data;

public enum EquipmentSlot
{
    Weapon,
    Armor,
    Accessory
}

[GlobalClass]
public partial class EquipmentItem : Item
{
    [Export]
    public EquipmentSlot Slot { get; set; }

    [ExportGroup("Combat Bonuses")]
    [Export]
    public int AttackPowerBonus { get; set; }

    [Export]
    public int StrengthBonus { get; set; }

    [Export]
    public int DefenseBonus { get; set; }

    [Export]
    public int IntelligenceBonus { get; set; }

    [Export]
    public int AgilityBonus { get; set; }

    [Export]
    public int LuckBonus { get; set; }

    [ExportGroup("Resource Bonuses")]
    [Export]
    public int MaxHpBonus { get; set; }

    [Export]
    public int MaxMpBonus { get; set; }

    public EquipmentItem()
    {
        ItemType = ItemType.Equipment;
    }
}
