using System.Collections.ObjectModel;

namespace UltimaMagic.Gameplay;

public sealed class Inventory
{
    private readonly List<IInventoryItem> _items = [];

    public IReadOnlyList<IInventoryItem> Items => new ReadOnlyCollection<IInventoryItem>(_items);

    public void AddItem(IInventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    public bool RemoveItem(IInventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _items.Remove(item);
    }

    public int CountItem(IInventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _items.Count(existingItem =>
            existingItem.GetType() == item.GetType()
            && string.Equals(existingItem.Name, item.Name, StringComparison.Ordinal));
    }
}
