using Godot;

namespace UltimaMagic.Data;

[GlobalClass]
public partial class ConsumableItem : Item
{
    [Export]
    public int HpRestore { get; set; }

    [Export]
    public int MpRestore { get; set; }

    public ConsumableItem()
    {
        ItemType = ItemType.Consumable;
    }

    public virtual void Use(CharacterStats target)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.Heal(HpRestore);
        target.RestoreMp(MpRestore);
    }
}
