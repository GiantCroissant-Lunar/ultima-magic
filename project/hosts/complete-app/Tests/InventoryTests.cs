using UltimaMagic.Gameplay;

namespace UltimaMagic.Tests;

public sealed class InventoryTests
{
    [Fact]
    public void AddRemoveAndCountItems_TracksDuplicatesByItemIdentity()
    {
        var potion = new TestItem("Potion");
        var secondPotion = new TestItem("Potion");
        var ether = new TestItem("Ether");
        var inventory = new Inventory();

        inventory.AddItem(potion);
        inventory.AddItem(secondPotion);
        inventory.AddItem(ether);

        Assert.Equal(2, inventory.CountItem(potion));
        Assert.Equal(2, inventory.CountItem(secondPotion));
        Assert.Equal(1, inventory.CountItem(ether));
        Assert.True(inventory.RemoveItem(potion));
        Assert.Equal(1, inventory.CountItem(potion));
    }

    [Fact]
    public void Items_ReturnsCachedReadOnlyView()
    {
        var inventory = new Inventory();

        Assert.Same(inventory.Items, inventory.Items);
    }

    [Fact]
    public void RemoveItem_UsesSameIdentityRulesAsCount()
    {
        var inventory = new Inventory();
        inventory.AddItem(new TestItem("Potion"));

        Assert.True(inventory.RemoveItem(new TestItem("Potion")));
        Assert.Equal(0, inventory.CountItem(new TestItem("Potion")));
    }

    private sealed record TestItem(string Name) : IInventoryItem;
}
