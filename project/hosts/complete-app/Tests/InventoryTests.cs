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

    private sealed record TestItem(string Name) : IInventoryItem;
}
